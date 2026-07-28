namespace MesaSitec.Aplicacion.DTOs;

public record CategoriaResumenDto(Guid Id, string Nombre);
public record AgenteResumenDto(Guid Id, string Nombre);
public record SolicitanteResumenDto(Guid Id, string Nombre);

public record SolicitudListadoItemDto(
    Guid Id,
    string Codigo,
    string Titulo,
    string Estado,
    string Prioridad,
    CategoriaResumenDto Categoria,
    AgenteResumenDto? Agente,
    DateTime FechaCreacion,
    DateTime FechaLimiteSla,
    bool Vencida
);

public record SolicitudDetalleDto(
    Guid Id,
    string Codigo,
    string Titulo,
    string Descripcion,
    string Estado,
    string Prioridad,
    CategoriaResumenDto Categoria,
    AgenteResumenDto? Agente,
    SolicitanteResumenDto Solicitante,
    DateTime FechaCreacion,
    DateTime FechaLimiteSla,
    bool Vencida,
    DateTime? FechaResolucion,
    string? MotivoResolucion,
    string? MotivoCancelacion
);

public record SolicitudPaginadaDto(
    IReadOnlyList<SolicitudListadoItemDto> Items,
    int Page,
    int PageSize,
    int Total,
    int TotalPaginas
);

public record CrearSolicitudRequest(
    string Titulo,
    string Descripcion,
    Guid CategoriaId,
    string Prioridad
);

public record EditarSolicitudRequest(
    string Titulo,
    string Descripcion,
    Guid CategoriaId,
    string Prioridad
);

public record TransicionRequest(
    string Accion,
    Guid? AgenteId,
    string? Motivo
);

public record ListadoSolicitudesQuery(
    string? Estado,
    string? Prioridad,
    Guid? CategoriaId,
    Guid? AgenteId,
    string? Q,
    bool? Vencidas,
    int Page = 1,
    int PageSize = 20,
    string Sort = "-fechaCreacion"
);
