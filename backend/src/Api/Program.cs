using System.Text;
using MesaSitec.Aplicacion.Interfaces;
using MesaSitec.Aplicacion.Servicios;
using MesaSitec.Api.Configuracion;
using MesaSitec.Api.Middleware;
using MesaSitec.Infraestructura.Persistencia;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ---------- Configuración base ----------

var jwtSecret = builder.Configuration["JWT_SECRET"]
    ?? Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? throw new InvalidOperationException("Falta la variable de entorno JWT_SECRET.");

var connectionString = builder.Configuration["ConnectionStrings:Default"]
    ?? Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? "Data Source=mesasitec.db";

const string CorsPolicy = "FrontendLocal";
var frontendOrigin = Environment.GetEnvironmentVariable("FRONTEND_ORIGIN") ?? "http://localhost:5173";

// ---------- Servicios ----------

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<MesaSitecDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IJwtEmisor, JwtEmisor>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CategoriaService>();
builder.Services.AddScoped<SolicitudService>();
builder.Services.AddScoped<EmpleadoService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
        policy.WithOrigins(frontendOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "MesaSitec API",
        Version = "v1",
        Description = "Mesa de servicio multi-tenant — prueba técnica Sitecpro."
    });

    var esquemaBearer = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Ingresa el token con el prefijo 'Bearer '."
    };
    options.AddSecurityDefinition("Bearer", esquemaBearer);
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        { new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            { Reference = new Microsoft.OpenApi.Models.OpenApiReference { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
          Array.Empty<string>() }
    });
});

var app = builder.Build();

// ---------- Migraciones + semilla automáticas al arrancar (sección 5.1 / 6.3) ----------

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MesaSitecDbContext>();
    db.Database.EnsureCreated();

    var fechaBaseTexto = Environment.GetEnvironmentVariable("SEED_FECHA_BASE") ?? "2026-01-15T08:00:00Z";
    var fechaBase = DateTime.Parse(fechaBaseTexto, null, System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal);
    await DatosSemilla.SembrarAsync(db, fechaBase);
}

// ---------- Middleware pipeline ----------

app.UseMiddleware<ManejadorErroresMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(CorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Necesario para que el proyecto de pruebas pueda usar WebApplicationFactory<Program>.
public partial class Program { }
