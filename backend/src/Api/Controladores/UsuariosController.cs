using MesaSitec.Aplicacion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MesaSitec.Api.Controladores;

/// <summary>
/// Endpoint adicional (no forma parte de los 9 del contrato original) que
/// alimenta el selector de agente al ejecutar la acción "asignar" en el
/// frontend. Ver comentario en UsuarioService y DECISIONES.md.
/// </summary>
[ApiController]
[Authorize(Roles = "Admin,Agente")]
[Route("api/v1/usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly UsuarioService _usuarioService;

    public UsuariosController(UsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet("agentes")]
    public async Task<IActionResult> ListarAgentes()
    {
        var agentes = await _usuarioService.ListarAgentesAsignablesAsync();
        return Ok(agentes);
    }
}
