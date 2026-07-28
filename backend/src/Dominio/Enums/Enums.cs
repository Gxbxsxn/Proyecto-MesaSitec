namespace MesaSitec.Dominio.Enums;

public enum RolUsuario
{
    Admin,
    Agente,
    Solicitante
}

public enum PrioridadSolicitud
{
    Baja,
    Media,
    Alta,
    Critica
}

public enum EstadoSolicitud
{
    Nueva,
    Asignada,
    EnProceso,
    Resuelta,
    Cerrada,
    Cancelada
}
