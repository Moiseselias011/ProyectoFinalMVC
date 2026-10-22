using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoFinalMVC.Models;
using ProyectoFinalMVC.Services;

namespace ProyectoFinalMVC.Controllers.Api
{

    [Route("api/comentarios")]
    [ApiController]
    public class ComentariosApiController : ControllerBase
    {
        private readonly IComentariosServicio _comentarios;

        public ComentariosApiController(IComentariosServicio comentarios)
        {
            _comentarios = comentarios;
        }

        private int? ObtenerUsuarioId()
        {
            string? valorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(valorId, out int usuarioId) ? usuarioId : null;
        }

       
        [HttpGet("{idTutorial}")]
        public async Task<IActionResult> Listar(int idTutorial, int pagina = 1)
        {
            const int tamañoPagina = 10;

            var comentarios = await _comentarios.ListarPorTutorialAsync(idTutorial, pagina, tamañoPagina);
            int total = await _comentarios.ContarPorTutorialAsync(idTutorial);

            return Ok(new
            {
                IdTutorial = idTutorial,
                Pagina = pagina,
                TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamañoPagina)),
                TotalElementos = total,
                Comentarios = comentarios
            });
        }

       
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] ComentarioRequest request)
        {
            int? usuarioId = ObtenerUsuarioId();
            if (usuarioId is null) return Unauthorized();

            var (ok, error, idCreado) = await _comentarios.CrearAsync(
                request.IdTutorial,
                usuarioId.Value,
                request.Contenido);

            if (!ok) return BadRequest(new { Mensaje = error });

            return Ok(new { Mensaje = "Comentario creado.", IdComentario = idCreado });
        }

       
        [Authorize]
        [HttpPost("{id}/responder")]
        public async Task<IActionResult> Responder(int id, [FromBody] ComentarioRequest request)
        {
            int? usuarioId = ObtenerUsuarioId();
            if (usuarioId is null) return Unauthorized();

            var (ok, error, idCreado) = await _comentarios.ResponderAsync(
                request.IdTutorial,
                usuarioId.Value,
                id,
                request.Contenido);

            if (!ok) return BadRequest(new { Mensaje = error });

            return Ok(new { Mensaje = "Respuesta creada.", IdComentario = idCreado });
        }

      
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, [FromBody] ContenidoRequest request)
        {
            int? usuarioId = ObtenerUsuarioId();
            if (usuarioId is null) return Unauthorized();

            bool esAdministrador = User.IsInRole(Roles.Administrador);

            var (ok, error) = await _comentarios.EditarAsync(id, usuarioId.Value, request.Contenido, esAdministrador);

            if (!ok) return BadRequest(new { Mensaje = error });

            return Ok(new { Mensaje = "Comentario actualizado." });
        }

       
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            int? usuarioId = ObtenerUsuarioId();
            if (usuarioId is null) return Unauthorized();

            bool esAdministrador = User.IsInRole(Roles.Administrador);

            bool eliminado = await _comentarios.EliminarAsync(id, usuarioId.Value, esAdministrador);

            if (!eliminado) return NotFound(new { Mensaje = "El comentario no existe o no tienes permiso." });

            return Ok(new { Mensaje = "Comentario eliminado." });
        }
    }

   
    public class ComentarioRequest
    {
        public int IdTutorial { get; set; }
        public string? Contenido { get; set; }
    }

    public class ContenidoRequest
    {
        public string? Contenido { get; set; }
    }
}