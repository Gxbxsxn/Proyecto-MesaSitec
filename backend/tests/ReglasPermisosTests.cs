using MesaSitec.Dominio.Entidades;
using MesaSitec.Dominio.Enums;
using MesaSitec.Dominio.Reglas;
using Xunit;

namespace MesaSitec.Tests;

public class ReglasPermisosTests
{
    private static Solicitud NuevaSolicitud(Guid solicitanteId, EstadoSolicitud estado = EstadoSolicitud.Nueva) => new()
    {
        SolicitanteId = solicitanteId,
        Estado = estado,
    };

    [Theory]
    [InlineData(RolUsuario.Admin, "cancelar", true)]
    [InlineData(RolUsuario.Agente, "cancelar", false)]
    [InlineData(RolUsuario.Solicitante, "cancelar", false)]
    [InlineData(RolUsuario.Admin, "asignar", true)]
    [InlineData(RolUsuario.Agente, "asignar", true)]
    [InlineData(RolUsuario.Solicitante, "asignar", false)]
    [InlineData(RolUsuario.Solicitante, "resolver", false)]
    public void PuedeEjecutarAccion_respeta_la_tabla_de_permisos_por_rol(RolUsuario rol, string accion, bool esperado)
    {
        Assert.Equal(esperado, ReglasPermisos.PuedeEjecutarAccion(rol, accion));
    }

    [Fact]
    public void Un_solicitante_puede_editar_solo_su_propia_solicitud_en_estado_nueva()
    {
        var solicitanteId = Guid.NewGuid();
        var otroId = Guid.NewGuid();

        var propiaNueva = NuevaSolicitud(solicitanteId, EstadoSolicitud.Nueva);
        var propiaEnProceso = NuevaSolicitud(solicitanteId, EstadoSolicitud.EnProceso);
        var ajena = NuevaSolicitud(otroId, EstadoSolicitud.Nueva);

        Assert.True(ReglasPermisos.PuedeEditar(RolUsuario.Solicitante, solicitanteId, propiaNueva));
        Assert.False(ReglasPermisos.PuedeEditar(RolUsuario.Solicitante, solicitanteId, propiaEnProceso));
        Assert.False(ReglasPermisos.PuedeEditar(RolUsuario.Solicitante, solicitanteId, ajena));
    }

    [Fact]
    public void Un_admin_o_agente_puede_editar_cualquier_solicitud_de_su_organizacion()
    {
        var solicitud = NuevaSolicitud(Guid.NewGuid(), EstadoSolicitud.EnProceso);

        Assert.True(ReglasPermisos.PuedeEditar(RolUsuario.Admin, Guid.NewGuid(), solicitud));
        Assert.True(ReglasPermisos.PuedeEditar(RolUsuario.Agente, Guid.NewGuid(), solicitud));
    }

    [Fact]
    public void Solo_admin_y_agente_pueden_listar_todas_las_solicitudes()
    {
        Assert.True(ReglasPermisos.PuedeListarTodas(RolUsuario.Admin));
        Assert.True(ReglasPermisos.PuedeListarTodas(RolUsuario.Agente));
        Assert.False(ReglasPermisos.PuedeListarTodas(RolUsuario.Solicitante));
    }

    [Fact]
    public void Un_solicitante_puede_cerrar_unicamente_su_propia_solicitud()
    {
        var solicitanteId = Guid.NewGuid();
        var propia = NuevaSolicitud(solicitanteId, EstadoSolicitud.Resuelta);
        var ajena = NuevaSolicitud(Guid.NewGuid(), EstadoSolicitud.Resuelta);

        Assert.True(ReglasPermisos.PuedeCerrar(RolUsuario.Solicitante, solicitanteId, propia));
        Assert.False(ReglasPermisos.PuedeCerrar(RolUsuario.Solicitante, solicitanteId, ajena));
    }
}
