using MesaSitec.Dominio.Enums;
using MesaSitec.Dominio.Reglas;
using Xunit;

namespace MesaSitec.Tests;

public class CalculadoraSlaTests
{
    [Fact]
    public void Categoria_incidente_con_prioridad_critica_vence_a_las_4_horas()
    {
        var creacion = new DateTime(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc);

        var limite = CalculadoraSla.CalcularFechaLimite(creacion, slaHorasCategoria: 8, PrioridadSolicitud.Critica);

        Assert.Equal(creacion.AddHours(4), limite);
    }

    [Fact]
    public void Categoria_consulta_con_prioridad_baja_vence_a_las_48_horas()
    {
        var creacion = new DateTime(2026, 1, 15, 8, 0, 0, DateTimeKind.Utc);

        var limite = CalculadoraSla.CalcularFechaLimite(creacion, slaHorasCategoria: 24, PrioridadSolicitud.Baja);

        Assert.Equal(creacion.AddHours(48), limite);
    }

    [Theory]
    [InlineData(EstadoSolicitud.Nueva, true)]
    [InlineData(EstadoSolicitud.Asignada, true)]
    [InlineData(EstadoSolicitud.EnProceso, true)]
    [InlineData(EstadoSolicitud.Resuelta, false)]
    [InlineData(EstadoSolicitud.Cerrada, false)]
    [InlineData(EstadoSolicitud.Cancelada, false)]
    public void Solo_esta_vencida_si_el_limite_paso_y_el_estado_no_es_terminal(EstadoSolicitud estado, bool esperadoVencidaCuandoPaso)
    {
        var ahora = new DateTime(2026, 1, 16, 0, 0, 0, DateTimeKind.Utc);
        var limiteYaPasado = ahora.AddHours(-1);

        var vencida = CalculadoraSla.EstaVencida(limiteYaPasado, estado, ahora);

        Assert.Equal(esperadoVencidaCuandoPaso, vencida);
    }

    [Fact]
    public void No_esta_vencida_si_el_limite_todavia_no_llega()
    {
        var ahora = new DateTime(2026, 1, 16, 0, 0, 0, DateTimeKind.Utc);
        var limiteFuturo = ahora.AddHours(2);

        var vencida = CalculadoraSla.EstaVencida(limiteFuturo, EstadoSolicitud.Nueva, ahora);

        Assert.False(vencida);
    }
}
