using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoFinalMVC.Models;
using ProyectoFinalMVC.Services;

namespace ProyectoFinalMVC.Controllers.Api
{
    [Route("api/tutoriales")]
    [ApiController]
    public class TutorialesApiController : ControllerBase
    {
        private readonly ITutorialesServicio _tutoriales;

        public TutorialesApiController(ITutorialesServicio tutoriales)
        {
            _tutoriales = tutoriales;
        }

        private int? ObtenerUsuarioId()
        {
            string? valorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(valorId, out int usuarioId) ? usuarioId : null;
        }

       
        [HttpGet]
        public async Task<IActionResult> Listar(int pagina = 1, int tamañoPagina = 10)
        {
            var tutoriales = await _tutoriales.ListarAsync(pagina, tamañoPagina);
            int total = await _tutoriales.ContarAsync();

            return Ok(new
            {
                Pagina = pagina,
                TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamañoPagina)),
                TotalElementos = total,
                Tutoriales = tutoriales
            });
        }

        
        [HttpGet("{id}")]
        public async Task<IActionResult> Obtener(int id)
        {
            var tutorial = await _tutoriales.ObtenerPorIdAsync(id);

            if (tutorial is null) return NotFound(new { Mensaje = "El tutorial no existe." });

            return Ok(tutorial);
        }

        
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] Tutorial tutorial)
        {
            int? usuarioId = ObtenerUsuarioId();
            if (usuarioId is null) return Unauthorized();

            tutorial.Idusuario = usuarioId.Value;

            var (ok, error) = await _tutoriales.CrearAsync(tutorial);

            if (!ok) return BadRequest(new { Mensaje = error });

            return CreatedAtAction(nameof(Obtener), new { id = tutorial.Idtutorial }, tutorial);
        }

       
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, [FromBody] Tutorial tutorial)
        {
            int? usuarioId = ObtenerUsuarioId();
            if (usuarioId is null) return Unauthorized();

            if (id != tutorial.Idtutorial) return BadRequest(new { Mensaje = "El identificador no coincide." });

            var tutorialActual = await _tutoriales.ObtenerPorIdAsync(id);
            if (tutorialActual is null) return NotFound(new { Mensaje = "El tutorial no existe." });

           
            if (tutorialActual.Idusuario != usuarioId.Value && !User.IsInRole(Roles.Administrador))
                return Forbid();

            var (ok, error) = await _tutoriales.EditarAsync(tutorial);

            if (!ok) return BadRequest(new { Mensaje = error });

            return Ok(tutorialActual);
        }

       
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            int? usuarioId = ObtenerUsuarioId();
            if (usuarioId is null) return Unauthorized();

            var tutorial = await _tutoriales.ObtenerPorIdAsync(id);
            if (tutorial is null) return NotFound(new { Mensaje = "El tutorial no existe." });

            if (tutorial.Idusuario != usuarioId.Value && !User.IsInRole(Roles.Administrador))
                return Forbid();

            await _tutoriales.EliminarAsync(id);

            return Ok(new { Mensaje = "El tutorial fue eliminado." });
        }
    }
}
