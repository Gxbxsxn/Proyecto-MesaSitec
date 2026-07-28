using MesaSitec.Dominio.Entidades;
using MesaSitec.Dominio.Enums;
using MesaSitec.Dominio.Reglas;
using Microsoft.AspNetCore.Identity;

namespace MesaSitec.Infraestructura.Persistencia;

/// <summary>
/// Siembra datos deterministas la primera vez que arranca la aplicación.
/// Todas las fechas se calculan como desplazamientos fijos respecto a
/// SEED_FECHA_BASE, nunca respecto a DateTime.UtcNow (sección 6.3).
/// </summary>
public static class DatosSemilla
{
    private const string PasswordSemilla = "Sitec.2026";

    public static async Task SembrarAsync(MesaSitecDbContext db, DateTime fechaBase)
    {
        if (await Task.FromResult(db.Tenants.Any()))
        {
            return; // ya sembrada
        }

        var hasher = new PasswordHasher<Usuario>();

        var norte = new Tenant { Nombre = "Cooperativa Norte", Activo = true };
        var sur = new Tenant { Nombre = "Bufete Sur", Activo = true };
        db.Tenants.AddRange(norte, sur);

        Usuario CrearUsuario(Tenant tenant, string email, string nombre, RolUsuario rol)
        {
            var u = new Usuario
            {
                TenantId = tenant.Id,
                Email = email,
                Nombre = nombre,
                Rol = rol,
                Activo = true,
            };
            u.PasswordHash = hasher.HashPassword(u, PasswordSemilla);
            return u;
        }

        var adminNorte = CrearUsuario(norte, "admin@norte.test", "Admin Norte", RolUsuario.Admin);
        var agente1Norte = CrearUsuario(norte, "agente1@norte.test", "Agente Uno Norte", RolUsuario.Agente);
        var agente2Norte = CrearUsuario(norte, "agente2@norte.test", "Agente Dos Norte", RolUsuario.Agente);
        var user1Norte = CrearUsuario(norte, "user1@norte.test", "Usuario Uno Norte", RolUsuario.Solicitante);
        var user2Norte = CrearUsuario(norte, "user2@norte.test", "Usuario Dos Norte", RolUsuario.Solicitante);

        var adminSur = CrearUsuario(sur, "admin@sur.test", "Admin Sur", RolUsuario.Admin);
        var user1Sur = CrearUsuario(sur, "user1@sur.test", "Usuario Uno Sur", RolUsuario.Solicitante);

        db.Usuarios.AddRange(adminNorte, agente1Norte, agente2Norte, user1Norte, user2Norte, adminSur, user1Sur);

        (Categoria incidente, Categoria requerimiento, Categoria consulta, Categoria fallaCritica) CrearCategorias(Tenant tenant)
        {
            var inc = new Categoria { TenantId = tenant.Id, Nombre = "Incidente", SlaHoras = 8, Activo = true };
            var req = new Categoria { TenantId = tenant.Id, Nombre = "Requerimiento", SlaHoras = 40, Activo = true };
            var con = new Categoria { TenantId = tenant.Id, Nombre = "Consulta", SlaHoras = 24, Activo = true };
            var crit = new Categoria { TenantId = tenant.Id, Nombre = "Falla crítica", SlaHoras = 4, Activo = true };
            db.Categorias.AddRange(inc, req, con, crit);
            return (inc, req, con, crit);
        }

        var catsNorte = CrearCategorias(norte);
        var catsSur = CrearCategorias(sur);

        // Ciclos deterministas de estado y prioridad para repartir la semilla.
        var estados = new[]
        {
            EstadoSolicitud.Nueva, EstadoSolicitud.Asignada, EstadoSolicitud.EnProceso,
            EstadoSolicitud.Resuelta, EstadoSolicitud.Cerrada, EstadoSolicitud.Cancelada
        };
        var prioridades = new[]
        {
            PrioridadSolicitud.Baja, PrioridadSolicitud.Media, PrioridadSolicitud.Alta, PrioridadSolicitud.Critica
        };

        void CrearSolicitudes(
            Tenant tenant,
            int cantidad,
            (Categoria incidente, Categoria requerimiento, Categoria consulta, Categoria fallaCritica) cats,
            Usuario[] solicitantes,
            Usuario[] agentes,
            int minResueltas)
        {
            var categorias = new[] { cats.incidente, cats.requerimiento, cats.consulta, cats.fallaCritica };
            var correlativo = 1;
            var resueltasCreadas = 0;

            for (var i = 0; i < cantidad; i++)
            {
                var estado = estados[i % estados.Length];
                // Aseguramos el mínimo de resueltas pedido, sustituyendo el ciclo cuando falte.
                if (i >= cantidad - (minResueltas - resueltasCreadas) && resueltasCreadas < minResueltas)
                {
                    estado = EstadoSolicitud.Resuelta;
                }
                if (estado == EstadoSolicitud.Resuelta) resueltasCreadas++;

                var prioridad = prioridades[i % prioridades.Length];
                var categoria = categorias[i % categorias.Length];
                var solicitante = solicitantes[i % solicitantes.Length];
                var fechaCreacion = fechaBase.AddHours(i * 3);
                var fechaLimite = CalculadoraSla.CalcularFechaLimite(fechaCreacion, categoria.SlaHoras, prioridad);

                var solicitud = new Solicitud
                {
                    TenantId = tenant.Id,
                    Codigo = $"SOL-{fechaBase.Year}-{correlativo:D5}",
                    Titulo = $"Solicitud semilla #{correlativo} de {tenant.Nombre}",
                    Descripcion = $"Descripción de ejemplo generada por los datos semilla para la solicitud número {correlativo}.",
                    CategoriaId = categoria.Id,
                    Prioridad = prioridad,
                    Estado = estado,
                    SolicitanteId = solicitante.Id,
                    FechaCreacion = fechaCreacion,
                    FechaLimiteSla = fechaLimite,
                };

                if (estado is EstadoSolicitud.Asignada or EstadoSolicitud.EnProceso or EstadoSolicitud.Resuelta or EstadoSolicitud.Cerrada)
                {
                    solicitud.AgenteId = agentes[i % agentes.Length].Id;
                }

                if (estado is EstadoSolicitud.Resuelta or EstadoSolicitud.Cerrada)
                {
                    solicitud.FechaResolucion = fechaLimite.AddHours(-1);
                    solicitud.MotivoResolucion = "Se resolvió la solicitud siguiendo el procedimiento estándar del equipo de soporte.";
                }

                if (estado == EstadoSolicitud.Cancelada)
                {
                    solicitud.MotivoCancelacion = "Solicitud duplicada, cancelada como parte de la limpieza de datos.";
                }

                db.Solicitudes.Add(solicitud);
                correlativo++;
            }
        }

        CrearSolicitudes(norte, 25, catsNorte, new[] { user1Norte, user2Norte }, new[] { agente1Norte, agente2Norte }, minResueltas: 3);
        CrearSolicitudes(sur, 8, catsSur, new[] { user1Sur }, new[] { adminSur }, minResueltas: 1);

        await db.SaveChangesAsync();
    }
}
