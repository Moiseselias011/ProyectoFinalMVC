using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoFinalMVC.Models;
using ProyectoFinalMVC.Services;

namespace ProyectoFinalMVC.Controllers.Api
{
  
    [Route("api/listas")]
    [ApiController]
    [Authorize]
    public class ListasApiController : ControllerBase
    {
        private readonly IListasServicio _listas;
        private readonly ITutorialGuardadoServicio _guardados;

        public ListasApiController(IListasServicio listas, ITutorialGuardadoServicio guardados)
        {
            _listas = listas;
            _guardados = guardados;
        }

        private int? ObtenerUsuarioId()
        {
            string? valorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(valorId, out int usuarioId) ? usuarioId : null;
        }

       
        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            int? usuarioId = ObtenerUsuarioId();
            if (usuarioId is null) return Unauthorized();

            return Ok(await _listas.ListarPorUsuarioAsync(usuarioId.Value));
        }

       
        [HttpGet("{id}")]
        public async Task<IActionResult> Obtener(int id, int pagina = 1)
        {
            int? usuarioId = ObtenerUsuarioId();
            if (usuarioId is null) return Unauthorized();

            var lista = await _listas.ObtenerPorIdAsync(id);
            if (lista is null) return NotFound(new { Mensaje = "La lista no existe." });
            if (lista.DueñoId != usuarioId.Value) return Forbid();

            const int tamañoPagina = 10;

            var guardados = await _guardados.ListarPorListaAsync(id, pagina, tamañoPagina);
            int total = await _guardados.ContarPorListaAsync(id);

            return Ok(new
            {
                Lista = lista,
                Pagina = pagina,
                TotalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamañoPagina)),
                TotalElementos = total,
                TutorialesGuardados = guardados
            });
        }

       
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] ListaRequest request)
        {
            int? usuarioId = ObtenerUsuarioId();
            if (usuarioId is null) return Unauthorized();

            var (ok, error, lista) = await _listas.CrearAsync(usuarioId.Value, request.Nombre);

            if (!ok) return BadRequest(new { Mensaje = error });

            return Ok(lista);
        }

       
        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, [FromBody] ListaRequest request)
        {
            int? usuarioId = ObtenerUsuarioId();
            if (usuarioId is null) return Unauthorized();

            var (ok, error) = await _listas.EditarAsync(id, usuarioId.Value, request.Nombre);

            if (!ok) return BadRequest(new { Mensaje = error });

            return Ok(new { Mensaje = "La lista fue actualizada." });
        }

  
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            int? usuarioId = ObtenerUsuarioId();
            if (usuarioId is null) return Unauthorized();

            bool eliminado = await _listas.EliminarAsync(id, usuarioId.Value);

            if (!eliminado) return NotFound(new { Mensaje = "La lista no existe o no tienes permiso." });

            return Ok(new { Mensaje = "La lista fue eliminada." });
        }

       
        [HttpPost("{id}/guardados")]
        public async Task<IActionResult> GuardarTutorial(int id, [FromBody] TutorialEnListaRequest request)
        {
            int? usuarioId = ObtenerUsuarioId();
            if (usuarioId is null) return Unauthorized();

            var (ok, error) = await _guardados.GuardarAsync(id, usuarioId.Value, request.TutorialId);

            if (!ok) return BadRequest(new { Mensaje = error });

            return Ok(new { Mensaje = "Tutorial guardado en la lista." });
        }

       
        [HttpDelete("guardados/{idTutorialGuardado}")]
        public async Task<IActionResult> QuitarTutorial(int idTutorialGuardado)
        {
            int? usuarioId = ObtenerUsuarioId();
            if (usuarioId is null) return Unauthorized();

            bool quitado = await _guardados.QuitarAsync(idTutorialGuardado, usuarioId.Value);

            if (!quitado) return NotFound(new { Mensaje = "El tutorial guardado no existe o no tienes permiso." });

            return Ok(new { Mensaje = "El tutorial fue quitado de la lista." });
        }
    }

  
    public class ListaRequest
    {
        public string? Nombre { get; set; }
    }

    public class TutorialEnListaRequest
    {
        public int TutorialId { get; set; }
    }
}