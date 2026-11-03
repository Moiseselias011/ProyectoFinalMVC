using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ProyectoFinalMVC.Models;
using System.IO;
using System.Threading.Tasks;

namespace ProyectoFinalMVC.Services;

public class ArchivoTutorialServicio : IArchivoTutorialServicio
{
    private readonly ProyectoFinalMVCContext _context;
    private readonly IWebHostEnvironment _entorno;
    private const string CarpetaAlmacenamiento = "wwwroot/files/tutoriales";
    private const long MaxTamañoBytes = 5 * 1024 * 1024;
    private readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".zip", ".doc", ".docx" };

    public ArchivoTutorialServicio(ProyectoFinalMVCContext context, IWebHostEnvironment entorno)
    {
        _context = context;
        _entorno = entorno;
        Directory.CreateDirectory(ObtenerRutaAbsoluta(CarpetaAlmacenamiento));
    }

    private string ObtenerRutaAbsoluta(string rutaRelativa)
    {
        var raizWebroot = _entorno.WebRootPath;
        if (string.IsNullOrEmpty(raizWebroot))
        {
            raizWebroot = Path.Combine(_entorno.ContentRootPath, "wwwroot");
        }

        var normalizada = rutaRelativa.Replace("wwwroot/", "").Replace("wwwroot\\", "");
        return Path.Combine(raizWebroot, normalizada.Replace('/', Path.DirectorySeparatorChar));
    }

    private string GenerarRutaAlmacenamiento(IFormFile archivo)
    {
        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        var nombreGuid = Guid.NewGuid().ToString("N");
        var nombreAlmacenado = $"{nombreGuid}{extension}";
        return Path.Combine(CarpetaAlmacenamiento, nombreAlmacenado);
    }

    private bool EsExtensionPermitida(IFormFile archivo)
    {
        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        return ExtensionesPermitidas.Contains(extension);
    }

    public async Task<(bool Ok, string? Error, ArchivoTutorial? Archivo)> SubirAsync(int idTutorial, int idUsuario, IFormFile archivo, string usuarioActual)
    {
        if (archivo is null || archivo.Length == 0)
            return (false, "El archivo está vacío o es nulo", null);

        if (!EsExtensionPermitida(archivo))
        {
            return (false, "Extensión de archivo no permitida. Permitidas: " + string.Join(", ", ExtensionesPermitidas), null);
        }

        if (archivo.Length > MaxTamañoBytes)
        {
            return (false, $"El archivo excede el límite de {MaxTamañoBytes / (1024 * 1024)} MB", null);
        }

        var tutorial = await _context.Tutorial.FindAsync(idTutorial);
        if (tutorial is null)
            return (false, "El tutorial especificado no existe", null);

        bool esAutor = tutorial.Idusuario == idUsuario;
        esAutor |= await _context.Usuario.AnyAsync(u => u.Id_usuario == idUsuario && u.Rol == "Administrador");

        if (!esAutor)
            return (false, "No tiene permiso para subir archivos a este tutorial", null);

        var rutaArchivo = GenerarRutaAlmacenamiento(archivo);
        var directorio = Path.GetDirectoryName(ObtenerRutaAbsoluta(rutaArchivo));
        if (!string.IsNullOrEmpty(directorio))
        {
            Directory.CreateDirectory(directorio);
        }

        using (var stream = new FileStream(ObtenerRutaAbsoluta(rutaArchivo), FileMode.Create))
        {
            await archivo.CopyToAsync(stream);
        }

        var archivoRegistro = new ArchivoTutorial
        {
            IdTutorial = idTutorial,
            NombreArchivo = archivo.FileName,
            RutaAlmacenamiento = rutaArchivo,
            TipoMime = archivo.ContentType,
            TamañoBytes = archivo.Length
        };

        _context.ArchivoTutorial.Add(archivoRegistro);
        await _context.SaveChangesAsync();

        return (true, null, archivoRegistro);
    }

    public async Task<List<ArchivoTutorial>> ListarPorTutorialAsync(int idTutorial)
    {
        return await _context.ArchivoTutorial
            .Where(a => a.IdTutorial == idTutorial)
            .OrderByDescending(a => a.FechaCarga)
            .ToListAsync();
    }

    public async Task<(bool Ok, string? Error)> EliminarAsync(int idArchivo, int idUsuario)
    {
        var archivo = await _context.ArchivoTutorial
            .Include(a => a.Tutorial)
            .FirstOrDefaultAsync(a => a.IdArchivoTutorial == idArchivo);

        if (archivo is null)
            return (false, "El archivo no existe");

        bool esQuienSubio = archivo.Tutorial.Idusuario == idUsuario;
        esQuienSubio |= await _context.Usuario.AnyAsync(u => u.Id_usuario == idUsuario && u.Rol == "Administrador");

        if (!esQuienSubio)
            return (false, "No tiene permiso para eliminar este archivo");

        var rutaFisica = ObtenerRutaAbsoluta(archivo.RutaAlmacenamiento);
        if (System.IO.File.Exists(rutaFisica))
        {
            try
            {
                System.IO.File.Delete(rutaFisica);
            }
            catch
            {
            }
        }

        _context.ArchivoTutorial.Remove(archivo);
        await _context.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Ok, string? Error, string? RutaAbsoluta, string? NombreDescarga, string? TipoMime)> ObtenerParaDescargaAsync(int idArchivo)
    {
        var archivo = await _context.ArchivoTutorial
            .FirstOrDefaultAsync(a => a.IdArchivoTutorial == idArchivo);

        if (archivo is null)
            return (false, "El archivo no existe", null, null, null);

        var rutaAbsoluta = ObtenerRutaAbsoluta(archivo.RutaAlmacenamiento);

        if (!System.IO.File.Exists(rutaAbsoluta))
            return (false, "El archivo físico ya no está disponible", null, null, null);

        var nombreDescarga = Path.GetFileName(archivo.NombreArchivo);
        var mime = string.IsNullOrWhiteSpace(archivo.TipoMime)
            ? "application/octet-stream"
            : archivo.TipoMime;

        return (true, null, rutaAbsoluta, nombreDescarga, mime);
    }

    public async Task<(bool Ok, string? Error)> ValidarExtensionYTamañoAsync(IFormFile archivo, long maxBytes)
    {
        if (archivo is null || archivo.Length == 0)
            return (false, "El archivo está vacío o es nulo");

        if (!EsExtensionPermitida(archivo))
            return (false, "Extensión de archivo no permitida");

        if (archivo.Length > maxBytes)
            return (false, $"El archivo excede el límite de {maxBytes / (1024 * 1024)} MB");

        return (true, null);
    }
}
