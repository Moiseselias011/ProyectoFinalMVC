
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
        return int.TryParse(valorId, out int usuarioId) ? usuarioId : null;
    }

    // Listar los comentarios principales de un tutorial.
    // Las respuestas se incluyen dentro de cada comentario.
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Index(int idtutorial, int pagina = 1)
    {
        if (idtutorial <= 0) return NotFound();

        var tutorial = await _context.Set<Tutorial>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.idtutorial == idtutorial);

        if (tutorial is null) return NotFound();

        int totalElementos = await _context.Set<Comentario>()
            .CountAsync(c =>
                c.idtutorial == idtutorial &&
                c.ComentarioPadreId == null
            );

        int totalPaginas = Math.Max(
            1,
            (int)Math.Ceiling(totalElementos / (double)TamañoPagina)
        );

        pagina = Math.Clamp(pagina, 1, totalPaginas);

        var comentarios = await _context.Set<Comentario>()
            .Where(c =>
                c.idtutorial == idtutorial &&
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
        ViewBag.IdTutorial = idtutorial;
        ViewBag.Pagina = pagina;
        ViewBag.TotalPaginas = totalPaginas;
        ViewBag.TotalElementos = totalElementos;

        return View(comentarios);
    }

    // Crear un comentario nuevo.
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(int idtutorial, string? contenido)
    {
        int? usuarioId = ObtenerUsuarioId();

        if (usuarioId is null) return Challenge();

        var tutorialExiste = await _context.Set<Tutorial>()
            .AnyAsync(t => t.idtutorial == idtutorial);

        if (!tutorialExiste) return NotFound();

        contenido = contenido?.Trim();

        if (string.IsNullOrWhiteSpace(contenido) || contenido.Length > 1000)
        {
            TempData["Error"] =
                "El comentario es obligatorio y no puede superar los 1000 caracteres.";

            return RedirectToAction(nameof(Index), new { idtutorial });
        }

        var comentario = new Comentario
        {
            idtutorial = idtutorial,
            Idusuario = usuarioId.Value,
            Contenido = contenido,
            FechaCreacion = DateTime.UtcNow
        };

        _context.Set<Comentario>().Add(comentario);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { idtutorial });
    }

    // Responder a un comentario del mismo tutorial.
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Responder(
        int idtutorial,
        int comentarioPadreId,
        string? contenido)
    {
        int? usuarioId = ObtenerUsuarioId();

        if (usuarioId is null) return Challenge();

        var comentarioPadre = await _context.Set<Comentario>()
            .FirstOrDefaultAsync(c => c.Idcomentario == comentarioPadreId);

        if (comentarioPadre is null) return NotFound();

        if (comentarioPadre.idtutorial != idtutorial)
            return BadRequest();

        contenido = contenido?.Trim();

        if (string.IsNullOrWhiteSpace(contenido) || contenido.Length > 1000)
        {
            TempData["Error"] =
                "La respuesta es obligatoria y no puede superar los 1000 caracteres.";

            return RedirectToAction(nameof(Index), new { idtutorial });
        }

        var respuesta = new Comentario
        {
            idtutorial = idtutorial,
            ComentarioPadreId = comentarioPadreId,
            Idusuario = usuarioId.Value,
            Contenido = contenido,
            FechaCreacion = DateTime.UtcNow
        };

        _context.Set<Comentario>().Add(respuesta);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { idtutorial });
    }

    // Mostrar el formulario de edición del comentario propio.
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        int? usuarioId = ObtenerUsuarioId();

        if (usuarioId is null) return Challenge();

        var comentario = await _context.Set<Comentario>()
            .Include(c => c.Tutorial)
            .FirstOrDefaultAsync(c => c.Idcomentario == id);

        if (comentario is null) return NotFound();

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

        if (usuarioId is null) return Challenge();

        var comentario = await _context.Set<Comentario>()
            .FirstOrDefaultAsync(c => c.Idcomentario == id);

        if (comentario is null) return NotFound();

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
            new { idtutorial = comentario.idtutorial }
        );
    }

    // Mostrar la confirmación de eliminación.
    // El autor o un administrador puede eliminar el comentario.
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Eliminar(int id)
    {
        int? usuarioId = ObtenerUsuarioId();

        if (usuarioId is null) return Challenge();

        var comentario = await _context.Set<Comentario>()
            .FirstOrDefaultAsync(c => c.Idcomentario == id);

        if (comentario is null) return NotFound();

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

        if (usuarioId is null) return Challenge();

        var comentario = await _context.Set<Comentario>()
            .FirstOrDefaultAsync(c => c.Idcomentario == id);

        if (comentario is null) return NotFound();

        if (comentario.Idusuario != usuarioId.Value &&
            !User.IsInRole(Roles.Administrador))
        {
            return Forbid();
        }

        int idtutorial = comentario.idtutorial;

        await EliminarComentarioYRespuestasAsync(comentario.Idcomentario);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), new { idtutorial });
    }

    // Eliminar recursivamente las respuestas y sus Me gusta
    // antes de eliminar el comentario principal.
    private async Task EliminarComentarioYRespuestasAsync(int comentarioId)
    {
        var idsHijos = await _context.Set<Comentario>()
            .Where(c => c.ComentarioPadreId == comentarioId)
            .Select(c => c.Idcomentario)
            .ToListAsync();

        foreach (int idHijo in idsHijos)
        {
            await EliminarComentarioYRespuestasAsync(idHijo);
        }

        var likes = await _context.Set<LikeDeComentario>()
            .Where(l => l.ComentarioId == comentarioId)
            .ToListAsync();

        _context.Set<LikeDeComentario>().RemoveRange(likes);

        var comentario = await _context.Set<Comentario>()
            .FindAsync(comentarioId);

        if (comentario is not null)
        {
            _context.Set<Comentario>().Remove(comentario);
        }
    }
}
