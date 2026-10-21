using Microsoft.EntityFrameworkCore;
using ProyectoFinalMVC.Models;

namespace ProyectoFinalMVC.Services;

public class TutorialesServicio : ITutorialesServicio
{
    private readonly ProyectoFinalMVCContext _context;

    public TutorialesServicio(ProyectoFinalMVCContext context)
    {
        _context = context;
    }

    // 1. LISTAR TUTORIALES CON PAGINACION Y FILTROS

    public async Task<List<Tutorial>> ListarAsync(int pagina = 1, int tamañoPagina = 10, string? buscar = null, int? tecnologiaId = null)
    {
        if (pagina < 1) pagina = 1;

        var consulta = _context.Tutorial
            .AsNoTracking()
            .Include(t => t.Usuario)
            .Include(t => t.Tecnologia)
            .Include(t => t.Likes)
            .AsQueryable();

        consulta = AplicarFiltros(consulta, buscar, tecnologiaId);

        return await consulta
            .OrderByDescending(t => t.FechaPublicacion)
            .Skip((pagina - 1) * tamañoPagina)
            .Take(tamañoPagina)
            .ToListAsync();
    }

    public async Task<int> ContarAsync(string? buscar = null, int? tecnologiaId = null)
    {
        var consulta = _context.Tutorial.AsQueryable();

        consulta = AplicarFiltros(consulta, buscar, tecnologiaId);

        return await consulta.CountAsync();
    }

 
    private static IQueryable<Tutorial> AplicarFiltros(IQueryable<Tutorial> consulta, string? buscar, int? tecnologiaId)
    {
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            string texto = buscar.Trim();

            consulta = consulta.Where(t =>
                t.Titulo.Contains(texto) ||
                t.Descripcion.Contains(texto));
        }

        if (tecnologiaId.HasValue && tecnologiaId.Value > 0)
        {
            consulta = consulta.Where(t => t.TecnologiaId == tecnologiaId.Value);
        }

        return consulta;
    }

    // 2. OBTENER UN TUTORIAL

    public async Task<Tutorial?> ObtenerPorIdAsync(int id)
    {
        return await _context.Tutorial
            .AsNoTracking()
            .Include(t => t.Usuario)
            .Include(t => t.Tecnologia)
            .Include(t => t.Likes)
            .Include(t => t.Comentarios)
                .ThenInclude(c => c.Usuario)
            .FirstOrDefaultAsync(t => t.Idtutorial == id);
    }

    // 3. CREAR TUTORIAL

    public async Task<(bool Ok, string? Error)> CrearAsync(Tutorial tutorial)
    {
        if (string.IsNullOrWhiteSpace(tutorial.Titulo))
            return (false, "El título es obligatorio.");

        if (string.IsNullOrWhiteSpace(tutorial.Descripcion))
            return (false, "La descripción es obligatoria.");

        if (string.IsNullOrWhiteSpace(tutorial.Url))
            return (false, "La URL del tutorial es obligatoria.");

        bool existeTecnologia = await _context.Tecnologia
            .AnyAsync(t => t.IdTecnologia == tutorial.TecnologiaId);

        if (!existeTecnologia)
            return (false, "La tecnología seleccionada no existe.");

        tutorial.FechaPublicacion = DateTime.UtcNow;

        
        tutorial.UrlImagen = ObtenerMiniaturaDeYouTube(tutorial.Url);

        _context.Tutorial.Add(tutorial);
        await _context.SaveChangesAsync();

        return (true, null);
    }

    // 4. EDITAR TUTORIAL

    public async Task<(bool Ok, string? Error)> EditarAsync(Tutorial tutorial)
    {
        var tutorialBase = await _context.Tutorial
            .FirstOrDefaultAsync(t => t.Idtutorial == tutorial.Idtutorial);

        if (tutorialBase is null) return (false, "El tutorial no existe.");

        if (string.IsNullOrWhiteSpace(tutorial.Titulo))
            return (false, "El título es obligatorio.");

        if (string.IsNullOrWhiteSpace(tutorial.Descripcion))
            return (false, "La descripción es obligatoria.");

        if (string.IsNullOrWhiteSpace(tutorial.Url))
            return (false, "La URL del tutorial es obligatoria.");

        bool existeTecnologia = await _context.Tecnologia
            .AnyAsync(t => t.IdTecnologia == tutorial.TecnologiaId);

        if (!existeTecnologia)
            return (false, "La tecnología seleccionada no existe.");

        
        tutorialBase.Titulo = tutorial.Titulo;
        tutorialBase.Descripcion = tutorial.Descripcion;
        tutorialBase.Url = tutorial.Url;
        tutorialBase.UrlImagen = ObtenerMiniaturaDeYouTube(tutorial.Url);
        tutorialBase.TecnologiaId = tutorial.TecnologiaId;

        await _context.SaveChangesAsync();

        return (true, null);
    }

    // 5. ELIMINAR TUTORIAL

    public async Task<bool> EliminarAsync(int id)
    {
        var tutorial = await _context.Tutorial
            .FirstOrDefaultAsync(t => t.Idtutorial == id);

        if (tutorial is null) return false;

        _context.Tutorial.Remove(tutorial);
        await _context.SaveChangesAsync();

        return true;
    }

    // 6. MINIATURA DE YOUTUBE

 
    private static string ObtenerMiniaturaDeYouTube(string url)
    {
        string? videoId = ExtraerIdDeYouTube(url);

        if (string.IsNullOrEmpty(videoId))
            return string.Empty;

 
        return $"https://img.youtube.com/vi/{videoId}/hqdefault.jpg";
    }

    
    private static string? ExtraerIdDeYouTube(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;

 
        if (!url.Contains("youtube.com") && !url.Contains("youtu.be")) return null;

        string id = string.Empty;

 
        if (url.Contains("youtu.be/"))
        {
            id = ExtraerEntre(url, "youtu.be/", '?');
        }
 
        else if (url.Contains("watch?v="))
        {
            id = ExtraerEntre(url, "watch?v=", '&');
        }
 
        else if (url.Contains("/embed/"))
        {
            id = ExtraerEntre(url, "/embed/", '?');
        }
        else if (url.Contains("/shorts/"))
        {
            id = ExtraerEntre(url, "/shorts/", '?');
        }

 
        if (id.Length < 6) return null;

        return id;
    }

 
    private static string ExtraerEntre(string texto, string marcador, char fin)
    {
        int inicio = texto.IndexOf(marcador, StringComparison.OrdinalIgnoreCase);

        if (inicio < 0) return string.Empty;

        inicio += marcador.Length;

        int resto = texto.IndexOf(fin, inicio);

        return resto < 0
            ? texto[inicio..]
            : texto[inicio..resto];
    }
}

