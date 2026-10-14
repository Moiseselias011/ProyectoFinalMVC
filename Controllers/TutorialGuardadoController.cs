using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinalMVC.Models;

namespace ProyectoFinalMVC.Controllers;

[Authorize]
public class TutorialGuardadoController : Controller
{
    private readonly ProyectoFinalMVCContext _context;
    private const int TamañoPagina = 10;

    public TutorialGuardadoController(ProyectoFinalMVCContext context)
    {
        _context = context;
    }

    private int? ObtenerUsuarioId()
    {
        string? valorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(valorId, out int usuarioId) ? usuarioId : null;
    }

    // 1. Listar los tutoriales guardados en una lista propia.
    [HttpGet]
    public async Task<IActionResult> Index(int idListaPersonal, int pagina = 1)
    {
        int? usuarioId = ObtenerUsuarioId();

        if (usuarioId is null) return Challenge();
        if (idListaPersonal <= 0) return NotFound();

        var lista = await _context.Set<ListaPersonal>()
            .FirstOrDefaultAsync(l => l.IdListaPersonal == idListaPersonal);

        if (lista is null) return NotFound();

        if (lista.DueñoId != usuarioId.Value)
            return Forbid();

        int totalElementos = await _context.Set<TutorialGuardado>()
            .CountAsync(tg => tg.IdListaPersonal == idListaPersonal);

        int totalPaginas = Math.Max(
            1,
            (int)Math.Ceiling(totalElementos / (double)TamañoPagina)
        );

        pagina = Math.Clamp(pagina, 1, totalPaginas);

        var guardados = await _context.Set<TutorialGuardado>()
            .Where(tg => tg.IdListaPersonal == idListaPersonal)
            .Include(tg => tg.Tutorial)
                .ThenInclude(t => t.Usuario)
            .OrderByDescending(tg => tg.FechaGuardado)
            .Skip((pagina - 1) * TamañoPagina)
            .Take(TamañoPagina)
            .ToListAsync();

        ViewBag.ListaPersonal = lista;
        ViewBag.Pagina = pagina;
        ViewBag.TotalPaginas = totalPaginas;
        ViewBag.TotalElementos = totalElementos;

        return View(guardados);
    }

    // 2. Guardar un tutorial en una lista propia.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(int idListaPersonal, int tutorialId)
    {
        int? usuarioId = ObtenerUsuarioId();

        if (usuarioId is null) return Challenge();

        var lista = await _context.Set<ListaPersonal>()
            .FirstOrDefaultAsync(l => l.IdListaPersonal == idListaPersonal);

        if (lista is null) return NotFound();

        if (lista.DueñoId != usuarioId.Value)
            return Forbid();

        bool existeTutorial = await _context.Set<Tutorial>()
            .AnyAsync(t => t.idtutorial == tutorialId);

        if (!existeTutorial) return NotFound();

        bool yaGuardado = await _context.Set<TutorialGuardado>()
            .AnyAsync(tg =>
                tg.IdListaPersonal == idListaPersonal &&
                tg.TutorialId == tutorialId
            );

        if (!yaGuardado)
        {
            var guardado = new TutorialGuardado
            {
                IdListaPersonal = idListaPersonal,
                TutorialId = tutorialId,
                FechaGuardado = DateTime.UtcNow
            };

            _context.Set<TutorialGuardado>().Add(guardado);

            await _context.SaveChangesAsync();
        }

        return RedirectToAction(
            nameof(Index),
            new { idListaPersonal }
        );
    }

    // 3. Quitar un tutorial de una lista sin eliminar el tutorial original.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int idTutorialGuardado)
    {
        int? usuarioId = ObtenerUsuarioId();

        if (usuarioId is null) return Challenge();

        var guardado = await _context.Set<TutorialGuardado>()
            .Include(tg => tg.ListaPersonal)
            .FirstOrDefaultAsync(
                tg => tg.IdTutorialGuardado == idTutorialGuardado
            );

        if (guardado is null) return NotFound();

        if (guardado.ListaPersonal.DueñoId != usuarioId.Value)
            return Forbid();

        int idListaPersonal = guardado.IdListaPersonal;

        _context.Set<TutorialGuardado>().Remove(guardado);

        await _context.SaveChangesAsync();

        return RedirectToAction(
            nameof(Index),
            new { idListaPersonal }
        );
    }
}
