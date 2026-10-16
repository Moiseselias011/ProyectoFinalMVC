using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinalMVC.Models;

namespace ProyectoFinalMVC.Controllers;

[Authorize]
public class LikeController : Controller
{
    private readonly ProyectoFinalMVCContext _context;

    public LikeController(ProyectoFinalMVCContext context)
    {
        _context = context;
    }

    private int? ObtenerUsuarioId()
    {
        string? valorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(valorId, out int usuarioId) ? usuarioId : null;
    }

    // Agregar un Me gusta a un tutorial.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(int tutorialId)
    {
        int? usuarioId = ObtenerUsuarioId();

        if (usuarioId is null) return Challenge();
        if (tutorialId <= 0) return BadRequest();

        bool existeTutorial = await _context.Set<Tutorial>()
            .AnyAsync(t => t.Idtutorial == tutorialId);

        if (!existeTutorial) return NotFound();

        bool yaExiste = await _context.Set<Like>()
            .AnyAsync(l =>
                l.UsuarioId == usuarioId.Value &&
                l.TutorialId == tutorialId
            );

        if (!yaExiste)
        {
            var like = new Like
            {
                UsuarioId = usuarioId.Value,
                TutorialId = tutorialId,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Set<Like>().Add(like);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index", "Home");
    }

    // Eliminar el Me gusta del usuario autenticado.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int tutorialId)
    {
        int? usuarioId = ObtenerUsuarioId();

        if (usuarioId is null) return Challenge();
        if (tutorialId <= 0) return BadRequest();

        var like = await _context.Set<Like>()
            .FirstOrDefaultAsync(l =>
                l.UsuarioId == usuarioId.Value &&
                l.TutorialId == tutorialId
            );

        if (like is not null)
        {
            _context.Set<Like>().Remove(like);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index", "Home");
    }
}
