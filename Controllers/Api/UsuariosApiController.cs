using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoFinalMVC.Models;
using ProyectoFinalMVC.Services;

namespace ProyectoFinalMVC.Controllers.Api
{
   
    [Route("api/usuarios")]
    [ApiController]
    [Authorize(Roles = Roles.Administrador)]
    public class UsuariosApiController : ControllerBase
    {
        private readonly IUsuariosServicio _usuarios;

        public UsuariosApiController(IUsuariosServicio usuarios)
        {
            _usuarios = usuarios;
        }

        private int? ObtenerUsuarioId()
        {
            string? valorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(valorId, out int usuarioId) ? usuarioId : null;
        }

      
        [HttpGet]
        public async Task<IActionResult> Listar(int pagina = 1)
        {
            const int tamañoPagina = 10;

            var usuarios = await _usuarios.ListarAsync(pagina, tamañoPagina);
            int total = await _usuarios.ContarAsync();

            return Ok(new
            {
                Pagina = pagina,
                TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamañoPagina)),
                TotalElementos = total,
                Usuarios = usuarios
            });
        }

        
        [HttpGet("{id}")]
        public async Task<IActionResult> Obtener(int id)
        {
            var usuario = await _usuarios.ObtenerPorIdAsync(id);

            if (usuario is null) return NotFound(new { Mensaje = "El usuario no existe." });

            return Ok(usuario);
        }

       
        [HttpPut("{id}/rol")]
        public async Task<IActionResult> CambiarRol(int id, [FromBody] RolRequest request)
        {
            int? administradorId = ObtenerUsuarioId();
            if (administradorId is null) return Unauthorized();

            var (ok, error) = await _usuarios.CambiarRolAsync(id, request.Rol ?? string.Empty, administradorId.Value);

            if (!ok) return BadRequest(new { Mensaje = error });

            return Ok(new { Mensaje = "El rol fue actualizado." });
        }

        
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            int? administradorId = ObtenerUsuarioId();
            if (administradorId is null) return Unauthorized();

            bool eliminado = await _usuarios.EliminarAsync(id, administradorId.Value);

            if (!eliminado) return BadRequest(new { Mensaje = "No puedes eliminar tu propia cuenta de administrador." });

            return Ok(new { Mensaje = "La cuenta y sus contenidos relacionados fueron eliminados." });
        }
    }

    
    public class RolRequest
    {
        public string? Rol { get; set; }
    }
}
