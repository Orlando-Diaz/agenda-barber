using AgendaBarber.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgendaBarber.Infrastructure.Persistence;

public class AgendaDbContext(DbContextOptions<AgendaDbContext> options) : DbContext(options)
{
    public DbSet<Barberia> Barberias => Set<Barberia>();
    public DbSet<Servicio> Servicios => Set<Servicio>();
    public DbSet<Barbero> Barberos => Set<Barbero>();
    public DbSet<HorarioTrabajo> HorariosTrabajo => Set<HorarioTrabajo>();
    public DbSet<Cita> Citas => Set<Cita>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<FotoBarberia> FotosBarberia => Set<FotoBarberia>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Encuentra todas las clases IEntityTypeConfiguration<T> de este proyecto.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AgendaDbContext).Assembly);
    }
}
