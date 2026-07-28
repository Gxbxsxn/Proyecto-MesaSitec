using MesaSitec.Aplicacion.DTOs;
using MesaSitec.Aplicacion.Errores;
using MesaSitec.Aplicacion.Interfaces;
using MesaSitec.Dominio.Entidades;
using MesaSitec.Dominio.Enums;
using MesaSitec.Dominio.Reglas;
using MesaSitec.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.Aplicacion.Servicios;

public class SolicitudService
{
    private readonly MesaSitecDbContext _db;
    private readonly ICurrentUser _usuarioActual;

    public SolicitudService(MesaSitecDbContext db, ICurrentUser usuarioActual)
    {
        _db = db;
        _usuarioActual = usuarioActual;
    }

    // ---------- Listado (RN-01, filtros del contrato 6.2 #4) ----------

    public async Task<SolicitudPaginadaDto> ListarAsync(ListadoSolicitudesQuery query)
    {
        if (query.Page < 1)
            throw ApiException.ParametroInvalido("El parámetro 'page' debe ser mayor o igual a 1.");
        if (query.PageSize > 100 || query.PageSize < 1)
            throw ApiException.ParametroInvalido("El parámetro 'pageSize' debe estar entre 1 y 100.");

        var consulta = _db.Solicitudes
            .Include(s => s.Categoria)
            .Include(s => s.Agente)
            .Where(s => s.TenantId == _usuarioActual.TenantId);

        // RN-03: un Solicitante solo ve las suyas en el listado.
        if (!ReglasPermisos.PuedeListarTodas(_usuarioActual.Rol))
        {
            consulta = consulta.Where(s => s.SolicitanteId == _usuarioActual.UsuarioId);
        }

        if (!string.IsNullOrWhiteSpace(query.Estado))
        {
            if (!Enum.TryParse<EstadoSolicitud>(query.Estado, true, out var estado))
                throw ApiException.ParametroInvalido($"Estado '{query.Estado}' no reconocido.");
            consulta = consulta.Where(s => s.Estado == estado);
        }

        if (!string.IsNullOrWhiteSpace(query.Prioridad))
        {
            if (!Enum.TryParse<PrioridadSolicitud>(query.Prioridad, true, out var prioridad))
                throw ApiException.ParametroInvalido($"Prioridad '{query.Prioridad}' no reconocida.");
            consulta = consulta.Where(s => s.Prioridad == prioridad);
        }

        if (query.CategoriaId.HasValue)
        {
            consulta = consulta.Where(s => s.CategoriaId == query.CategoriaId.Value);
        }

        if (query.AgenteId.HasValue)
        {
            consulta = consulta.Where(s => s.AgenteId == query.AgenteId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var termino = query.Q.Trim();
            consulta = consulta.Where(s =>
                EF.Functions.Like(s.Titulo, $"%{termino}%") ||
                EF.Functions.Like(s.Descripcion, $"%{termino}%") ||
                EF.Functions.Like(s.Codigo, $"%{termino}%"));
        }

        var ahora = DateTime.UtcNow;

        if (query.Vencidas.HasValue)
        {
            var estadosFinales = new[] { EstadoSolicitud.Resuelta, EstadoSolicitud.Cerrada, EstadoSolicitud.Cancelada };
            consulta = query.Vencidas.Value
                ? consulta.Where(s => s.FechaLimiteSla < ahora && !estadosFinales.Contains(s.Estado))
                : consulta.Where(s => s.FechaLimiteSla >= ahora || estadosFinales.Contains(s.Estado));
        }

        consulta = query.Sort switch
        {
            "fechaCreacion" => consulta.OrderBy(s => s.FechaCreacion),
            "-fechaCreacion" => consulta.OrderByDescending(s => s.FechaCreacion),
            "prioridad" => consulta.OrderBy(s => s.Prioridad),
            "-prioridad" => consulta.OrderByDescending(s => s.Prioridad),
            "codigo" => consulta.OrderBy(s => s.Codigo),
            _ => throw ApiException.ParametroInvalido($"Valor de 'sort' no soportado: '{query.Sort}'.")
        };

        var total = await consulta.CountAsync();
        var totalPaginas = total == 0 ? 0 : (int)Math.Ceiling(total / (double)query.PageSize);

        var items = await consulta
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        var itemsDto = items.Select(s => MapearListadoItem(s, ahora)).ToList();

        return new SolicitudPaginadaDto(itemsDto, query.Page, query.PageSize, total, totalPaginas);
    }

    // ---------- Detalle ----------

    public async Task<SolicitudDetalleDto> ObtenerDetalleAsync(Guid id)
    {
        var solicitud = await ObtenerEntidadOr404Async(id);

        if (!ReglasPermisos.PuedeVerDetalle(_usuarioActual.Rol, _usuarioActual.UsuarioId, solicitud))
        {
            // Por RN-01, un recurso "prohibido" para un Solicitante se comporta como no encontrado.
            throw ApiException.NoEncontrado();
        }

        return MapearDetalle(solicitud);
    }

    // ---------- Crear ----------

    public async Task<SolicitudDetalleDto> CrearAsync(CrearSolicitudRequest request)
    {
        var errores = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Titulo) || request.Titulo.Length < 5 || request.Titulo.Length > 120)
            errores["titulo"] = new[] { "El título debe tener entre 5 y 120 caracteres." };

