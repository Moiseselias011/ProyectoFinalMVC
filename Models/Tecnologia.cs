
namespace ProyectoFinalMVC.Models;

public class Tecnologia
{
    public int IdTecnologia { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Dominio { get; set; } = Domains.PorDefecto;

    public ICollection<Tutorial> Tutorials { get; set; }
        = new List<Tutorial>();
}

public static class Domains
{
    public const string Programacion = "programacion";

    public const string PorDefecto = Programacion;

    public static bool EsValido(string dominio) =>
        dominio == Programacion;
}
