using Microsoft.EntityFrameworkCore;
using ProyectoFinalMVC.Models;

namespace ProyectoFinalMVC.Services;

public class TutorialGuardadoServicio : ITutorialGuardadoServicio
{
    private readonly ProyectoFinalMVCContext _context;

    public TutorialGuardadoServicio(ProyectoFinalMVCContext context)
    {
        _context = context;
    }

    // 1. LISTAR LOS TUTORIALES GUARDADOS DE UNA LISTA

    public async Task<List<TutorialGuardado>> ListarPorListaAsync(int idListaPersonal, int pagina = 1, int tamañoPagina = 10)
    {
        if (pagina < 1) pagina = 1;

        return await _context.TutorialGuardado
            .AsNoTracking()
            .Where(tg => tg.IdListaPersonal == idListaPersonal)
            .Include(tg => tg.Tutorial)
                .ThenInclude(t => t.Usuario)
            .OrderByDescending(tg => tg.FechaGuardado)
            .Skip((pagina - 1) * tamañoPagina)
            .Take(tamañoPagina)
            .ToListAsync();
    }

    public async Task<int> ContarPorListaAsync(int idListaPersonal)
    {
        return await _context.TutorialGuardado
            .CountAsync(tg => tg.IdListaPersonal == idListaPersonal);
    }

    // 2. GUARDAR UN TUTORIAL EN UNA LISTA

    public async Task<(bool Ok, string? Error)> GuardarAsync(int idListaPersonal, int usuarioId, int tutorialId)
    {
        var lista = await _context.ListaPersonal
            .FirstOrDefaultAsync(l => l.IdListaPersonal == idListaPersonal);

        if (lista is null) return (false, "La lista no existe.");
        if (lista.DueñoId != usuarioId) return (false, "No tienes permiso sobre esta lista.");

        bool existeTutorial = await _context.Tutorial
            .AnyAsync(t => t.Idtutorial == tutorialId);

        if (!existeTutorial) return (false, "El tutorial no existe.");

        bool yaGuardado = await _context.TutorialGuardado
            .AnyAsync(tg =>
                tg.IdListaPersonal == idListaPersonal &&
                tg.TutorialId == tutorialId);

        if (yaGuardado) return (true, null);

        var guardado = new TutorialGuardado
        {
            IdListaPersonal = idListaPersonal,
            TutorialId = tutorialId,
            FechaGuardado = DateTime.UtcNow
        };

        _context.TutorialGuardado.Add(guardado);
        await _context.SaveChangesAsync();

        return (true, null);
    }

    // 3. QUITAR UN TUTORIAL GUARDADO

    public async Task<bool> QuitarAsync(int idTutorialGuardado, int usuarioId)
    {
        var guardado = await _context.TutorialGuardado
            .Include(tg => tg.ListaPersonal)
            .FirstOrDefaultAsync(tg => tg.IdTutorialGuardado == idTutorialGuardado);

        if (guardado is null) return false;
        if (guardado.ListaPersonal.DueñoId != usuarioId) return false;

        _context.TutorialGuardado.Remove(guardado);
        await _context.SaveChangesAsync();

        return true;
    }
}
