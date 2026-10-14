using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinalMVC.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ProyectoFinalMVC.Controllers
{    
    [Authorize(Roles = Roles.Administrador)]
    public class UsuarioController : Controller
    {
        private readonly ProyectoFinalMVCContext _context;

        public UsuarioController(ProyectoFinalMVCContext context)
        {
            _context = context;
        }
         

         // 1. LISTAR USUARIOS con Paginación (Index)

         public async Task<IActionResult> Index(int pagina = 1)
        {   
            if(pagina < 1) pagina = 1;
            int tamañoPagina = 10;  
            int totalUsuario = await _context.Usuario.CountAsync();
            int totalPaginas = (int)Math.Ceiling(totalUsuario / (double)tamañoPagina);
            
             var usuariosQuery = await _context.Usuario
                .OrderBy(u => u.Id_usuario)
                .Skip((pagina - 1) * tamañoPagina)
                .Take(tamañoPagina)
                .ToListAsync();

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas == 0 ? 1 : totalPaginas;
            return View(usuariosQuery);

        }
         // 2. CONSULTAR INFORMACIÓN (Detalle)
        [HttpGet]

        public async Task<IActionResult> Detalle (int id)
        {
           var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Id_usuario == id); 
              
           if(usuario == null)
            {
                return NotFound();
            }
            return View(usuario);
        }  

       //3. CAMBIAR EL ROL DE UN USUARIO (CambiarRol)
      
      [HttpGet]
        public async Task<IActionResult> CambiarRol(int id)
        {
           
            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Id_usuario == id);
            if (usuario == null)
            {
                return NotFound();
            }

           
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> CambiarRol(int id_usuario, string rol)
        {
           
            if (rol != Roles.Administrador && rol != Roles.Usuario)
            {
                ModelState.AddModelError("Rol", "El rol seleccionado no es válido.");
                
                var usuarioError = await _context.Usuario.FirstOrDefaultAsync(u => u.Id_usuario == id_usuario);
                return View(usuarioError);
            }

            
            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Id_usuario == id_usuario);
            if (usuario == null)
            {
                return NotFound();
            }

            
            var administradorActualId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (usuario.Id_usuario.ToString() == administradorActualId && rol != Roles.Administrador)
            {
                ModelState.AddModelError("Rol", "No puedes quitarte el rol de Administrador a ti mismo.");
                return View(usuario);
            }

            
            usuario.Rol = rol;

            _context.Usuario.Update(usuario);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = $"Rol del usuario '{usuario.NombreUsuario}' actualizado a '{rol}' con éxito.";
            return RedirectToAction("Index");
        }


    }
}