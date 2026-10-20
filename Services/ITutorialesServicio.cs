using ProyectoFinalMVC.Models;

namespace ProyectoFinalMVC.Services;


public interface ITutorialesServicio
{
   
    Task<List<Tutorial>> ListarAsync(int pagina = 1, int tamañoPagina = 10, string? buscar = null, int? tecnologiaId = null);

   
    Task<int> ContarAsync(string? buscar = null, int? tecnologiaId = null);

   
    Task<Tutorial?> ObtenerPorIdAsync(int id);

    
    Task<(bool Ok, string? Error)> CrearAsync(Tutorial tutorial);

    
    Task<(bool Ok, string? Error)> EditarAsync(Tutorial tutorial);

    
    Task<bool> EliminarAsync(int id);
}