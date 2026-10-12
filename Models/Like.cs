
namespace ProyectoFinalMVC.Models;

public class Like
{
    public int IdLike { get; set; }

    public int UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public int TutorialId { get; set; }

    public Tutorial Tutorial { get; set; } = null!;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