        if (string.IsNullOrWhiteSpace(request.Descripcion) || request.Descripcion.Length < 10 || request.Descripcion.Length > 4000)
            errores["descripcion"] = new[] { "La descripción debe tener entre 10 y 4000 caracteres." };

        if (!Enum.TryParse<PrioridadSolicitud>(request.Prioridad, true, out var prioridad))
            errores["prioridad"] = new[] { "La prioridad no es válida." };

        var categoria = await _db.Categorias.FirstOrDefaultAsync(c =>
            c.Id == request.CategoriaId && c.TenantId == _usuarioActual.TenantId && c.Activo);
        if (categoria is null)
            errores["categoriaId"] = new[] { "La categoría no existe o no está activa." };

        if (errores.Count > 0)
            throw ApiException.Validacion(errores);

        var fechaCreacion = DateTime.UtcNow;
        var fechaLimite = CalculadoraSla.CalcularFechaLimite(fechaCreacion, categoria!.SlaHoras, prioridad);
        var codigo = await SiguienteCodigoAsync(fechaCreacion.Year);

        var solicitud = new Solicitud
        {
            TenantId = _usuarioActual.TenantId,
            Codigo = codigo,
            Titulo = request.Titulo.Trim(),
            Descripcion = request.Descripcion.Trim(),
            CategoriaId = categoria.Id,
            Prioridad = prioridad,
            Estado = EstadoSolicitud.Nueva,
            SolicitanteId = _usuarioActual.UsuarioId,
            FechaCreacion = fechaCreacion,
            FechaLimiteSla = fechaLimite,
        };

        _db.Solicitudes.Add(solicitud);
        await _db.SaveChangesAsync();

