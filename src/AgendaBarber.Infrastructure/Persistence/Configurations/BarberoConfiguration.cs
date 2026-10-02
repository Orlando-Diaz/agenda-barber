using AgendaBarber.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendaBarber.Infrastructure.Persistence.Configurations;

public class BarberoConfiguration : IEntityTypeConfiguration<Barbero>
{
    public void Configure(EntityTypeBuilder<Barbero> b)
    {
        b.ToTable("barberos");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();

        b.Property(x => x.Nombre).HasMaxLength(80).IsRequired();

        b.HasOne<Barberia>().WithMany().HasForeignKey(x => x.BarberiaId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.BarberiaId);

        // Los horarios son parte del barbero: si se borra el barbero, se borran sus horarios.
        b.HasMany(x => x.Horarios).WithOne().HasForeignKey(h => h.BarberoId).OnDelete(DeleteBehavior.Cascade);
    }
}
