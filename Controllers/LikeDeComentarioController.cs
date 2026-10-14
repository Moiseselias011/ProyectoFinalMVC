using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinalMVC.Models;

namespace ProyectoFinalMVC.Controllers;

[Authorize]
public class LikeDeComentarioController : Controller
{
    private readonly ProyectoFinalMVCContext _context;

    public LikeDeComentarioController(ProyectoFinalMVCContext context)
    {
        _context = context;
    }

    private int? ObtenerUsuarioId()
    {
        string? valorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(valorId, out int usuarioId) ? usuarioId : null;
    }

    // Agregar un Me gusta a un comentario.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(int comentarioId)
    {
        int? usuarioId = ObtenerUsuarioId();

        if (usuarioId is null) return Challenge();
        if (comentarioId <= 0) return BadRequest();

        bool existeComentario = await _context.Set<Comentario>()
            .AnyAsync(c => c.Idcomentario == comentarioId);

        if (!existeComentario) return NotFound();

        bool yaExiste = await _context.Set<LikeDeComentario>()
            .AnyAsync(l =>
                l.UsuarioId == usuarioId.Value &&
                l.ComentarioId == comentarioId
            );

        if (!yaExiste)
        {
            var like = new LikeDeComentario
            {
                UsuarioId = usuarioId.Value,
                ComentarioId = comentarioId,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Set<LikeDeComentario>().Add(like);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index", "Home");
    }

    // Eliminar el Me gusta propio de un comentario.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int comentarioId)
    {
        int? usuarioId = ObtenerUsuarioId();

        if (usuarioId is null) return Challenge();
        if (comentarioId <= 0) return BadRequest();

        var like = await _context.Set<LikeDeComentario>()
            .FirstOrDefaultAsync(l =>
                l.UsuarioId == usuarioId.Value &&
                l.ComentarioId == comentarioId
            );

        if (like is not null)
        {
            _context.Set<LikeDeComentario>().Remove(like);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index", "Home");
    }
}