        solicitud.Categoria = categoria;
        return MapearDetalle(solicitud);
    }

    // ---------- Editar ----------

    public async Task<SolicitudDetalleDto> EditarAsync(Guid id, EditarSolicitudRequest request)
    {
        var solicitud = await ObtenerEntidadOr404Async(id);

        if (!ReglasPermisos.PuedeEditar(_usuarioActual.Rol, _usuarioActual.UsuarioId, solicitud))
        {
            // Si ni siquiera puede verla, es 404; si puede verla pero no editarla, es 403.
            if (!ReglasPermisos.PuedeVerDetalle(_usuarioActual.Rol, _usuarioActual.UsuarioId, solicitud))
                throw ApiException.NoEncontrado();
            throw ApiException.OperacionNoPermitida("No puedes editar esta solicitud en su estado o rol actual.");
        }

        var errores = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Titulo) || request.Titulo.Length < 5 || request.Titulo.Length > 120)
            errores["titulo"] = new[] { "El título debe tener entre 5 y 120 caracteres." };

        if (string.IsNullOrWhiteSpace(request.Descripcion) || request.Descripcion.Length < 10 || request.Descripcion.Length > 4000)
            errores["descripcion"] = new[] { "La descripción debe tener entre 10 y 4000 caracteres." };

        if (!Enum.TryParse<PrioridadSolicitud>(request.Prioridad, true, out var nuevaPrioridad))
            errores["prioridad"] = new[] { "La prioridad no es válida." };

        var categoria = await _db.Categorias.FirstOrDefaultAsync(c =>
            c.Id == request.CategoriaId && c.TenantId == _usuarioActual.TenantId && c.Activo);
        if (categoria is null)
            errores["categoriaId"] = new[] { "La categoría no existe o no está activa." };

        if (errores.Count > 0)
            throw ApiException.Validacion(errores);

        var cambioCategoriaOPrioridad = solicitud.CategoriaId != categoria!.Id || solicitud.Prioridad != nuevaPrioridad;

        solicitud.Titulo = request.Titulo.Trim();
        solicitud.Descripcion = request.Descripcion.Trim();
        solicitud.CategoriaId = categoria.Id;
        solicitud.Prioridad = nuevaPrioridad;

        // RN-04: cambiar prioridad o categoría recalcula el SLA si aún no está resuelta.
        // fechaCreacion nunca se modifica.
        if (cambioCategoriaOPrioridad && solicitud.Estado != EstadoSolicitud.Resuelta)
        {
            solicitud.FechaLimiteSla = CalculadoraSla.CalcularFechaLimite(solicitud.FechaCreacion, categoria.SlaHoras, nuevaPrioridad);
        }

        await _db.SaveChangesAsync();

        solicitud.Categoria = categoria;
        return MapearDetalle(solicitud);
    }

    // ---------- Transiciones ----------

    public async Task<SolicitudDetalleDto> EjecutarTransicionAsync(Guid id, TransicionRequest request)
    {
        var solicitud = await ObtenerEntidadOr404Async(id, incluirAgente: true);

        if (!ReglasPermisos.PuedeVerDetalle(_usuarioActual.Rol, _usuarioActual.UsuarioId, solicitud))
            throw ApiException.NoEncontrado();

        var accion = request.Accion?.Trim() ?? string.Empty;

        var tienePermisoDeAccion = accion == "cerrar"
            ? ReglasPermisos.PuedeCerrar(_usuarioActual.Rol, _usuarioActual.UsuarioId, solicitud)
            : ReglasPermisos.PuedeEjecutarAccion(_usuarioActual.Rol, accion);

        if (!tienePermisoDeAccion)
            throw ApiException.OperacionNoPermitida($"Tu rol no puede ejecutar la acción '{accion}'.");

        if (!MaquinaEstadosSolicitud.EsAccionConocida(accion) ||
            !MaquinaEstadosSolicitud.TryTransicionar(solicitud.Estado, accion, out var nuevoEstado))
        {
            throw ApiException.TransicionInvalida(
                $"No se puede aplicar '{accion}' sobre una solicitud en estado '{solicitud.Estado}'.");
        }

        switch (accion)
        {
            case "asignar":
                await ValidarYAplicarAgenteAsync(solicitud, request.AgenteId);
                break;

            case "resolver":
                if (string.IsNullOrWhiteSpace(request.Motivo) || request.Motivo.Trim().Length < 20)
                    throw ApiException.MotivoRequerido("El motivo de resolución debe tener al menos 20 caracteres.");
                solicitud.MotivoResolucion = request.Motivo.Trim();
                solicitud.FechaResolucion = DateTime.UtcNow;
                break;

            case "cancelar":
                if (string.IsNullOrWhiteSpace(request.Motivo) || request.Motivo.Trim().Length < 10)
                    throw ApiException.MotivoRequerido("El motivo de cancelación debe tener al menos 10 caracteres.");
                solicitud.MotivoCancelacion = request.Motivo.Trim();
                break;
        }

        solicitud.Estado = nuevoEstado;
        await _db.SaveChangesAsync();

        return MapearDetalle(solicitud);
    }

    private async Task ValidarYAplicarAgenteAsync(Solicitud solicitud, Guid? agenteId)
    {
        if (agenteId is null)
            throw ApiException.AgenteInvalido("Debes indicar un agenteId.");

        var agente = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == agenteId.Value);

        var esValido = agente is not null
            && agente.Activo
            && agente.TenantId == solicitud.TenantId
            && agente.Rol is RolUsuario.Agente or RolUsuario.Admin;

        if (!esValido)
            throw ApiException.AgenteInvalido();

        solicitud.AgenteId = agente!.Id;
    }

    // ---------- Utilidades privadas ----------

    private async Task<Solicitud> ObtenerEntidadOr404Async(Guid id, bool incluirAgente = false)
    {
        var query = _db.Solicitudes.Include(s => s.Categoria).AsQueryable();
        if (incluirAgente) query = query.Include(s => s.Agente);

        // RN-01: siempre se filtra por tenant. Si no aparece, es 404 (no 403).
        var solicitud = await query.FirstOrDefaultAsync(s => s.Id == id && s.TenantId == _usuarioActual.TenantId);
        if (solicitud is null)
            throw ApiException.NoEncontrado();

        return solicitud;
    }

    private async Task<string> SiguienteCodigoAsync(int anio)
    {
        var prefijo = $"SOL-{anio}-";
        var codigos = await _db.Solicitudes
            .Where(s => s.TenantId == _usuarioActual.TenantId && s.Codigo.StartsWith(prefijo))
            .Select(s => s.Codigo)
            .ToListAsync();

        var maximo = codigos
            .Select(c => int.TryParse(c[(prefijo.Length)..], out var n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max();

        return $"{prefijo}{(maximo + 1):D5}";
    }

    private static SolicitudListadoItemDto MapearListadoItem(Solicitud s, DateTime ahora) => new(
        s.Id,
        s.Codigo,
        s.Titulo,
        s.Estado.ToString(),
        s.Prioridad.ToString(),
        new CategoriaResumenDto(s.Categoria!.Id, s.Categoria.Nombre),
        s.Agente is null ? null : new AgenteResumenDto(s.Agente.Id, s.Agente.Nombre),
        s.FechaCreacion,
        s.FechaLimiteSla,
        CalculadoraSla.EstaVencida(s.FechaLimiteSla, s.Estado, ahora)
    );

    private static SolicitudDetalleDto MapearDetalle(Solicitud s)
    {
        var ahora = DateTime.UtcNow;
        return new SolicitudDetalleDto(
            s.Id,
            s.Codigo,
            s.Titulo,
            s.Descripcion,
            s.Estado.ToString(),
            s.Prioridad.ToString(),
            new CategoriaResumenDto(s.Categoria!.Id, s.Categoria.Nombre),
            s.Agente is null ? null : new AgenteResumenDto(s.Agente.Id, s.Agente.Nombre),
            new SolicitanteResumenDto(s.SolicitanteId, s.Solicitante?.Nombre ?? string.Empty),
            s.FechaCreacion,
            s.FechaLimiteSla,
            CalculadoraSla.EstaVencida(s.FechaLimiteSla, s.Estado, ahora),
            s.FechaResolucion,
            s.MotivoResolucion,
            s.MotivoCancelacion
        );
    }
}
