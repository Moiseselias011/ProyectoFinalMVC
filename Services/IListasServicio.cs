using ProyectoFinalMVC.Models;

namespace ProyectoFinalMVC.Services;


public interface IListasServicio
{
    
    Task<List<ListaPersonal>> ListarPorUsuarioAsync(int usuarioId);

    
    Task<ListaPersonal?> ObtenerPorIdAsync(int id);

    
    Task<(bool Ok, string? Error, ListaPersonal? Lista)> CrearAsync(int usuarioId, string? nombre);

    
    Task<(bool Ok, string? Error)> EditarAsync(int id, int usuarioId, string? nombre);

    
    Task<bool> EliminarAsync(int id, int usuarioId);
}