using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinalMVC.Models;
using BCrypt.Net;
using System.Security.Claims; 
using Microsoft.AspNetCore.Authentication; 
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ProyectoFinalMVC.Controllers
{
    public class AuthController : Controller
    {    
         //1. REGISTRO DE USUARIOS

        private readonly ProyectoFinalMVCContext _context;
         public AuthController(ProyectoFinalMVCContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        public ActionResult Registro ()
        {
           return View(); 
        }
        [HttpPost]
        public async Task<ActionResult> Registro  (string NombreUsuario,string Email,string Contrasena,string ConfirmacionDeContrasena)

        {
           if (string.IsNullOrWhiteSpace(NombreUsuario)||string.IsNullOrWhiteSpace(Email)||string.IsNullOrWhiteSpace(Contrasena)||string.IsNullOrWhiteSpace(ConfirmacionDeContrasena))
            {
                ViewBag.Error = "Todos los Campos Son Obligatorios";
                return View();
              
            }
           if(Contrasena != ConfirmacionDeContrasena)
            {
               ViewBag.Error = "las contraseñas deben ser iguales " ;
               return View();
            } 

             var usuarioExistente = await _context.Usuario.FirstOrDefaultAsync(u => u.Email == Email);

            if (usuarioExistente != null)
            {
                ViewBag.Error = "El Email Ya Esta Registrado";
                return View();
            } 
            
            string HashSeguro = BCrypt.Net.BCrypt.HashPassword(Contrasena);
            
            var nuevoUsuario = new Usuario
            {
                NombreUsuario = NombreUsuario,
                Email = Email,
                HashContrasena = HashSeguro,
                FechaCreacion = DateTime.UtcNow

            };

            _context.Usuario.Add(nuevoUsuario); 
             await _context.SaveChangesAsync();

             TempData["Mensaje De Exito"] = "Registro Completado";
             return RedirectToAction ("Login");

        }
        // 2. INICIO DE SESIÓN (LOGIN) 
        [HttpGet]
        public ActionResult Login  ()
        {
            return View();
        }
        
        [HttpPost]

        public async Task<ActionResult> Login (string Email,string Contrasena)
        {   
           if(string.IsNullOrWhiteSpace(Email)||string.IsNullOrWhiteSpace(Contrasena))
           { 
            ViewBag.Error = "Todos los Campos Son Obligatorios";
            return View();
           }
           var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == Email);
           
           if(usuario == null)
            {
                ViewBag.Error = "El Email o Contraseña Incorrecto";
                return View();
            }

           bool esContrasenaCorrecta = BCrypt.Net.BCrypt.Verify(Contrasena,usuario.HashContrasena);
           
           if(!esContrasenaCorrecta)
            {
              ViewBag.Error = "El Email o Contraseña Incorrecto";
              return View();  
            }
            
             var claims = new List<Claim>
             {
               new Claim(ClaimTypes.NameIdentifier,usuario.Id_usuario.ToString()),
               new Claim(ClaimTypes.Name,usuario.NombreUsuario),
               new Claim(ClaimTypes.Email,usuario.Email ),
               new Claim(ClaimTypes.Role,usuario.Rol )

             };

             var identidadUsuario = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

              await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identidadUsuario)
            );
             return RedirectToAction("Index", "Home");

        }
        
            


        //3. CIERRE DE SESIÓN (LOGOUT)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }         



    }
}