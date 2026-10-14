using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinalMVC.Models;
using BCrypt.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ProyectoFinalMVC.Controllers
{
    [Authorize] 
    public class CuentaController : Controller
    {
        private readonly ProyectoFinalMVCContext _context;
        private readonly IWebHostEnvironment _environment;

        public CuentaController(ProyectoFinalMVCContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment; 
        }

        
        // 1. CONSULTAR PERFIL PROPIO (Perfil)
        
        [HttpGet]
        public async Task<IActionResult> Perfil()
        {
            // Obtener usuario autenticado
            var usuario = await ObtenerUsuarioAutenticadoAsync();
            if (usuario == null) return RedirectToAction("Login", "Auth");

           
            return View(usuario);
        }

        
        // 2. EDITAR PERFIL (Nombre de usuario y Email)
        
        [HttpGet]
        public async Task<IActionResult> EditarPerfil()
        {
            var usuario = await ObtenerUsuarioAutenticadoAsync();
            if (usuario == null) return RedirectToAction("Login", "Auth");

            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> EditarPerfil(string NombreUsuario, string Email)
        {
            var usuario = await ObtenerUsuarioAutenticadoAsync();
            if (usuario == null) return RedirectToAction("Login", "Auth");

           
            if (string.IsNullOrWhiteSpace(NombreUsuario) || string.IsNullOrWhiteSpace(Email))
            {
                ViewBag.Error = "Todos los campos son obligatorios.";
                return View(usuario);
            }

            
            var emailExiste = await _context.Usuario
                .AnyAsync(u => u.Email == Email && u.Id_usuario != usuario.Id_usuario);

            if (emailExiste)
            {
                ViewBag.Error = "Este email ya está registrado por otro usuario.";
                return View(usuario);
            }

           
            usuario.NombreUsuario = NombreUsuario;
            usuario.Email = Email;

            _context.Usuario.Update(usuario);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Perfil actualizado correctamente.";
            return RedirectToAction("Perfil");
        }

       
        // 3. CAMBIAR CONTRASEÑA (CambiarContrasena)
        
        [HttpGet]
        public IActionResult CambiarContrasena()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarContrasena(string ContrasenaActual, string NuevaContrasena, string ConfirmarNuevaContrasena)
        {
            var usuario = await ObtenerUsuarioAutenticadoAsync();
            if (usuario == null) return RedirectToAction("Login", "Auth");

           
            if (string.IsNullOrWhiteSpace(ContrasenaActual) || string.IsNullOrWhiteSpace(NuevaContrasena) || string.IsNullOrWhiteSpace(ConfirmarNuevaContrasena))
            {
                ViewBag.Error = "Todos los campos son obligatorios.";
                return View();
            }

           
            bool esContrasenaCorrecta = BCrypt.Net.BCrypt.Verify(ContrasenaActual, usuario.HashContrasena);
            if (!esContrasenaCorrecta)
            {
                ViewBag.Error = "La contraseña actual es incorrecta.";
                return View();
            }

            
            if (NuevaContrasena != ConfirmarNuevaContrasena)
            {
                ViewBag.Error = "La nueva contraseña y la confirmación no coinciden.";
                return View();
            }

           
            usuario.HashContrasena = BCrypt.Net.BCrypt.HashPassword(NuevaContrasena);

            _context.Usuario.Update(usuario);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Contraseña cambiada con éxito.";
            return RedirectToAction("Perfil");
        }

       
        // 4. SUBIR O CAMBIAR FOTO DE PERFIL (CambiarFoto)
        
        [HttpPost]
        [ValidateAntiForgeryToken]    
         public async Task<IActionResult> CambiarFoto(IFormFile archivoFoto)
        {
            var usuario = await ObtenerUsuarioAutenticadoAsync();
            if (usuario == null) return RedirectToAction("Login", "Auth");

            if (archivoFoto == null || archivoFoto.Length == 0)
            {
                TempData["MensajeError"] = "Por favor, selecciona una imagen válida.";
                return RedirectToAction("Perfil");
            }

           
            var extension = Path.GetExtension(archivoFoto.FileName).ToLower();
            var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            if (!extensionesPermitidas.Contains(extension))
            {
                TempData["MensajeError"] = "Formato no permitido. Solo se aceptan imágenes JPG, PNG o GIF.";
                return RedirectToAction("Perfil");
            }

            
            if (archivoFoto.Length > 2 * 1024 * 1024)
            {
                TempData["MensajeError"] = "La imagen no puede pesar más de 2MB.";
                return RedirectToAction("Perfil");
            }

            
            EliminarArchivoFotoFisico(usuario.RutaFoto);

            
            string nombreArchivoUnico = Guid.NewGuid().ToString() + extension;
            string carpetaSubidas = Path.Combine(_environment.WebRootPath, "uploads", "perfiles");
            
            if (!Directory.Exists(carpetaSubidas))
            {
                Directory.CreateDirectory(carpetaSubidas);
            }

            string rutaCompletaServidor = Path.Combine(carpetaSubidas, nombreArchivoUnico);

            
            using (var stream = new FileStream(rutaCompletaServidor, FileMode.Create))
            {
                await archivoFoto.CopyToAsync(stream);
            }

           
            usuario.RutaFoto = "/uploads/perfiles/" + nombreArchivoUnico;
            _context.Usuario.Update(usuario);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Foto de perfil actualizada correctamente.";
            return RedirectToAction("Perfil");
        }

        
        // 5. QUITAR FOTO DE PERFIL
       
        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> QuitarFoto()
        {
            var usuario = await ObtenerUsuarioAutenticadoAsync();
            if (usuario == null) return RedirectToAction("Login", "Auth");

            
            if (!string.IsNullOrEmpty(usuario.RutaFoto))
            {
                
                EliminarArchivoFotoFisico(usuario.RutaFoto);

                
                usuario.RutaFoto = null;
                _context.Usuario.Update(usuario);
                await _context.SaveChangesAsync();

                TempData["MensajeExito"] = "Foto de perfil personalizada eliminada.";
            }
            else
            {
                TempData["MensajeError"] = "No tienes ninguna foto de perfil asignada.";
            }

            return RedirectToAction("Perfil");
        }

        
        // MÉTODOS PRIVADOS DE AYUDA (HELPER METHODS)
       

        
        private async Task<Usuario?> ObtenerUsuarioAutenticadoAsync()
        {
            var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(usuarioIdClaim, out int idUsuario)) return null;

           
            return await _context.Usuario.FirstOrDefaultAsync(u => u.Id_usuario == idUsuario);
        }

        
        private void EliminarArchivoFotoFisico(string? rutaRelativa)
{
    if (!string.IsNullOrEmpty(rutaRelativa))
    {
     
        string rutaFisicaVieja = Path.Combine(_environment.WebRootPath, rutaRelativa.TrimStart('/'));
        
       
        if (System.IO.File.Exists(rutaFisicaVieja))
        {
            System.IO.File.Delete(rutaFisicaVieja);
        }
    }
}
}
}