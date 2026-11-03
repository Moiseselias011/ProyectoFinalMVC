using ProyectoFinalMVC.Models;

namespace ProyectoFinalMVC.Services;

public interface IArchivoTutorialServicio
{
    Task<(bool Ok, string? Error, ArchivoTutorial? Archivo)> SubirAsync(int idTutorial, int idUsuario, IFormFile archivo, string usuarioActual);

    Task<List<ArchivoTutorial>> ListarPorTutorialAsync(int idTutorial);

    Task<(bool Ok, string? Error)> EliminarAsync(int idArchivo, int idUsuario);

    Task<(bool Ok, string? Error, string? RutaAbsoluta, string? NombreDescarga, string? TipoMime)> ObtenerParaDescargaAsync(int idArchivo);

    Task<(bool Ok, string? Error)> ValidarExtensionYTamañoAsync(IFormFile archivo, long maxBytes);
}
