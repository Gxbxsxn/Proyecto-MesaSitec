using MesaSitec.Aplicacion.DTOs;
using MesaSitec.Aplicacion.Interfaces;
using MesaSitec.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.Aplicacion.Servicios;

public class CategoriaService
{
    private readonly MesaSitecDbContext _db;
    private readonly ICurrentUser _usuarioActual;

    public CategoriaService(MesaSitecDbContext db, ICurrentUser usuarioActual)
    {
        _db = db;
        _usuarioActual = usuarioActual;
    }

    public async Task<IReadOnlyList<CategoriaDto>> ListarActivasAsync()
    {
        return await _db.Categorias
            .Where(c => c.TenantId == _usuarioActual.TenantId && c.Activo)
            .OrderBy(c => c.Nombre)
            .Select(c => new CategoriaDto(c.Id, c.Nombre, c.SlaHoras))
            .ToListAsync();
    }
}
