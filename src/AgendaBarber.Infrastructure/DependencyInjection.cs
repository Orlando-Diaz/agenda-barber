using AgendaBarber.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgendaBarber.Infrastructure;

public static class DependencyInjection
{
    /// <summary>Registra todo lo de Infrastructure (por ahora, la base de datos).</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var conexion = configuration.GetConnectionString("AgendaBarber")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'AgendaBarber'.");

        services.AddDbContext<AgendaDbContext>(opciones => opciones.UseNpgsql(conexion));
        return services;
    }
}
