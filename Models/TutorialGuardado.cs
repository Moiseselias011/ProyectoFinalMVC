
namespace ProyectoFinalMVC.Models;

public class TutorialGuardado
{
    public int IdTutorialGuardado { get; set; }

    public int PersonalListId { get; set; }

    public PersonalList PersonalList { get; set; } = null!;

    public int TutorialId { get; set; }

    public Tutorial Tutorial { get; set; } = null!;

    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
}
