
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinalMVC.Models;

namespace ProyectoFinalMVC.Controllers;

public class ComentarioController : Controller
{
    private readonly ProyectoFinalMVCContext _context;
    private const int TamañoPagina = 10;

    public ComentarioController(ProyectoFinalMVCContext context)
    {
        _context = context;
    }

    private int? ObtenerUsuarioId()
    {
        string? valorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(valorId, out int usuarioId)
            ? usuarioId
            : null;
    }

    // Listar los comentarios principales de un tutorial.
    // Las respuestas se incluyen dentro de cada comentario.
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Index(int idTutorial, int pagina = 1)
    {
        if (idTutorial <= 0)
            return NotFound();

        var tutorial = await _context.Set<Tutorial>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Idtutorial == idTutorial);

        if (tutorial is null)
            return NotFound();

        int totalElementos = await _context.Set<Comentario>()
            .CountAsync(c =>
                c.idtutorial == idTutorial &&
                c.ComentarioPadreId == null
            );

        int totalPaginas = Math.Max(
            1,
            (int)Math.Ceiling(totalElementos / (double)TamañoPagina)
        );

        pagina = Math.Clamp(pagina, 1, totalPaginas);

        var comentarios = await _context.Set<Comentario>()
            .Where(c =>
                c.idtutorial == idTutorial &&
                c.ComentarioPadreId == null
            )
            .Include(c => c.Usuario)
            .Include(c => c.Likes)
            .Include(c => c.Respuestas)
                .ThenInclude(r => r.Usuario)
            .Include(c => c.Respuestas)
                .ThenInclude(r => r.Likes)
            .OrderByDescending(c => c.FechaCreacion)
            .Skip((pagina - 1) * TamañoPagina)
            .Take(TamañoPagina)
            .ToListAsync();

        ViewBag.Tutorial = tutorial;
        ViewBag.IdTutorial = idTutorial;
        ViewBag.Pagina = pagina;
        ViewBag.TotalPaginas = totalPaginas;
        ViewBag.TotalElementos = totalElementos;

