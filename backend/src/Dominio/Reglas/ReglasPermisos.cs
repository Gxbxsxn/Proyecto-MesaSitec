using MesaSitec.Dominio.Entidades;
using MesaSitec.Dominio.Enums;

namespace MesaSitec.Dominio.Reglas;

/// <summary>
/// RN-03 — Permisos por rol. Centraliza la tabla de la sección 4 del enunciado
/// para que no queden reglas de autorización esparcidas en los controllers.
/// </summary>
public static class ReglasPermisos
{
    /// <summary>¿Puede este rol, en abstracto, ejecutar esta acción de transición?
    /// (La pertenencia — "solo las propias" — se valida aparte con PuedeCerrarPropia/PuedeEditar).</summary>
    public static bool PuedeEjecutarAccion(RolUsuario rol, string accion)
    {
        return accion switch
        {
            "asignar" or "iniciar" or "resolver" or "reabrir" => rol is RolUsuario.Admin or RolUsuario.Agente,
            "cerrar" => true, // Admin y Agente siempre; Solicitante solo si es la propia (ver PuedeCerrar)
            "cancelar" => rol == RolUsuario.Admin,
            _ => false
        };
    }

    /// <summary>Chequeo completo de "cerrar", que sí depende de si la solicitud es propia.</summary>
    public static bool PuedeCerrar(RolUsuario rol, Guid usuarioId, Solicitud solicitud)
    {
        if (rol is RolUsuario.Admin or RolUsuario.Agente) return true;
        return rol == RolUsuario.Solicitante && solicitud.SolicitanteId == usuarioId;
    }

    public static bool PuedeListarTodas(RolUsuario rol) => rol is RolUsuario.Admin or RolUsuario.Agente;

    public static bool PuedeVerDetalle(RolUsuario rol, Guid usuarioId, Solicitud solicitud)
    {
        if (rol is RolUsuario.Admin or RolUsuario.Agente) return true;
        return solicitud.SolicitanteId == usuarioId;
    }

    public static bool PuedeEditar(RolUsuario rol, Guid usuarioId, Solicitud solicitud)
    {
        if (rol is RolUsuario.Admin or RolUsuario.Agente) return true;
        return rol == RolUsuario.Solicitante
            && solicitud.SolicitanteId == usuarioId
            && solicitud.Estado == EstadoSolicitud.Nueva;
    }
}
