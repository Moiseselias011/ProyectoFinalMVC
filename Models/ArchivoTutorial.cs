using System.ComponentModel.DataAnnotations;

namespace ProyectoFinalMVC.Models;

public class ArchivoTutorial
{
    [Key]
    public int IdArchivoTutorial { get; set; }

    [Required, MaxLength(200)]
    public string NombreArchivo { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string RutaAlmacenamiento { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string TipoMime { get; set; } = string.Empty;

    public long TamañoBytes { get; set; }

    public DateTime FechaCarga { get; set; } = DateTime.UtcNow;

    public int IdTutorial { get; set; }

    public Tutorial Tutorial { get; set; } = null!;
}
