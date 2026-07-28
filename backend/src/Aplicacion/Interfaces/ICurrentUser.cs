using MesaSitec.Dominio.Enums;

namespace MesaSitec.Aplicacion.Interfaces;

/// <summary>
/// Abstrae el acceso a los claims del JWT (sub, tenantId, rol, email) para que
/// la capa de Aplicación no dependa directamente de HttpContext.
/// </summary>
public interface ICurrentUser
{
    Guid UsuarioId { get; }
    Guid TenantId { get; }
    RolUsuario Rol { get; }
    string Email { get; }
}
