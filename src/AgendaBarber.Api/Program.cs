using System.Text;
using AgendaBarber.Api.Seguridad;
using AgendaBarber.Application.Autenticacion;
using AgendaBarber.Application.Barberias;
using AgendaBarber.Application.Barberos;
using AgendaBarber.Application.Citas;
using AgendaBarber.Application.Disponibilidad;
using AgendaBarber.Application.Servicios;
using AgendaBarber.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton(TimeProvider.System);

// Casos de uso
builder.Services.AddScoped<ObtenerBarberiaPorSlug>();
builder.Services.AddScoped<CrearServicio>();
builder.Services.AddScoped<ListarServicios>();
builder.Services.AddScoped<CrearBarbero>();
builder.Services.AddScoped<AgregarHorarioBarbero>();
builder.Services.AddScoped<ListarBarberos>();
builder.Services.AddScoped<ConsultarDisponibilidad>();
builder.Services.AddScoped<ReservarCita>();
builder.Services.AddScoped<ConsultarAgenda>();
builder.Services.AddScoped<CambiarEstadoCita>();
builder.Services.AddScoped<RegistrarDueno>();
builder.Services.AddScoped<IniciarSesion>();

// Autenticación con JWT
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOpciones>()
    ?? throw new InvalidOperationException("Falta la sección 'Jwt' en la configuración.");
if (Encoding.UTF8.GetByteCount(jwt.Clave) < 32)
    throw new InvalidOperationException("Jwt:Clave debe tener al menos 32 caracteres.");

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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication(); // primero: ¿quién eres? (lee el token)
app.UseAuthorization();  // después: ¿puedes entrar aquí?
app.MapControllers();

app.Run();
