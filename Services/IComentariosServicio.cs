using ProyectoFinalMVC.Models;

namespace ProyectoFinalMVC.Services;

public interface IComentariosServicio
{
    
    Task<List<Comentario>> ListarPorTutorialAsync(int idTutorial, int pagina = 1, int tamañoPagina = 10);

   
    Task<int> ContarPorTutorialAsync(int idTutorial);
    
    Task<(bool Ok, string? Error, int? IdCreado)> CrearAsync(int idTutorial, int idUsuario, string? contenido);

    
    Task<(bool Ok, string? Error, int? IdCreado)> ResponderAsync(int idTutorial, int idUsuario, int comentarioPadreId, string? contenido);

   
    Task<(bool Ok, string? Error)> EditarAsync(int id, int idUsuario, string? contenido, bool esAdministrador);

    
    Task<bool> EliminarAsync(int id, int idUsuario, bool esAdministrador);
}