using MesaSitec.Aplicacion.DTOs;
using MesaSitec.Aplicacion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace MesaSitec.Api.Controladores;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var respuesta = await _authService.LoginAsync(request);
        return Ok(respuesta);
    }
}
