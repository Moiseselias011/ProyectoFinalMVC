using Microsoft.EntityFrameworkCore;
using ProyectoFinalMVC.Models;

namespace ProyectoFinalMVC.Services;

public class LikesServicio : ILikesServicio
{
    private readonly ProyectoFinalMVCContext _context;

    public LikesServicio(ProyectoFinalMVCContext context)
    {
        _context = context;
    }

    // 1. LIKES DE TUTORIALES

    public async Task<bool> AgregarLikeTutorialAsync(int usuarioId, int tutorialId)
    {
        bool existeTutorial = await _context.Tutorial
            .AnyAsync(t => t.Idtutorial == tutorialId);

        if (!existeTutorial) return false;

        bool yaExiste = await _context.Like
            .AnyAsync(l =>
                l.UsuarioId == usuarioId &&
                l.TutorialId == tutorialId);

        if (yaExiste) return true;

        var like = new Like
        {
            UsuarioId = usuarioId,
            TutorialId = tutorialId,
            FechaCreacion = DateTime.UtcNow
        };

        _context.Like.Add(like);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> QuitarLikeTutorialAsync(int usuarioId, int tutorialId)
    {
        var like = await _context.Like
            .FirstOrDefaultAsync(l =>
                l.UsuarioId == usuarioId &&
                l.TutorialId == tutorialId);

        if (like is null) return false;

        _context.Like.Remove(like);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<int> ContarLikesTutorialAsync(int tutorialId)
    {
        return await _context.Like
            .CountAsync(l => l.TutorialId == tutorialId);
    }

    public async Task<bool> LeGustaTutorialAsync(int usuarioId, int tutorialId)
    {
        return await _context.Like
            .AnyAsync(l =>
                l.UsuarioId == usuarioId &&
                l.TutorialId == tutorialId);
    }

    // 2. LIKES DE COMENTARIOS

    public async Task<bool> AgregarLikeComentarioAsync(int usuarioId, int comentarioId)
    {
        bool existeComentario = await _context.Comentario
            .AnyAsync(c => c.Idcomentario == comentarioId);

        if (!existeComentario) return false;

        bool yaExiste = await _context.LikeDeComentario
            .AnyAsync(l =>
                l.UsuarioId == usuarioId &&
                l.ComentarioId == comentarioId);

        if (yaExiste) return true;

        var like = new LikeDeComentario
        {
            UsuarioId = usuarioId,
            ComentarioId = comentarioId,
            FechaCreacion = DateTime.UtcNow
        };

        _context.LikeDeComentario.Add(like);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> QuitarLikeComentarioAsync(int usuarioId, int comentarioId)
    {
        var like = await _context.LikeDeComentario
            .FirstOrDefaultAsync(l =>
                l.UsuarioId == usuarioId &&
                l.ComentarioId == comentarioId);

        if (like is null) return false;

        _context.LikeDeComentario.Remove(like);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<int> ContarLikesComentarioAsync(int comentarioId)
    {
        return await _context.LikeDeComentario
            .CountAsync(l => l.ComentarioId == comentarioId);
    }
}

