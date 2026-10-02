using AgendaBarber.Infrastructure;
using AgendaBarber.Application.Barberias;
using AgendaBarber.Application.Servicios;
using AgendaBarber.Application.Barberos;
using AgendaBarber.Application.Disponibilidad;
using AgendaBarber.Application.Citas;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<CrearBarberia>();
builder.Services.AddScoped<ObtenerBarberiaPorSlug>();
builder.Services.AddScoped<CrearServicio>();
builder.Services.AddScoped<ListarServicios>();
builder.Services.AddScoped<CrearBarbero>();
builder.Services.AddScoped<AgregarHorarioBarbero>();
builder.Services.AddScoped<ListarBarberos>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<ConsultarDisponibilidad>();
builder.Services.AddScoped<ReservarCita>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
