using MesaSitec.Aplicacion.DTOs;
using MesaSitec.Aplicacion.Errores;
using MesaSitec.Aplicacion.Interfaces;
using MesaSitec.Aplicacion.Servicios;
using MesaSitec.Infraestructura.Persistencia;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.Api.Controladores;

[ApiController]
[Authorize]
[Route("api/v1/me")]
public class MeController : ControllerBase
{
    private readonly MesaSitecDbContext _db;
    private readonly ICurrentUser _usuarioActual;

    public MeController(MesaSitecDbContext db, ICurrentUser usuarioActual)
    {
        _db = db;
        _usuarioActual = usuarioActual;
    }

    [HttpGet]
    [ProducesResponseType(typeof(UsuarioDto), 200)]
    public async Task<IActionResult> Obtener()
    {
        var usuario = await _db.Usuarios
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Id == _usuarioActual.UsuarioId);

        if (usuario is null) throw ApiException.NoAutenticado();

        return Ok(AuthService.MapearUsuario(usuario));
    }
}
