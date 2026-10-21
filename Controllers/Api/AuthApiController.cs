using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ProyectoFinalMVC.Models;
using ProyectoFinalMVC.Services;
using System.Security.Claims;

namespace ProyectoFinalMVC.Controllers.Api
{
   
    [Route("api/auth")]
    [ApiController]
    public class AuthApiController : ControllerBase
    {
        private readonly IUsuariosServicio _usuarios;

        public AuthApiController(IUsuariosServicio usuarios)
        {
            _usuarios = usuarios;
        }

       
        [HttpPost("registro")]
        public async Task<IActionResult> Registro([FromBody] RegistroRequest request)
        {
            var (ok, error) = await _usuarios.RegistrarAsync(
                request.NombreUsuario ?? string.Empty,
                request.Email ?? string.Empty,
                request.Contrasena ?? string.Empty);

            if (!ok) return BadRequest(new { Mensaje = error });

            return Ok(new { Mensaje = "Registro completado." });
        }

   
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Contrasena))
                return BadRequest(new { Mensaje = "Todos los campos son obligatorios." });

            var usuario = await _usuarios.AutenticarAsync(request.Email, request.Contrasena);

            if (usuario is null)
                return Unauthorized(new { Mensaje = "El email o la contraseña es incorrecto." });

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id_usuario.ToString()),
                new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            var identidadUsuario = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identidadUsuario));

            return Ok(new
            {
                Mensaje = "Sesión iniciada.",
                Usuario = new
                {
                    usuario.Id_usuario,
                    usuario.NombreUsuario,
                    usuario.Email,
                    usuario.Rol
                }
            });
        }

 
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Ok(new { Mensaje = "Sesión cerrada." });
        }

     
        [HttpGet("perfil")]
        public async Task<IActionResult> Perfil()
        {
            string? valorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(valorId, out int usuarioId)) return Unauthorized();

            var usuario = await _usuarios.ObtenerPorIdAsync(usuarioId);

            if (usuario is null) return NotFound(new { Mensaje = "El usuario no existe." });

            return Ok(usuario);
        }
    }

    public class RegistroRequest
    {
        public string? NombreUsuario { get; set; }
        public string? Email { get; set; }
        public string? Contrasena { get; set; }
    }

    public class LoginRequest
    {
        public string? Email { get; set; }
        public string? Contrasena { get; set; }
    }
}
