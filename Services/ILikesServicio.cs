using ProyectoFinalMVC.Models;

namespace ProyectoFinalMVC.Services;


public interface ILikesServicio
{
    Task<bool> AgregarLikeTutorialAsync(int usuarioId, int tutorialId);

   
    Task<bool> QuitarLikeTutorialAsync(int usuarioId, int tutorialId);

   
    Task<int> ContarLikesTutorialAsync(int tutorialId);

    Task<bool> LeGustaTutorialAsync(int usuarioId, int tutorialId);

   
    Task<bool> AgregarLikeComentarioAsync(int usuarioId, int comentarioId);

   
    Task<bool> QuitarLikeComentarioAsync(int usuarioId, int comentarioId);

   
    Task<int> ContarLikesComentarioAsync(int comentarioId);
}