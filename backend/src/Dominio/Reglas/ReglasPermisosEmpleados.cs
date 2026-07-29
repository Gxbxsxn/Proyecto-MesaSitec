using MesaSitec.Dominio.Enums;

namespace MesaSitec.Dominio.Reglas;

/// <summary>
/// RN-08 (nueva) — Permisos del módulo de empleados.
///
/// Regla acordada con el cliente: Admin y Agente pueden ver, crear y editar
/// empleados de su propia organización. Pero solo Admin puede cambiar el rol
/// de un empleado o bloquearlo/desbloquearlo. No existe "eliminar": un
/// usuario referenciado por solicitudes no se puede borrar sin romper
/// integridad referencial, así que la acción destructiva es "bloquear"
/// (Activo = false), no un DELETE real. Ver DECISIONES.md.
/// </summary>
public static class ReglasPermisosEmpleados
{
    public static bool PuedeVerEmpleados(RolUsuario rol) => rol is RolUsuario.Admin or RolUsuario.Agente;

    public static bool PuedeCrearEmpleado(RolUsuario rol) => rol is RolUsuario.Admin or RolUsuario.Agente;

    public static bool PuedeEditarEmpleado(RolUsuario rol) => rol is RolUsuario.Admin or RolUsuario.Agente;

    /// <summary>Solo Admin puede asignar o cambiar el rol de un empleado (evita que un Agente se auto-promueva).</summary>
    public static bool PuedeAsignarRol(RolUsuario rol) => rol == RolUsuario.Admin;

    /// <summary>Solo Admin puede bloquear o desbloquear cuentas.</summary>
    public static bool PuedeBloquearODesbloquear(RolUsuario rol) => rol == RolUsuario.Admin;
}
