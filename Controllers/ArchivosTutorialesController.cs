using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProyectoFinalMVC.Models;
using ProyectoFinalMVC.Services;
using System.Security.Claims;

namespace ProyectoFinalMVC.Controllers;

[Authorize]
[Route("api/archivos")]
[ApiController]
public class ArchivosTutorialesController : ControllerBase
{
    private readonly IArchivoTutorialServicio _archivoServicio;

    public ArchivosTutorialesController(IArchivoTutorialServicio archivoServicio)
    {
        _archivoServicio = archivoServicio;
    }

    private int? ObtenerUsuarioId()
    {
        var valor = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(valor, out int usuarioId) ? usuarioId : null;
    }

    [HttpPost("subir")]
    public async Task<IActionResult> Subir(int idTutorial, IFormFile archivo)
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId is null)
            return Challenge();

        var resultado = await _archivoServicio.SubirAsync(idTutorial, usuarioId.Value, archivo, usuarioId.Value.ToString());

        if (!resultado.Ok)
            return BadRequest(new { error = resultado.Error });

        return Ok(new
        {
            archivo = new
            {
                resultado.Archivo!.IdArchivoTutorial,
                resultado.Archivo.NombreArchivo,
                resultado.Archivo.RutaAlmacenamiento,
                resultado.Archivo.TipoMime,
                resultado.Archivo.TamañoBytes,
                resultado.Archivo.FechaCarga
            }
        });
    }

    [HttpGet("listar/{idTutorial}")]
    public async Task<IActionResult> Listar(int idTutorial)
    {
        var archivos = await _archivoServicio.ListarPorTutorialAsync(idTutorial);
        return Ok(archivos);
    }

    [HttpPost("eliminar/{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var usuarioId = ObtenerUsuarioId();
        if (usuarioId is null)
            return Challenge();

        var resultado = await _archivoServicio.EliminarAsync(id, usuarioId.Value);

        if (!resultado.Ok)
            return StatusCode(403, new { error = resultado.Error });

        return Ok(new { mensaje = resultado.Error == null ? "Archivo eliminado" : resultado.Error });
    }

    [HttpGet("descargar/{id}")]
    public async Task<IActionResult> Descargar(int id)
    {
        var (ok, error, rutaAbsoluta, nombreDescarga, tipoMime) =
            await _archivoServicio.ObtenerParaDescargaAsync(id);

        if (!ok || string.IsNullOrEmpty(rutaAbsoluta))
            return NotFound(new { error = error ?? "El archivo no está disponible." });

        return PhysicalFile(rutaAbsoluta, tipoMime!, nombreDescarga!);
    }
}
