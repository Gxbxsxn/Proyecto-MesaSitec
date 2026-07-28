using MesaSitec.Aplicacion.DTOs;
using MesaSitec.Aplicacion.Errores;
using MesaSitec.Aplicacion.Interfaces;
using MesaSitec.Infraestructura.Persistencia;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.Aplicacion.Servicios;

public class AuthService
{
    private readonly MesaSitecDbContext _db;
    private readonly IJwtEmisor _jwt;
    private readonly PasswordHasher<Dominio.Entidades.Usuario> _hasher = new();

    public AuthService(MesaSitecDbContext db, IJwtEmisor jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var usuario = await _db.Usuarios
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (usuario is null || !usuario.Activo || usuario.Tenant is null || !usuario.Tenant.Activo)
        {
            throw ApiException.NoAutenticado("Email o contraseña incorrectos.");
        }

        var resultado = _hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, request.Password);
        if (resultado == PasswordVerificationResult.Failed)
        {
            throw ApiException.NoAutenticado("Email o contraseña incorrectos.");
        }

        var (token, expiraEn) = _jwt.EmitirToken(usuario);
        var dto = MapearUsuario(usuario);

        return new LoginResponse(token, expiraEn, dto);
    }

    public static UsuarioDto MapearUsuario(Dominio.Entidades.Usuario usuario) => new(
        usuario.Id,
        usuario.Nombre,
        usuario.Email,
        usuario.Rol.ToString(),
        usuario.TenantId,
        usuario.Tenant?.Nombre ?? string.Empty
    );
}
