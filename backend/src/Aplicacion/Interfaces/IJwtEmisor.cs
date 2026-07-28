using MesaSitec.Dominio.Entidades;

namespace MesaSitec.Aplicacion.Interfaces;

public interface IJwtEmisor
{
    /// <returns>El token firmado y los segundos hasta su expiración.</returns>
    (string token, int expiraEnSegundos) EmitirToken(Usuario usuario);
}
