using AgendaBarber.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendaBarber.Infrastructure.Persistence.Configurations;

public class CitaConfiguration : IEntityTypeConfiguration<Cita>
{
    public void Configure(EntityTypeBuilder<Cita> b)
    {
        b.ToTable("citas");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();

        b.Property(x => x.ClienteNombre).HasMaxLength(100).IsRequired();
        b.Property(x => x.ClienteTelefono).HasMaxLength(20).IsRequired();
        b.Property(x => x.Precio).HasPrecision(12, 2);

        // El estado se guarda como texto ("Pendiente", "Confirmada"...): se lee mejor en la base de datos.
        b.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20).IsRequired();

        b.HasOne<Barberia>().WithMany().HasForeignKey(x => x.BarberiaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Barbero>().WithMany().HasForeignKey(x => x.BarberoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Servicio>().WithMany().HasForeignKey(x => x.ServicioId).OnDelete(DeleteBehavior.Restrict);

        // Para listar la agenda de un barbero o de una barbería por fecha.
        b.HasIndex(x => new { x.BarberoId, x.InicioUtc });
        b.HasIndex(x => new { x.BarberiaId, x.InicioUtc });
    }
}
