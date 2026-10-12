using System.ComponentModel.DataAnnotations;

namespace ProyectoFinalMVC.Models;

public class TutorialGuardado
{   
    [Key]
    public int IdTutorialGuardado { get; set; }
    
    public int IdListaPersonal { get; set; }

    public ListaPersonal ListaPersonal { get; set; } = null!;

    public int TutorialId { get; set; }

    public Tutorial Tutorial { get; set; } = null!;

    public DateTime FechaGuardado { get; set; } = DateTime.UtcNow;
}
