namespace MesaSitec.Aplicacion.DTOs;

public record AgenteDisponibleDto(Guid Id, string Nombre, string Email);

public record EmpleadoListadoItemDto(
    Guid Id,
    string Nombre,
    string Email,
    string Rol,
    bool Activo
);

public record CrearEmpleadoRequest(
    string Nombre,
    string Email,
    string Rol,
    string PasswordTemporal
);

public record EditarEmpleadoRequest(
    string Nombre,
    string Email,
    string? Rol
);
