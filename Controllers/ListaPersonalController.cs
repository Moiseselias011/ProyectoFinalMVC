using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinalMVC.Models;

namespace ProyectoFinalMVC.Controllers;

[Authorize]
public class ListaPersonalController : Controller
{
    private readonly ProyectoFinalMVCContext _context;
    private const int TamañoPagina = 10;

    public ListaPersonalController(ProyectoFinalMVCContext context)
    {
        _context = context;
    }

    private int? ObtenerUsuarioId()
    {
        string? valorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(valorId, out int usuarioId) ? usuarioId : null;
    }

    // 1. Listar solamente las listas del usuario autenticado.
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        int? usuarioId = ObtenerUsuarioId();
        if (usuarioId is null) return Challenge();

        var listas = await _context.Set<ListaPersonal>()
            .Where(l => l.DueñoId == usuarioId.Value)
            .Include(l => l.TutorialesGuardados)
            .OrderByDescending(l => l.FechaCreacion)
            .ToListAsync();

        // Cada lista incluye TutorialesGuardados para calcular la cantidad.
        return View(listas);
    }

    // 2. Consultar una lista y paginar sus tutoriales guardados.
    [HttpGet]
    public async Task<IActionResult> Detalle(int id, int pagina = 1)
    {
        int? usuarioId = ObtenerUsuarioId();
        if (usuarioId is null) return Challenge();
        if (id <= 0) return NotFound();

        var lista = await _context.Set<ListaPersonal>()
            .FirstOrDefaultAsync(l => l.IdListaPersonal == id);

        if (lista is null) return NotFound();
        if (lista.DueñoId != usuarioId.Value) return Forbid();

        int totalElementos = await _context.Set<TutorialGuardado>()
            .CountAsync(tg => tg.IdListaPersonal == id);

        int totalPaginas = Math.Max(
            1,
            (int)Math.Ceiling(totalElementos / (double)TamañoPagina)
        );

        pagina = Math.Clamp(pagina, 1, totalPaginas);

        var tutorialesGuardados = await _context.Set<TutorialGuardado>()
            .Where(tg => tg.IdListaPersonal == id)
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

        return View(tutorialesGuardados);
    }

    // 3. Mostrar el formulario para crear una lista.
    [HttpGet]
    public IActionResult Crear()
    {
        return View();
    }

    // Procesar la creación de una lista.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(string? nombre)
    {
        int? usuarioId = ObtenerUsuarioId();
        if (usuarioId is null) return Challenge();

        nombre = nombre?.Trim();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            ModelState.AddModelError(
                nameof(nombre),
                "El nombre de la lista es obligatorio."
            );

            return View();
        }

        if (nombre.Length > 100)
        {
            ModelState.AddModelError(
                nameof(nombre),
                "El nombre no puede superar los 100 caracteres."
            );

            return View();
        }

        var nuevaLista = new ListaPersonal
        {
            Nombre = nombre,
            DueñoId = usuarioId.Value,
            FechaCreacion = DateTime.UtcNow
        };

        _context.Set<ListaPersonal>().Add(nuevaLista);
        await _context.SaveChangesAsync();

        TempData["Mensaje"] = "La lista se creó correctamente.";

        return RedirectToAction(nameof(Index));
    }

    // 4. Mostrar el formulario para editar una lista propia.
    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        int? usuarioId = ObtenerUsuarioId();
        if (usuarioId is null) return Challenge();

        var lista = await _context.Set<ListaPersonal>()
            .FirstOrDefaultAsync(l => l.IdListaPersonal == id);

        if (lista is null) return NotFound();
        if (lista.DueñoId != usuarioId.Value) return Forbid();

        return View(lista);
    }

    // Procesar la modificación del nombre.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, string? nombre)
    {
        int? usuarioId = ObtenerUsuarioId();
        if (usuarioId is null) return Challenge();

        var lista = await _context.Set<ListaPersonal>()
            .FirstOrDefaultAsync(l => l.IdListaPersonal == id);

        if (lista is null) return NotFound();
        if (lista.DueñoId != usuarioId.Value) return Forbid();

        nombre = nombre?.Trim();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            ModelState.AddModelError(
                nameof(nombre),
                "El nombre de la lista es obligatorio."
            );

            return View(lista);
        }

        if (nombre.Length > 100)
        {
            ModelState.AddModelError(
                nameof(nombre),
                "El nombre no puede superar los 100 caracteres."
            );

            return View(lista);
        }

        // No se modifica DueñoId.
        lista.Nombre = nombre;

        await _context.SaveChangesAsync();

        TempData["Mensaje"] = "La lista se actualizó correctamente.";

        return RedirectToAction(nameof(Index));
    }

    // Mostrar la confirmación para eliminar una lista propia.
    [HttpGet]
    public async Task<IActionResult> Eliminar(int id)
    {
        int? usuarioId = ObtenerUsuarioId();
        if (usuarioId is null) return Challenge();

        var lista = await _context.Set<ListaPersonal>()
            .FirstOrDefaultAsync(l => l.IdListaPersonal == id);

        if (lista is null) return NotFound();
        if (lista.DueñoId != usuarioId.Value) return Forbid();

        return View(lista);
    }

    // Eliminar las relaciones de guardado y luego la lista.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Eliminar")]
    public async Task<IActionResult> ConfirmarEliminar(int id)
    {
        int? usuarioId = ObtenerUsuarioId();
        if (usuarioId is null) return Challenge();

        var lista = await _context.Set<ListaPersonal>()
            .FirstOrDefaultAsync(l => l.IdListaPersonal == id);

        if (lista is null) return NotFound();
        if (lista.DueñoId != usuarioId.Value) return Forbid();

        var relaciones = await _context.Set<TutorialGuardado>()
            .Where(tg => tg.IdListaPersonal == id)
            .ToListAsync();

        _context.Set<TutorialGuardado>().RemoveRange(relaciones);
        _context.Set<ListaPersonal>().Remove(lista);

        await _context.SaveChangesAsync();

        TempData["Mensaje"] = "La lista se eliminó correctamente.";

        return RedirectToAction(nameof(Index));
    }
}
