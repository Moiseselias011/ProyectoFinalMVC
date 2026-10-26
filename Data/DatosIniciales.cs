using Microsoft.EntityFrameworkCore;
using ProyectoFinalMVC.Models;

namespace ProyectoFinalMVC;

// Carga inicial de la base de datos.
public static class DatosIniciales
{
    // Lista fija de tecnologías: la aplicación solo permite elegir entre estas,
    // no se pueden crear, editar ni eliminar desde la interfaz.
    public static readonly (string Nombre, string Slug)[] Tecnologias = new[]
    {
        ("C#", "csharp"),
        (".NET", "dotnet"),
        ("JavaScript", "javascript"),
        ("TypeScript", "typescript"),
        ("React", "react"),
        ("Angular", "angular"),
        ("Node.js", "nodejs"),
        ("Python", "python"),
        ("Java", "java"),
        ("PHP", "php"),
        ("SQL", "sql"),
        ("HTML y CSS", "html-css")
    };

    
    public static async Task SembrarTecnologiasAsync(ProyectoFinalMVCContext context)
    {
        var slugsExistentes = await context.Tecnologia
            .Select(t => t.Slug)
            .ToListAsync();

        var faltantes = Tecnologias
            .Where(t => !slugsExistentes.Contains(t.Slug))
            .Select(t => new Tecnologia
            {
                Nombre = t.Nombre,
                Slug = t.Slug,
                Dominio = Domains.Programacion
            })
            .ToList();

        if (faltantes.Count == 0) return;

        context.Tecnologia.AddRange(faltantes);
        await context.SaveChangesAsync();
    }
}
