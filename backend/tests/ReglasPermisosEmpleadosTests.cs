using MesaSitec.Dominio.Enums;
using MesaSitec.Dominio.Reglas;
using Xunit;

namespace MesaSitec.Tests;

public class ReglasPermisosEmpleadosTests
{
    [Theory]
    [InlineData(RolUsuario.Admin, true)]
    [InlineData(RolUsuario.Agente, true)]
    [InlineData(RolUsuario.Solicitante, false)]
    public void Solo_admin_y_agente_pueden_ver_el_listado_de_empleados(RolUsuario rol, bool esperado)
    {
        Assert.Equal(esperado, ReglasPermisosEmpleados.PuedeVerEmpleados(rol));
    }

    [Theory]
    [InlineData(RolUsuario.Admin, true)]
    [InlineData(RolUsuario.Agente, true)]
    [InlineData(RolUsuario.Solicitante, false)]
    public void Solo_admin_y_agente_pueden_crear_o_editar_empleados(RolUsuario rol, bool esperado)
    {
        Assert.Equal(esperado, ReglasPermisosEmpleados.PuedeCrearEmpleado(rol));
        Assert.Equal(esperado, ReglasPermisosEmpleados.PuedeEditarEmpleado(rol));
    }

    [Theory]
    [InlineData(RolUsuario.Admin, true)]
    [InlineData(RolUsuario.Agente, false)]
    [InlineData(RolUsuario.Solicitante, false)]
    public void Solo_admin_puede_asignar_o_cambiar_el_rol_de_un_empleado(RolUsuario rol, bool esperado)
    {
        Assert.Equal(esperado, ReglasPermisosEmpleados.PuedeAsignarRol(rol));
    }

    [Theory]
    [InlineData(RolUsuario.Admin, true)]
    [InlineData(RolUsuario.Agente, false)]
    [InlineData(RolUsuario.Solicitante, false)]
    public void Solo_admin_puede_bloquear_o_desbloquear_empleados(RolUsuario rol, bool esperado)
    {
        Assert.Equal(esperado, ReglasPermisosEmpleados.PuedeBloquearODesbloquear(rol));
    }
}
