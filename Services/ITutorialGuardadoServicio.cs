using ProyectoFinalMVC.Models;

namespace ProyectoFinalMVC.Services;

public interface ITutorialGuardadoServicio
{
   
    Task<List<TutorialGuardado>> ListarPorListaAsync(int idListaPersonal, int pagina = 1, int tamañoPagina = 10);

    
    Task<int> ContarPorListaAsync(int idListaPersonal);

   
    Task<(bool Ok, string? Error)> GuardarAsync(int idListaPersonal, int usuarioId, int tutorialId);

    
    Task<bool> QuitarAsync(int idTutorialGuardado, int usuarioId);
}