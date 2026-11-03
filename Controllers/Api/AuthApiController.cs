using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
        private readonly ITokensServicio _tokens;

        public AuthApiController(IUsuariosServicio usuarios, ITokensServicio tokens)
        {
            _usuarios = usuarios;
            _tokens = tokens;
        }

        
        [AllowAnonymous]
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

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Contrasena))
                return BadRequest(new { Mensaje = "Todos los campos son obligatorios." });

            var usuario = await _usuarios.AutenticarAsync(request.Email, request.Contrasena);

            if (usuario is null)
                return Unauthorized(new { Mensaje = "El email o la contraseña es incorrecto." });

            var (token, expiracion) = _tokens.GenerarToken(usuario);

            return Ok(new
            {
                Mensaje = "Sesión iniciada.",
                Token = token,
                Expiracion = expiracion,
                Usuario = new
                {
                    usuario.Id_usuario,
                    usuario.NombreUsuario,
                    usuario.Email,
                    usuario.Rol
                }
            });
        }

 
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            return Ok(new
            {
                Mensaje = "El token JWT no se almacena en el servidor, así que el cierre de sesión se realiza en el cliente descartando el token. No existe un mecanismo real de invalidación del lado del servidor mientras el token no haya expirado."
            });
        }

     
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
