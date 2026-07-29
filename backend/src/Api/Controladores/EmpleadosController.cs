using MesaSitec.Aplicacion.DTOs;
using MesaSitec.Aplicacion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MesaSitec.Api.Controladores;

/// <summary>
/// RN-08 (nueva) — Módulo de empleados. No forma parte de los 9 endpoints
/// del contrato original; se agrega para la gestión de personal pedida
/// (ver empleados, bloquear/desbloquear, alta con permisos por rol) y para
/// alimentar el selector de agente al "asignar" una solicitud. Declarado en
/// DECISIONES.md. Todo queda fuera del alcance de un Solicitante.
/// </summary>
[ApiController]
[Authorize(Roles = "Admin,Agente")]
[Route("api/v1")]
public class EmpleadosController : ControllerBase
{
    private readonly EmpleadoService _empleadoService;

    public EmpleadosController(EmpleadoService empleadoService)
    {
        _empleadoService = empleadoService;
    }

    [HttpGet("usuarios/agentes")]
    public async Task<IActionResult> ListarAgentesAsignables()
    {
        var agentes = await _empleadoService.ListarAgentesAsignablesAsync();
        return Ok(agentes);
    }

    [HttpGet("empleados")]
    public async Task<IActionResult> Listar([FromQuery] string? rol, [FromQuery] string? q)
    {
        var empleados = await _empleadoService.ListarAsync(rol, q);
        return Ok(empleados);
    }

    [HttpPost("empleados")]
    [ProducesResponseType(typeof(EmpleadoListadoItemDto), 201)]
    public async Task<IActionResult> Crear([FromBody] CrearEmpleadoRequest request)
    {
        var creado = await _empleadoService.CrearAsync(request);
        return CreatedAtAction(nameof(Listar), creado);
    }

    [HttpPut("empleados/{id:guid}")]
    public async Task<IActionResult> Editar(Guid id, [FromBody] EditarEmpleadoRequest request)
    {
        var actualizado = await _empleadoService.EditarAsync(id, request);
        return Ok(actualizado);
    }

    [HttpPost("empleados/{id:guid}/bloquear")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Bloquear(Guid id)
    {
        var actualizado = await _empleadoService.BloquearAsync(id);
        return Ok(actualizado);
    }

    [HttpPost("empleados/{id:guid}/desbloquear")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Desbloquear(Guid id)
    {
        var actualizado = await _empleadoService.DesbloquearAsync(id);
        return Ok(actualizado);
    }
}
