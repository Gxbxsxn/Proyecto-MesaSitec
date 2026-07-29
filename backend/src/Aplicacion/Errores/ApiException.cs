namespace MesaSitec.Aplicacion.Errores;

/// <summary>
/// Excepción de dominio/aplicación que el middleware global traduce
/// directamente al formato problem+json exigido en la sección 6.1.
/// </summary>
public class ApiException : Exception
{
    public int Status { get; }
    public string Codigo { get; }
    public string TituloError { get; }
    public IDictionary<string, string[]>? Errores { get; }

    public ApiException(int status, string codigo, string titulo, string detail, IDictionary<string, string[]>? errores = null)
        : base(detail)
    {
        Status = status;
        Codigo = codigo;
        TituloError = titulo;
        Errores = errores;
    }

    public static ApiException NoAutenticado(string detail = "Token ausente, inválido o expirado.")
        => new(401, "NO_AUTENTICADO", "No autenticado", detail);

    public static ApiException OperacionNoPermitida(string detail = "Tu rol no permite realizar esta operación.")
        => new(403, "OPERACION_NO_PERMITIDA", "Operación no permitida", detail);

    public static ApiException NoEncontrado(string detail = "El recurso solicitado no existe.")
        => new(404, "RECURSO_NO_ENCONTRADO", "Recurso no encontrado", detail);

    public static ApiException TransicionInvalida(string detail)
        => new(409, "TRANSICION_INVALIDA", "Transición inválida", detail);

    public static ApiException AgenteInvalido(string detail = "El agente indicado no es válido para esta operación.")
        => new(422, "AGENTE_INVALIDO", "Agente inválido", detail);

    public static ApiException MotivoRequerido(string detail)
        => new(422, "MOTIVO_REQUERIDO", "Motivo requerido", detail);

    public static ApiException ParametroInvalido(string detail)
        => new(400, "PARAMETRO_INVALIDO", "Parámetro inválido", detail);

    public static ApiException Validacion(IDictionary<string, string[]> errores, string detail = "Uno o más campos no son válidos.")
        => new(422, "VALIDACION", "Error de validación", detail, errores);

    public static ApiException EmailDuplicado(string detail = "Ya existe un usuario con ese email en tu organización.")
        => new(409, "EMAIL_DUPLICADO", "Email duplicado", detail);

    public static ApiException AutoBloqueoNoPermitido(string detail = "No puedes bloquear tu propio usuario.")
        => new(409, "AUTO_BLOQUEO_NO_PERMITIDO", "Operación no permitida", detail);

    public static ApiException UltimoAdminActivo(string detail = "No puedes bloquear al último administrador activo de tu organización.")
        => new(409, "ULTIMO_ADMIN_ACTIVO", "Operación no permitida", detail);
}
