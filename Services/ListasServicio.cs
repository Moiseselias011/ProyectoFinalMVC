using Microsoft.EntityFrameworkCore;
using ProyectoFinalMVC.Models;

namespace ProyectoFinalMVC.Services;

public class ListasServicio : IListasServicio
{
    private readonly ProyectoFinalMVCContext _context;

    public ListasServicio(ProyectoFinalMVCContext context)
    {
        _context = context;
    }

    // 1. LISTAR LAS LISTAS DEL USUARIO

    public async Task<List<ListaPersonal>> ListarPorUsuarioAsync(int usuarioId)
    {
        return await _context.ListaPersonal
            .AsNoTracking()
            .Where(l => l.DueñoId == usuarioId)
            .Include(l => l.TutorialesGuardados)
            .OrderByDescending(l => l.FechaCreacion)
            .ToListAsync();
    }

    public async Task<ListaPersonal?> ObtenerPorIdAsync(int id)
    {
        return await _context.ListaPersonal
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.IdListaPersonal == id);
    }

    // 2. CREAR LISTA

    public async Task<(bool Ok, string? Error, ListaPersonal? Lista)> CrearAsync(int usuarioId, string? nombre)
    {
        nombre = nombre?.Trim();

        if (string.IsNullOrWhiteSpace(nombre))
            return (false, "El nombre de la lista es obligatorio.", null);

        if (nombre.Length > 100)
            return (false, "El nombre no puede superar los 100 caracteres.", null);

        bool nombreEnUso = await _context.ListaPersonal
            .AnyAsync(l => l.DueñoId == usuarioId && l.Nombre == nombre);

        if (nombreEnUso)
            return (false, "Ya tienes una lista con ese nombre.", null);

        var nuevaLista = new ListaPersonal
        {
            Nombre = nombre,
            DueñoId = usuarioId,
            FechaCreacion = DateTime.UtcNow
        };

        _context.ListaPersonal.Add(nuevaLista);
        await _context.SaveChangesAsync();

        return (true, null, nuevaLista);
    }

    // 3. EDITAR LISTA

    public async Task<(bool Ok, string? Error)> EditarAsync(int id, int usuarioId, string? nombre)
    {
        var lista = await _context.ListaPersonal
            .FirstOrDefaultAsync(l => l.IdListaPersonal == id);

        if (lista is null) return (false, "La lista no existe.");
        if (lista.DueñoId != usuarioId) return (false, "No tienes permiso sobre esta lista.");

        nombre = nombre?.Trim();

        if (string.IsNullOrWhiteSpace(nombre))
            return (false, "El nombre de la lista es obligatorio.");

        if (nombre.Length > 100)
            return (false, "El nombre no puede superar los 100 caracteres.");

        bool nombreEnUso = await _context.ListaPersonal
            .AnyAsync(l =>
                l.DueñoId == usuarioId &&
                l.Nombre == nombre &&
                l.IdListaPersonal != id);

        if (nombreEnUso)
            return (false, "Ya tienes una lista con ese nombre.");

        // DueñoId no se modifica.
        lista.Nombre = nombre;

        await _context.SaveChangesAsync();

        return (true, null);
    }

    // 4. ELIMINAR LISTA

    public async Task<bool> EliminarAsync(int id, int usuarioId)
    {
        var lista = await _context.ListaPersonal
            .FirstOrDefaultAsync(l => l.IdListaPersonal == id);

        if (lista is null) return false;
        if (lista.DueñoId != usuarioId) return false;

       
        var relaciones = await _context.TutorialGuardado
            .Where(tg => tg.IdListaPersonal == id)
            .ToListAsync();

        _context.TutorialGuardado.RemoveRange(relaciones);
        _context.ListaPersonal.Remove(lista);

        await _context.SaveChangesAsync();

        return true;
    }
}
