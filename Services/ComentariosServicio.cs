using Microsoft.EntityFrameworkCore;
using ProyectoFinalMVC.Models;

namespace ProyectoFinalMVC.Services;

public class ComentariosServicio : IComentariosServicio
{
    private readonly ProyectoFinalMVCContext _context;

    public ComentariosServicio(ProyectoFinalMVCContext context)
    {
        _context = context;
    }

    // 1. LISTAR COMENTARIOS DE UN TUTORIAL

    public async Task<List<Comentario>> ListarPorTutorialAsync(int idTutorial, int pagina = 1, int tamañoPagina = 10)
    {
        if (pagina < 1) pagina = 1;

        return await _context.Comentario
            .AsNoTracking()
            .Where(c =>
                c.idtutorial == idTutorial &&
                c.ComentarioPadreId == null)
            .Include(c => c.Usuario)
            .Include(c => c.Likes)
            .Include(c => c.Respuestas)
                .ThenInclude(r => r.Usuario)
            .Include(c => c.Respuestas)
                .ThenInclude(r => r.Likes)
            .OrderByDescending(c => c.FechaCreacion)
            .Skip((pagina - 1) * tamañoPagina)
            .Take(tamañoPagina)
            .ToListAsync();
    }

    public async Task<int> ContarPorTutorialAsync(int idTutorial)
    {
        return await _context.Comentario
            .CountAsync(c =>
                c.idtutorial == idTutorial &&
                c.ComentarioPadreId == null);
    }

    // 2. CREAR COMENTARIO

    public async Task<(bool Ok, string? Error, int? IdCreado)> CrearAsync(int idTutorial, int idUsuario, string? contenido)
    {
        bool tutorialExiste = await _context.Tutorial
            .AnyAsync(t => t.Idtutorial == idTutorial);

        if (!tutorialExiste)
            return (false, "El tutorial no existe.", null);

        contenido = contenido?.Trim();

        if (string.IsNullOrWhiteSpace(contenido) || contenido.Length > 1000)
            return (false, "El comentario es obligatorio y no puede superar los 1000 caracteres.", null);

        var comentario = new Comentario
        {
            idtutorial = idTutorial,
            Idusuario = idUsuario,
            Contenido = contenido,
            FechaCreacion = DateTime.UtcNow
        };

        _context.Comentario.Add(comentario);
        await _context.SaveChangesAsync();

        return (true, null, comentario.Idcomentario);
    }

    // 3. RESPONDER A UN COMENTARIO

    public async Task<(bool Ok, string? Error, int? IdCreado)> ResponderAsync(int idTutorial, int idUsuario, int comentarioPadreId, string? contenido)
    {
        var comentarioPadre = await _context.Comentario
            .FirstOrDefaultAsync(c => c.Idcomentario == comentarioPadreId);

        if (comentarioPadre is null)
            return (false, "El comentario al que respondes no existe.", null);

        if (comentarioPadre.idtutorial != idTutorial)
            return (false, "El comentario no pertenece a ese tutorial.", null);

        contenido = contenido?.Trim();

        if (string.IsNullOrWhiteSpace(contenido) || contenido.Length > 1000)
            return (false, "La respuesta es obligatoria y no puede superar los 1000 caracteres.", null);

        var respuesta = new Comentario
        {
            idtutorial = idTutorial,
            ComentarioPadreId = comentarioPadreId,
            Idusuario = idUsuario,
            Contenido = contenido,
            FechaCreacion = DateTime.UtcNow
        };

        _context.Comentario.Add(respuesta);
        await _context.SaveChangesAsync();

        return (true, null, respuesta.Idcomentario);
    }

    // 4. EDITAR COMENTARIO

    public async Task<(bool Ok, string? Error)> EditarAsync(int id, int idUsuario, string? contenido, bool esAdministrador)
    {
        var comentario = await _context.Comentario
            .FirstOrDefaultAsync(c => c.Idcomentario == id);

        if (comentario is null) return (false, "El comentario no existe.");

        if (comentario.Idusuario != idUsuario && !esAdministrador)
            return (false, "No tienes permiso para editar este comentario.");

        contenido = contenido?.Trim();

        if (string.IsNullOrWhiteSpace(contenido) || contenido.Length > 1000)
            return (false, "El contenido es obligatorio y no puede superar los 1000 caracteres.");

        comentario.Contenido = contenido;

        await _context.SaveChangesAsync();

        return (true, null);
    }

    // 5. ELIMINAR COMENTARIO (las respuestas se conservan)

    public async Task<bool> EliminarAsync(int id, int idUsuario, bool esAdministrador)
    {
        var comentario = await _context.Comentario
            .FirstOrDefaultAsync(c => c.Idcomentario == id);

        if (comentario is null) return false;

        if (comentario.Idusuario != idUsuario && !esAdministrador) return false;

       
        var respuestas = await _context.Comentario
            .Where(c => c.ComentarioPadreId == id)
            .ToListAsync();

        foreach (var respuesta in respuestas)
        {
            respuesta.ComentarioPadreId = null;
        }

        // Se eliminan los likes del comentario.
        var likes = await _context.LikeDeComentario
            .Where(l => l.ComentarioId == id)
            .ToListAsync();

        _context.LikeDeComentario.RemoveRange(likes);

        _context.Comentario.Remove(comentario);

        await _context.SaveChangesAsync();

        return true;
    }
}
