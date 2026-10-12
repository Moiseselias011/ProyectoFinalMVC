
using System.ComponentModel.DataAnnotations;
namespace ProyectoFinalMVC.Models;

public class Tecnologia
{
    [Key]
    public int IdTecnologia { get; set; }
    
    [Required,MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required,MaxLength(100)] 
    public string Slug { get; set; } = string.Empty;
     

    public string Dominio { get; set; } = Domains.PorDefecto;

    public ICollection<Tutorial> Tutoriales { get; set; }= new List<Tutorial>();
                                                          
}

public static class Domains
{
    public const string Programacion = "programacion";

    public const string PorDefecto = Programacion;

    public static bool EsValido(string dominio) => dominio == Programacion;
       
}
