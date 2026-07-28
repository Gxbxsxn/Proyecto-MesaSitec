using MesaSitec.Aplicacion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MesaSitec.Api.Controladores;

[ApiController]
[Authorize]
[Route("api/v1/categorias")]
public class CategoriasController : ControllerBase
{
    private readonly CategoriaService _categoriaService;

    public CategoriasController(CategoriaService categoriaService)
    {
        _categoriaService = categoriaService;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var categorias = await _categoriaService.ListarActivasAsync();
        return Ok(categorias);
    }
}
