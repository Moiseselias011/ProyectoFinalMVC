
using System.ComponentModel.DataAnnotations;
namespace ProyectoFinalMVC.Models;

public class LikeDeComentario
{
    [Key]
    public int IdLikeDeComentario { get; set; }

    public int UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public int ComentarioId { get; set; }

    public Comentario Comentario { get; set; } = null!;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
