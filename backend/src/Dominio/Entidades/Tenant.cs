namespace MesaSitec.Dominio.Entidades;

public class Tenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    public ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();
    public ICollection<Solicitud> Solicitudes { get; set; } = new List<Solicitud>();
}
