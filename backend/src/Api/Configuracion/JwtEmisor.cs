using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MesaSitec.Aplicacion.Interfaces;
using MesaSitec.Dominio.Entidades;
using Microsoft.IdentityModel.Tokens;

namespace MesaSitec.Api.Configuracion;

public class JwtEmisor : IJwtEmisor
{
    private readonly IConfiguration _config;
    private const int ExpiracionHoras = 8;

    public JwtEmisor(IConfiguration config)
    {
        _config = config;
    }

    public (string token, int expiraEnSegundos) EmitirToken(Usuario usuario)
    {
        var secreto = _config["JWT_SECRET"] ?? Environment.GetEnvironmentVariable("JWT_SECRET")
            ?? throw new InvalidOperationException("Falta configurar la variable de entorno JWT_SECRET.");

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim("tenantId", usuario.TenantId.ToString()),
            new Claim("rol", usuario.Rol.ToString()),
            new Claim(ClaimTypes.Role, usuario.Rol.ToString()),
            new Claim("email", usuario.Email),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secreto));
        var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expira = DateTime.UtcNow.AddHours(ExpiracionHoras);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: expira,
            signingCredentials: credenciales
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return (jwt, ExpiracionHoras * 3600);
    }
}
