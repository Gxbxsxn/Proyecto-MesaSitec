using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MesaSitec.Api.Controladores;

[ApiController]
[AllowAnonymous]
[Route("api/v1/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Obtener() => Ok(new { estado = "ok" });
}
