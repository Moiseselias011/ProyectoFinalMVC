using ProyectoFinalMVC.Models;

namespace ProyectoFinalMVC.Services;

public interface ITokensServicio
{
    (string Token, DateTime Expiracion) GenerarToken(Usuario usuario);
}
