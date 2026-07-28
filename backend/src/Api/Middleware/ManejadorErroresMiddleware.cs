using System.Text.Json;
using MesaSitec.Aplicacion.Errores;

namespace MesaSitec.Api.Middleware;

/// <summary>
/// Traduce cualquier excepción (de negocio o no controlada) al formato
/// problem+json exigido en la sección 6.1. Ninguna excepción no controlada
/// debe llegar al cliente como un 500 con stack trace (sección 5.3).
/// </summary>
public class ManejadorErroresMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ManejadorErroresMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    public ManejadorErroresMiddleware(RequestDelegate next, ILogger<ManejadorErroresMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ApiException ex)
        {
            await EscribirRespuestaAsync(context, ex.Status, ex.Codigo, ex.TituloError, ex.Message, ex.Errores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado procesando {Path}", context.Request.Path);
            await EscribirRespuestaAsync(
                context, 500, "ERROR_INTERNO", "Error interno",
                "Ocurrió un error inesperado. Intenta nuevamente más tarde.", null);
        }
    }

    private static async Task EscribirRespuestaAsync(
        HttpContext context, int status, string codigo, string titulo, string detail,
        IDictionary<string, string[]>? errores)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = status;

        var body = new
        {
            type = $"https://mesasitec.local/errores/{ToSlug(codigo)}",
            title = titulo,
            status,
            detail,
            codigo,
            errores,
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
    }

    private static string ToSlug(string codigo) => codigo.ToLowerInvariant().Replace('_', '-');
}