        return View(comentarios);
    }

    // Crear un comentario nuevo.
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(int idTutorial, string? contenido)
    {
        int? usuarioId = ObtenerUsuarioId();

        if (usuarioId is null)
            return Challenge();

        if (idTutorial <= 0)
            return NotFound();

        bool tutorialExiste = await _context.Set<Tutorial>()
            .AnyAsync(t => t.Idtutorial == idTutorial);

        if (!tutorialExiste)
            return NotFound();

        contenido = contenido?.Trim();

        if (string.IsNullOrWhiteSpace(contenido) || contenido.Length > 1000)
        {
            TempData["Error"] =
                "El comentario es obligatorio y no puede superar los 1000 caracteres.";

            return RedirectToAction(nameof(Index), new { idTutorial });
        }

        var comentario = new Comentario
        {
            idtutorial = idTutorial,
            Idusuario = usuarioId.Value,
            Contenido = contenido,
            FechaCreacion = DateTime.UtcNow
        };

        _context.Set<Comentario>().Add(comentario);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { idTutorial });
    }

    // Responder a un comentario del mismo tutorial.
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Responder(
        int idTutorial,
        int comentarioPadreId,
        string? contenido)
    {
        int? usuarioId = ObtenerUsuarioId();

        if (usuarioId is null)
            return Challenge();

        if (idTutorial <= 0)
            return NotFound();

        var comentarioPadre = await _context.Set<Comentario>()
            .FirstOrDefaultAsync(c => c.Idcomentario == comentarioPadreId);

        if (comentarioPadre is null)
            return NotFound();

        if (comentarioPadre.idtutorial != idTutorial)
            return BadRequest();

        contenido = contenido?.Trim();

        if (string.IsNullOrWhiteSpace(contenido) || contenido.Length > 1000)
        {
            TempData["Error"] =
                "La respuesta es obligatoria y no puede superar los 1000 caracteres.";

            return RedirectToAction(nameof(Index), new { idTutorial });
        }

        var respuesta = new Comentario
        {
            idtutorial = idTutorial,
            ComentarioPadreId = comentarioPadreId,
            Idusuario = usuarioId.Value,
            Contenido = contenido,
            FechaCreacion = DateTime.UtcNow
        };

        _context.Set<Comentario>().Add(respuesta);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { idTutorial });
    }

    // Mostrar el formulario de edición del comentario propio.
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        int? usuarioId = ObtenerUsuarioId();

        if (usuarioId is null)
            return Challenge();

        var comentario = await _context.Set<Comentario>()
            .Include(c => c.Tutorial)
            .FirstOrDefaultAsync(c => c.Idcomentario == id);

        if (comentario is null)
            return NotFound();

        if (comentario.Idusuario != usuarioId.Value)
            return Forbid();

        return View(comentario);
    }

    // Procesar la edición del comentario.
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, string? contenido)
    {
        int? usuarioId = ObtenerUsuarioId();

        if (usuarioId is null)
            return Challenge();

        var comentario = await _context.Set<Comentario>()
            .FirstOrDefaultAsync(c => c.Idcomentario == id);

        if (comentario is null)
            return NotFound();

        if (comentario.Idusuario != usuarioId.Value)
            return Forbid();

        contenido = contenido?.Trim();

        if (string.IsNullOrWhiteSpace(contenido) || contenido.Length > 1000)
        {
            ModelState.AddModelError(
                nameof(contenido),
                "El contenido es obligatorio y no puede superar los 1000 caracteres."
            );

            return View(comentario);
        }

        comentario.Contenido = contenido;

        await _context.SaveChangesAsync();

        return RedirectToAction(
            nameof(Index),
            new { idTutorial = comentario.idtutorial }
        );
    }

    // Mostrar la confirmación de eliminación.
    // El autor o un administrador puede eliminar el comentario.
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Eliminar(int id)
    {
        int? usuarioId = ObtenerUsuarioId();

        if (usuarioId is null)
            return Challenge();

        var comentario = await _context.Set<Comentario>()
            .FirstOrDefaultAsync(c => c.Idcomentario == id);

        if (comentario is null)
            return NotFound();

        if (comentario.Idusuario != usuarioId.Value &&
            !User.IsInRole(Roles.Administrador))
        {
            return Forbid();
        }

        return View(comentario);
    }

    // Confirmar y ejecutar la eliminación.
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Eliminar")]
    public async Task<IActionResult> ConfirmarEliminar(int id)
    {
        int? usuarioId = ObtenerUsuarioId();

        if (usuarioId is null)
            return Challenge();

        var comentario = await _context.Set<Comentario>()
            .FirstOrDefaultAsync(c => c.Idcomentario == id);

        if (comentario is null)
            return NotFound();

        if (comentario.Idusuario != usuarioId.Value &&
            !User.IsInRole(Roles.Administrador))
        {
            return Forbid();
        }

        int idTutorial = comentario.idtutorial;

        await EliminarComentarioAsync(comentario.Idcomentario);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { idTutorial });
    }

    // Eliminar los Me gusta y conservar las respuestas,
    // convirtiéndolas en comentarios principales.
    private async Task EliminarComentarioAsync(int comentarioId)
    {
        var comentario = await _context.Set<Comentario>()
            .FindAsync(comentarioId);

        if (comentario is null)
            return;

        var respuestas = await _context.Set<Comentario>()
            .Where(c => c.ComentarioPadreId == comentarioId)
            .ToListAsync();

        foreach (var respuesta in respuestas)
        {
            respuesta.ComentarioPadreId = null;
        }

        var likes = await _context.Set<LikeDeComentario>()
            .Where(l => l.ComentarioId == comentarioId)
            .ToListAsync();

        _context.Set<LikeDeComentario>().RemoveRange(likes);

        _context.Set<Comentario>().Remove(comentario);
    }
}
