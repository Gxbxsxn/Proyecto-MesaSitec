using MesaSitec.Dominio.Enums;

namespace MesaSitec.Dominio.Reglas;

/// <summary>
/// RN-02 — Máquina de estados de la Solicitud.
/// Implementada a mano (no con librerías externas) para que las reglas
/// de transición queden explícitas y sean fáciles de auditar y testear.
/// </summary>
public static class MaquinaEstadosSolicitud
{
    private static readonly IReadOnlyDictionary<EstadoSolicitud, IReadOnlyDictionary<string, EstadoSolicitud>> Transiciones =
        new Dictionary<EstadoSolicitud, IReadOnlyDictionary<string, EstadoSolicitud>>
        {
            [EstadoSolicitud.Nueva] = new Dictionary<string, EstadoSolicitud>
            {
                ["asignar"] = EstadoSolicitud.Asignada,
                ["cancelar"] = EstadoSolicitud.Cancelada,
            },
            [EstadoSolicitud.Asignada] = new Dictionary<string, EstadoSolicitud>
            {
                ["iniciar"] = EstadoSolicitud.EnProceso,
                ["asignar"] = EstadoSolicitud.Asignada,
                ["cancelar"] = EstadoSolicitud.Cancelada,
            },
            [EstadoSolicitud.EnProceso] = new Dictionary<string, EstadoSolicitud>
            {
                ["resolver"] = EstadoSolicitud.Resuelta,
                ["asignar"] = EstadoSolicitud.Asignada,
                ["cancelar"] = EstadoSolicitud.Cancelada,
            },
            [EstadoSolicitud.Resuelta] = new Dictionary<string, EstadoSolicitud>
            {
                ["cerrar"] = EstadoSolicitud.Cerrada,
                ["reabrir"] = EstadoSolicitud.EnProceso,
            },
            [EstadoSolicitud.Cerrada] = new Dictionary<string, EstadoSolicitud>(),
            [EstadoSolicitud.Cancelada] = new Dictionary<string, EstadoSolicitud>(),
        };

    public static readonly IReadOnlyCollection<string> AccionesValidas =
        new[] { "asignar", "iniciar", "resolver", "cerrar", "reabrir", "cancelar" };

    /// <summary>
    /// Intenta aplicar una acción sobre un estado actual.
    /// Devuelve true y el estado resultante si la transición es válida.
    /// </summary>
    public static bool TryTransicionar(EstadoSolicitud estadoActual, string accion, out EstadoSolicitud estadoNuevo)
    {
        estadoNuevo = estadoActual;

        if (!Transiciones.TryGetValue(estadoActual, out var accionesPermitidas))
        {
            return false;
        }

        if (!accionesPermitidas.TryGetValue(accion, out var siguiente))
        {
            return false;
        }

        estadoNuevo = siguiente;
        return true;
    }

    public static bool EsAccionConocida(string accion) => AccionesValidas.Contains(accion);
}
