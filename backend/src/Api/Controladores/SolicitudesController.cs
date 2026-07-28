using MesaSitec.Aplicacion.DTOs;
using MesaSitec.Aplicacion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MesaSitec.Api.Controladores;

[ApiController]
[Authorize]
[Route("api/v1/solicitudes")]
public class SolicitudesController : ControllerBase
{
    private readonly SolicitudService _solicitudService;

    public SolicitudesController(SolicitudService solicitudService)
    {
        _solicitudService = solicitudService;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? estado,
        [FromQuery] string? prioridad,
        [FromQuery] Guid? categoriaId,
        [FromQuery] Guid? agenteId,
        [FromQuery] string? q,
        [FromQuery] bool? vencidas,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string sort = "-fechaCreacion")
    {
        var query = new ListadoSolicitudesQuery(estado, prioridad, categoriaId, agenteId, q, vencidas, page, pageSize, sort);
        var resultado = await _solicitudService.ListarAsync(query);
        return Ok(resultado);
    }

    [HttpPost]
    [ProducesResponseType(typeof(SolicitudDetalleDto), 201)]
    public async Task<IActionResult> Crear([FromBody] CrearSolicitudRequest request)
    {
        var creada = await _solicitudService.CrearAsync(request);
        return CreatedAtAction(nameof(ObtenerDetalle), new { id = creada.Id }, creada);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObtenerDetalle(Guid id)
    {
        var detalle = await _solicitudService.ObtenerDetalleAsync(id);
        return Ok(detalle);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Editar(Guid id, [FromBody] EditarSolicitudRequest request)
    {
        var actualizada = await _solicitudService.EditarAsync(id, request);
        return Ok(actualizada);
    }

    [HttpPost("{id:guid}/transiciones")]
    public async Task<IActionResult> EjecutarTransicion(Guid id, [FromBody] TransicionRequest request)
    {
        var actualizada = await _solicitudService.EjecutarTransicionAsync(id, request);
        return Ok(actualizada);
    }
}
