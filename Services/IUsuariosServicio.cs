using ProyectoFinalMVC.Models;

namespace ProyectoFinalMVC.Services;


public interface IUsuariosServicio
{
    
    Task<List<Usuario>> ListarAsync(int pagina = 1, int tamañoPagina = 10);

    
    Task<int> ContarAsync();

    
    Task<Usuario?> ObtenerPorIdAsync(int id);

    
    Task<(bool Ok, string? Error)> RegistrarAsync(string nombreUsuario, string email, string contrasena);

    
    Task<Usuario?> AutenticarAsync(string email, string contrasena);

    
    Task<(bool Ok, string? Error)> CambiarRolAsync(int id, string rol, int idAdministradorActual);

   
    Task<bool> EliminarAsync(int id, int idAdministradorActual);
}

