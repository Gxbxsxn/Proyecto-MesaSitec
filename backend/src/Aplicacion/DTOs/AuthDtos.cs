namespace MesaSitec.Aplicacion.DTOs;

public record LoginRequest(string Email, string Password);

public record UsuarioDto(
    Guid Id,
    string Nombre,
    string Email,
    string Rol,
    Guid TenantId,
    string TenantNombre
);

public record LoginResponse(string AccessToken, int ExpiraEn, UsuarioDto Usuario);
