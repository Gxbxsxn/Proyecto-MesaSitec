using MesaSitec.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace MesaSitec.Infraestructura.Persistencia;

public class MesaSitecDbContext : DbContext
{
    public MesaSitecDbContext(DbContextOptions<MesaSitecDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Solicitud> Solicitudes => Set<Solicitud>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tenant>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Nombre).IsRequired();
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Email).IsRequired();
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Rol).HasConversion<string>();
            e.HasOne(u => u.Tenant).WithMany(t => t.Usuarios).HasForeignKey(u => u.TenantId);
        });

        modelBuilder.Entity<Categoria>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Nombre).IsRequired();
            e.HasOne(c => c.Tenant).WithMany(t => t.Categorias).HasForeignKey(c => c.TenantId);
        });

        modelBuilder.Entity<Solicitud>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Codigo).IsRequired();
            // Único por tenant, no global (RN-07: el correlativo es por organización).
            e.HasIndex(s => new { s.TenantId, s.Codigo }).IsUnique();
            e.Property(s => s.Prioridad).HasConversion<string>();
            e.Property(s => s.Estado).HasConversion<string>();

            e.HasOne(s => s.Tenant).WithMany(t => t.Solicitudes).HasForeignKey(s => s.TenantId);
            e.HasOne(s => s.Categoria).WithMany().HasForeignKey(s => s.CategoriaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.Solicitante).WithMany().HasForeignKey(s => s.SolicitanteId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.Agente).WithMany().HasForeignKey(s => s.AgenteId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
