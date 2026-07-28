using MesaSitec.Dominio.Enums;
using MesaSitec.Dominio.Reglas;
using Xunit;

namespace MesaSitec.Tests;

public class MaquinaEstadosSolicitudTests
{
    [Theory]
    [InlineData(EstadoSolicitud.Nueva, "asignar", EstadoSolicitud.Asignada)]
    [InlineData(EstadoSolicitud.Nueva, "cancelar", EstadoSolicitud.Cancelada)]
    [InlineData(EstadoSolicitud.Asignada, "iniciar", EstadoSolicitud.EnProceso)]
    [InlineData(EstadoSolicitud.Asignada, "asignar", EstadoSolicitud.Asignada)]
    [InlineData(EstadoSolicitud.EnProceso, "resolver", EstadoSolicitud.Resuelta)]
    [InlineData(EstadoSolicitud.Resuelta, "cerrar", EstadoSolicitud.Cerrada)]
    [InlineData(EstadoSolicitud.Resuelta, "reabrir", EstadoSolicitud.EnProceso)]
    public void TryTransicionar_con_transicion_valida_devuelve_true_y_el_estado_correcto(
        EstadoSolicitud actual, string accion, EstadoSolicitud esperado)
    {
        var ok = MaquinaEstadosSolicitud.TryTransicionar(actual, accion, out var nuevo);

        Assert.True(ok);
        Assert.Equal(esperado, nuevo);
    }

    [Theory]
    [InlineData(EstadoSolicitud.Nueva, "resolver")]
    [InlineData(EstadoSolicitud.Nueva, "iniciar")]
    [InlineData(EstadoSolicitud.Cerrada, "reabrir")]
    [InlineData(EstadoSolicitud.Cancelada, "asignar")]
    [InlineData(EstadoSolicitud.Resuelta, "asignar")]
    public void TryTransicionar_con_transicion_invalida_devuelve_false(EstadoSolicitud actual, string accion)
    {
        var ok = MaquinaEstadosSolicitud.TryTransicionar(actual, accion, out var nuevo);

        Assert.False(ok);
        Assert.Equal(actual, nuevo); // el estado no debe cambiar
    }

    [Fact]
    public void Los_estados_finales_no_admiten_ninguna_accion()
    {
        foreach (var accion in MaquinaEstadosSolicitud.AccionesValidas)
        {
            Assert.False(MaquinaEstadosSolicitud.TryTransicionar(EstadoSolicitud.Cerrada, accion, out _));
            Assert.False(MaquinaEstadosSolicitud.TryTransicionar(EstadoSolicitud.Cancelada, accion, out _));
        }
    }

    [Fact]
    public void Una_accion_desconocida_no_es_valida()
    {
        Assert.False(MaquinaEstadosSolicitud.EsAccionConocida("teletransportar"));
    }
}
