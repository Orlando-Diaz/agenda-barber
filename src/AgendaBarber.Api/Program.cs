using System.Text;
using AgendaBarber.Api.Seguridad;
using AgendaBarber.Application.Autenticacion;
using AgendaBarber.Application.Barberias;
using AgendaBarber.Application.Barberos;
using AgendaBarber.Application.Citas;
using AgendaBarber.Application.Disponibilidad;
using AgendaBarber.Application.Servicios;
using AgendaBarber.Infrastructure;
using AgendaBarber.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddExceptionHandler<ManejadorDeExcepciones>();
builder.Services.AddProblemDetails();

// Detrás de un proxy (Render, nginx...) la IP real del cliente llega en X-Forwarded-For.
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    o.KnownNetworks.Clear();
    o.KnownProxies.Clear();
});

// Límite de intentos en login y registro: frena a quien prueba contraseñas sin parar.
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy("auth", contexto => RateLimitPartition.GetFixedWindowLimiter(
        contexto.Connection.RemoteIpAddress?.ToString() ?? "desconocida",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
    o.OnRejected = async (ctx, ct) =>
    {
        ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await ctx.HttpContext.Response.WriteAsJsonAsync(new { error = "Demasiados intentos. Espera un minuto e intenta de nuevo." }, ct);
    };
});
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton(TimeProvider.System);

// Casos de uso
builder.Services.AddScoped<ObtenerBarberiaPorSlug>();
builder.Services.AddScoped<ListarBarberias>();
builder.Services.AddScoped<ActualizarPerfilBarberia>();
builder.Services.AddScoped<GuardarFotoBarberia>();
builder.Services.AddScoped<QuitarFotoBarberia>();
builder.Services.AddScoped<ObtenerFotoBarberia>();
builder.Services.AddScoped<CrearServicio>();
builder.Services.AddScoped<ListarServicios>();
builder.Services.AddScoped<EditarServicio>();
builder.Services.AddScoped<QuitarServicio>();
builder.Services.AddScoped<CrearBarbero>();
builder.Services.AddScoped<AgregarHorarioBarbero>();
builder.Services.AddScoped<ListarBarberos>();
builder.Services.AddScoped<EditarBarbero>();
builder.Services.AddScoped<QuitarBarbero>();
builder.Services.AddScoped<QuitarHorarioBarbero>();
builder.Services.AddScoped<ConsultarDisponibilidad>();
builder.Services.AddScoped<ReservarCita>();
builder.Services.AddScoped<ConsultarAgenda>();
builder.Services.AddScoped<CambiarEstadoCita>();
builder.Services.AddScoped<RegistrarDueno>();
builder.Services.AddScoped<IniciarSesion>();

// CORS: qué páginas web pueden llamar a la API desde el navegador (ver "Cors:Origenes" en appsettings)
var origenes = builder.Configuration.GetSection("Cors:Origenes").Get<string[]>() ?? [];
builder.Services.AddCors(opciones => opciones.AddPolicy("Web", politica => politica
    .WithOrigins(origenes)
    .AllowAnyHeader()
    .AllowAnyMethod()));

// Autenticación con JWT
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOpciones>()
    ?? throw new InvalidOperationException("Falta la sección 'Jwt' en la configuración.");
if (Encoding.UTF8.GetByteCount(jwt.Clave) < 32)
    throw new InvalidOperationException("Jwt:Clave debe tener al menos 32 caracteres.");

if (!builder.Environment.IsDevelopment() && jwt.Clave.Contains("solo-para-desarrollo"))
    throw new InvalidOperationException("Estás usando la clave JWT de desarrollo fuera de desarrollo. Define la variable de entorno Jwt__Clave.");

builder.Services.Configure<JwtOpciones>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<IGeneradorToken, GeneradorTokenJwt>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        opciones.MapInboundClaims = false; // los claims conservan sus nombres: "slug", "rol"...
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Emisor,
            ValidateAudience = true,
            ValidAudience = jwt.Audiencia,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Clave)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });

// Autorización: "ser dueño de ESTA barbería"
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IAuthorizationHandler, DuenoDeLaBarberiaHandler>();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(PoliticasDeAcceso.DuenoDeLaBarberia, politica => politica
        .RequireAuthenticatedUser()
        .AddRequirements(new DuenoDeLaBarberiaRequirement()));

var app = builder.Build();

// En producción (Docker) la base se actualiza sola al arrancar: Base__MigrarAlIniciar=true
if (builder.Configuration.GetValue<bool>("Base:MigrarAlIniciar"))
{
    using var alcance = app.Services.CreateScope();
    alcance.ServiceProvider.GetRequiredService<AgendaDbContext>().Database.Migrate();
}

app.UseForwardedHeaders();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseCors("Web");     // antes de autenticar: responde las consultas previas ("preflight") del navegador
app.UseAuthentication(); // primero: ¿quién eres? (lee el token)
app.UseAuthorization();  // después: ¿puedes entrar aquí?
app.MapControllers();

app.Run();
