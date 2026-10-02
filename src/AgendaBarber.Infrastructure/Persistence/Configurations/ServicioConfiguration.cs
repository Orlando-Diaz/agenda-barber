using AgendaBarber.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendaBarber.Infrastructure.Persistence.Configurations;

public class ServicioConfiguration : IEntityTypeConfiguration<Servicio>
{
    public void Configure(EntityTypeBuilder<Servicio> b)
    {
        b.ToTable("servicios");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();

        b.Property(x => x.Nombre).HasMaxLength(80).IsRequired();
        b.Property(x => x.Precio).HasPrecision(12, 2);

        b.HasOne<Barberia>().WithMany().HasForeignKey(x => x.BarberiaId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.BarberiaId);
    }
}
