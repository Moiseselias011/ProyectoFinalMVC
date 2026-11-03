using Microsoft.EntityFrameworkCore;
using ProyectoFinalMVC.Models;

namespace ProyectoFinalMVC.Services;

public class TecnologiasServicio : ITecnologiasServicio
{
    private readonly ProyectoFinalMVCContext _context;

    public TecnologiasServicio(ProyectoFinalMVCContext context)
    {
        _context = context;
    }

    // 1. LISTAR TECNOLOGÍAS

    public async Task<List<Tecnologia>> ListarAsync()
    {
        return await _context.Tecnologia
            .AsNoTracking()
            .OrderBy(t => t.Nombre)
            .ToListAsync();
    }

    // 2. OBTENER POR ID O SLUG

    public async Task<Tecnologia?> ObtenerPorIdOSlugAsync(string idOrSlug)
    {
        bool esId = int.TryParse(idOrSlug, out int idTecnologia);

        return await _context.Tecnologia
            .AsNoTracking()
            .FirstOrDefaultAsync(t =>
                (esId && t.IdTecnologia == idTecnologia) ||
                (!esId && t.Slug == idOrSlug));
    }

    public async Task<List<Tecnologia>> BuscarPorNombreAsync(string termino)
    {
        if (string.IsNullOrWhiteSpace(termino))
        {
            return new List<Tecnologia>();
        }

        return await _context.Tecnologia
            .AsNoTracking()
            .Where(t => EF.Functions.Like(t.Nombre, $"%{termino}%") || EF.Functions.Like(t.Slug, $"%{termino}%"))
            .OrderBy(t => t.Nombre)
            .ToListAsync();
    }
}
