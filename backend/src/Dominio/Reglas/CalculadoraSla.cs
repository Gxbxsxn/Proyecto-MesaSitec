using MesaSitec.Dominio.Enums;

namespace MesaSitec.Dominio.Reglas;

/// <summary>
/// RN-04 — Cálculo del SLA. Siempre se ejecuta en el servidor.
/// </summary>
public static class CalculadoraSla
{
    private static readonly IReadOnlyDictionary<PrioridadSolicitud, double> FactorPorPrioridad =
        new Dictionary<PrioridadSolicitud, double>
        {
            [PrioridadSolicitud.Critica] = 0.5,
            [PrioridadSolicitud.Alta] = 0.75,
            [PrioridadSolicitud.Media] = 1.0,
            [PrioridadSolicitud.Baja] = 2.0,
        };

    public static DateTime CalcularFechaLimite(DateTime fechaCreacionUtc, int slaHorasCategoria, PrioridadSolicitud prioridad)
    {
        var horasAjustadas = slaHorasCategoria * FactorPorPrioridad[prioridad];
        return fechaCreacionUtc.AddHours(horasAjustadas);
    }

    /// <summary>
    /// Una solicitud está vencida si su fecha límite ya pasó y su estado
    /// no es uno de los estados finales/terminales de atención.
    /// </summary>
    public static bool EstaVencida(DateTime fechaLimiteUtc, EstadoSolicitud estado, DateTime ahoraUtc)
    {
        if (estado is EstadoSolicitud.Resuelta or EstadoSolicitud.Cerrada or EstadoSolicitud.Cancelada)
        {
            return false;
        }

        return fechaLimiteUtc < ahoraUtc;
    }
}
