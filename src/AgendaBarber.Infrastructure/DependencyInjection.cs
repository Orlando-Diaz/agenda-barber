using AgendaBarber.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AgendaBarber.Application.Barberias;
using AgendaBarber.Application.Servicios;
using AgendaBarber.Application.Barberos;
using AgendaBarber.Application.Citas;
using AgendaBarber.Application.Autenticacion;
using AgendaBarber.Infrastructure.Seguridad;

namespace AgendaBarber.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registra todo lo de Infrastructure (por ahora, la base de datos).</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var conexion = configuration.GetConnectionString("AgendaBarber")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'AgendaBarber'.");

        services.AddDbContext<AgendaDbContext>(opciones => opciones.UseNpgsql(conexion));
        services.AddScoped<IBarberiaRepositorio, BarberiaRepositorio>();
        services.AddScoped<IFotoBarberiaRepositorio, FotoBarberiaRepositorio>();
        services.AddScoped<IServicioRepositorio, ServicioRepositorio>();
        services.AddScoped<IBarberoRepositorio, BarberoRepositorio>();
        services.AddScoped<ICitaRepositorio, CitaRepositorio>();
        services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
        services.AddSingleton<IHasheadorContrasenas, HasheadorContrasenas>();
        return services;
    }
}
