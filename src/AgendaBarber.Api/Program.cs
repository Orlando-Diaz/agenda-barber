using AgendaBarber.Infrastructure;
using AgendaBarber.Application.Barberias;
using AgendaBarber.Application.Servicios;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<CrearBarberia>();
builder.Services.AddScoped<ObtenerBarberiaPorSlug>();
builder.Services.AddScoped<CrearServicio>();
builder.Services.AddScoped<ListarServicios>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
