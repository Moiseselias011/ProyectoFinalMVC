using ProyectoFinalMVC.Models;

namespace ProyectoFinalMVC.Services;


public interface ITecnologiasServicio
{
    
    Task<List<Tecnologia>> ListarAsync();

   
    Task<Tecnologia?> ObtenerPorIdOSlugAsync(string idOrSlug);
}