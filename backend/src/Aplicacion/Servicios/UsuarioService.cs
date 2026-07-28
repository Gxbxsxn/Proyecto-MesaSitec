using MesaSitec.Aplicacion.DTOs;
using MesaSitec.Aplicacion.Interfaces;
using MesaSitec.Dominio.Enums;
using MesaSitec.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.Aplicacion.Servicios;

/// <summary>
/// El enunciado no incluye un endpoint para listar usuarios/agentes, pero la
/// pantalla de detalle necesita poblar el selector de agente al ejecutar
/// "asignar" (RN-05). Se agrega este endpoint adicional, declarado en
/// DECISIONES.md, sin tocar ninguno de los 9 endpoints del contrato original.
/// </summary>
public class UsuarioService
{
    private readonly MesaSitecDbContext _db;
    private readonly ICurrentUser _usuarioActual;

    public UsuarioService(MesaSitecDbContext db, ICurrentUser usuarioActual)
    {
        _db = db;
        _usuarioActual = usuarioActual;
    }

    public async Task<IReadOnlyList<AgenteDisponibleDto>> ListarAgentesAsignablesAsync()
    {
        return await _db.Usuarios
            .Where(u => u.TenantId == _usuarioActual.TenantId
                        && u.Activo
                        && (u.Rol == RolUsuario.Agente || u.Rol == RolUsuario.Admin))
            .OrderBy(u => u.Nombre)
            .Select(u => new AgenteDisponibleDto(u.Id, u.Nombre, u.Email))
            .ToListAsync();
    }
}
