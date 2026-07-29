using MesaSitec.Aplicacion.DTOs;
using MesaSitec.Aplicacion.Errores;
using MesaSitec.Aplicacion.Interfaces;
using MesaSitec.Dominio.Entidades;
using MesaSitec.Dominio.Enums;
using MesaSitec.Dominio.Reglas;
using MesaSitec.Infraestructura.Persistencia;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.Aplicacion.Servicios;

/// <summary>
/// RN-08 (nueva) — Módulo de gestión de empleados. Todo queda siempre
/// filtrado por el tenant del usuario autenticado (RN-01): un Admin de
/// "Cooperativa Norte" jamás ve ni afecta empleados de "Bufete Sur".
/// </summary>
public class EmpleadoService
{
    private readonly MesaSitecDbContext _db;
    private readonly ICurrentUser _usuarioActual;
    private readonly PasswordHasher<Usuario> _hasher = new();

    public EmpleadoService(MesaSitecDbContext db, ICurrentUser usuarioActual)
    {
        _db = db;
        _usuarioActual = usuarioActual;
    }

    public async Task<IReadOnlyList<EmpleadoListadoItemDto>> ListarAsync(string? rol, string? q)
    {
        var consulta = _db.Usuarios.Where(u => u.TenantId == _usuarioActual.TenantId).AsQueryable();

        if (!string.IsNullOrWhiteSpace(rol))
        {
            if (!Enum.TryParse<RolUsuario>(rol, true, out var rolFiltro))
                throw ApiException.ParametroInvalido($"Rol '{rol}' no reconocido.");
            consulta = consulta.Where(u => u.Rol == rolFiltro);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var termino = q.Trim();
            consulta = consulta.Where(u => EF.Functions.Like(u.Nombre, $"%{termino}%") || EF.Functions.Like(u.Email, $"%{termino}%"));
        }

        var empleados = await consulta.OrderBy(u => u.Nombre).ToListAsync();
        return empleados.Select(Mapear).ToList();
    }

    public async Task<EmpleadoListadoItemDto> CrearAsync(CrearEmpleadoRequest request)
    {
        var errores = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Nombre) || request.Nombre.Trim().Length < 3 || request.Nombre.Trim().Length > 120)
            errores["nombre"] = new[] { "El nombre debe tener entre 3 y 120 caracteres." };

        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
            errores["email"] = new[] { "El email no es válido." };

        if (!Enum.TryParse<RolUsuario>(request.Rol, true, out var rol))
            errores["rol"] = new[] { "El rol no es válido." };

        if (string.IsNullOrWhiteSpace(request.PasswordTemporal) || request.PasswordTemporal.Length < 8)
            errores["passwordTemporal"] = new[] { "La contraseña temporal debe tener al menos 8 caracteres." };

        if (errores.Count > 0)
            throw ApiException.Validacion(errores);

        // Solo Admin puede decidir el rol; un Agente que intente crear un Admin/Agente distinto de Solicitante se rechaza.
        if (!ReglasPermisosEmpleados.PuedeAsignarRol(_usuarioActual.Rol) && rol != RolUsuario.Solicitante)
            throw ApiException.OperacionNoPermitida("Solo un Admin puede crear empleados con rol Admin o Agente.");

        var emailNormalizado = request.Email.Trim().ToLowerInvariant();
        var existe = await _db.Usuarios.AnyAsync(u => u.Email == emailNormalizado);
        if (existe)
            throw ApiException.EmailDuplicado();

        var usuario = new Usuario
        {
            TenantId = _usuarioActual.TenantId,
            Nombre = request.Nombre.Trim(),
            Email = emailNormalizado,
            Rol = rol,
            Activo = true,
        };
        usuario.PasswordHash = _hasher.HashPassword(usuario, request.PasswordTemporal);

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        return Mapear(usuario);
    }

    public async Task<EmpleadoListadoItemDto> EditarAsync(Guid id, EditarEmpleadoRequest request)
    {
        var usuario = await ObtenerOr404Async(id);

        var errores = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Nombre) || request.Nombre.Trim().Length < 3 || request.Nombre.Trim().Length > 120)
            errores["nombre"] = new[] { "El nombre debe tener entre 3 y 120 caracteres." };

        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
            errores["email"] = new[] { "El email no es válido." };

        RolUsuario? nuevoRol = null;
        if (!string.IsNullOrWhiteSpace(request.Rol))
        {
            if (!Enum.TryParse<RolUsuario>(request.Rol, true, out var rolParseado))
                errores["rol"] = new[] { "El rol no es válido." };
            else
                nuevoRol = rolParseado;
        }

        if (errores.Count > 0)
            throw ApiException.Validacion(errores);

        var emailNormalizado = request.Email.Trim().ToLowerInvariant();
        var emailEnUso = await _db.Usuarios.AnyAsync(u => u.Email == emailNormalizado && u.Id != id);
        if (emailEnUso)
            throw ApiException.EmailDuplicado();

        // RN-08: solo Admin puede cambiar el rol. Un Agente puede editar nombre/email libremente,
        // pero cualquier intento de cambiar el rol desde un Agente se rechaza explícitamente
        // (no se ignora en silencio, para que la UI no dé una falsa sensación de éxito parcial).
        if (nuevoRol.HasValue && nuevoRol.Value != usuario.Rol && !ReglasPermisosEmpleados.PuedeAsignarRol(_usuarioActual.Rol))
            throw ApiException.OperacionNoPermitida("Solo un Admin puede cambiar el rol de un empleado.");

        usuario.Nombre = request.Nombre.Trim();
        usuario.Email = emailNormalizado;
        if (nuevoRol.HasValue) usuario.Rol = nuevoRol.Value;

        await _db.SaveChangesAsync();
        return Mapear(usuario);
    }

    public async Task<EmpleadoListadoItemDto> BloquearAsync(Guid id)
    {
        var usuario = await ObtenerOr404Async(id);

        if (usuario.Id == _usuarioActual.UsuarioId)
            throw ApiException.AutoBloqueoNoPermitido();

        if (usuario.Rol == RolUsuario.Admin)
        {
            var otrosAdminsActivos = await _db.Usuarios.CountAsync(u =>
                u.TenantId == _usuarioActual.TenantId && u.Rol == RolUsuario.Admin && u.Activo && u.Id != usuario.Id);
            if (otrosAdminsActivos == 0)
                throw ApiException.UltimoAdminActivo();
        }

        usuario.Activo = false;
        await _db.SaveChangesAsync();
        return Mapear(usuario);
    }

    public async Task<EmpleadoListadoItemDto> DesbloquearAsync(Guid id)
    {
        var usuario = await ObtenerOr404Async(id);
        usuario.Activo = true;
        await _db.SaveChangesAsync();
        return Mapear(usuario);
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

    private async Task<Usuario> ObtenerOr404Async(Guid id)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id && u.TenantId == _usuarioActual.TenantId);
        if (usuario is null) throw ApiException.NoEncontrado();
        return usuario;
    }

    private static EmpleadoListadoItemDto Mapear(Usuario u) => new(u.Id, u.Nombre, u.Email, u.Rol.ToString(), u.Activo);
}
