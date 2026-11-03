using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ProyectoFinalMVC.Models;

namespace ProyectoFinalMVC.Services;

public class TokensServicio : ITokensServicio
{
    private readonly IConfiguration _configuracion;

    public TokensServicio(IConfiguration configuracion)
    {
        _configuracion = configuracion;
    }

    public (string Token, DateTime Expiracion) GenerarToken(Usuario usuario)
    {
        string? clave = _configuracion["Jwt:Clave"];

        if (string.IsNullOrEmpty(clave) || Encoding.UTF8.GetByteCount(clave) < 32)
        {
            throw new InvalidOperationException(
                "La clave de firma JWT no está configurada (user-secrets Jwt:Clave) o tiene menos de 256 bits.");
        }

        int minutos = 60;
        int.TryParse(_configuracion["Jwt:MinutosExpiracion"], out minutos);
        if (minutos <= 0) minutos = 60;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id_usuario.ToString()),
            new Claim(ClaimTypes.Name, usuario.NombreUsuario),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Rol)
        };

        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clave)),
            SecurityAlgorithms.HmacSha256);

        var expiracion = DateTime.UtcNow.AddMinutes(minutos);

        var token = new JwtSecurityToken(
            issuer: _configuracion["Jwt:Emisor"],
            audience: _configuracion["Jwt:Audiencia"],
            claims: claims,
            expires: expiracion,
            signingCredentials: credenciales);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiracion);
    }
}
