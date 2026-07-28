using System.Security.Claims;
using MesaSitec.Aplicacion.Interfaces;
using MesaSitec.Dominio.Enums;

namespace MesaSitec.Api.Configuracion;

public class CurrentUser : ICurrentUser
{
    public Guid UsuarioId { get; }
    public Guid TenantId { get; }
    public RolUsuario Rol { get; }
    public string Email { get; }

    public CurrentUser(IHttpContextAccessor accessor)
    {
        var principal = accessor.HttpContext?.User;
        var sub = principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal?.FindFirstValue("sub");
        var tenantId = principal?.FindFirstValue("tenantId");
        var rol = principal?.FindFirstValue(ClaimTypes.Role) ?? principal?.FindFirstValue("rol");
        var email = principal?.FindFirstValue(ClaimTypes.Email) ?? principal?.FindFirstValue("email");

        UsuarioId = Guid.TryParse(sub, out var uid) ? uid : Guid.Empty;
        TenantId = Guid.TryParse(tenantId, out var tid) ? tid : Guid.Empty;
        Rol = Enum.TryParse<RolUsuario>(rol, true, out var r) ? r : RolUsuario.Solicitante;
        Email = email ?? string.Empty;
    }
}
