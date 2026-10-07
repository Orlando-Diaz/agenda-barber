using AgendaBarber.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendaBarber.Infrastructure.Persistence.Configurations;

public class BarberiaConfiguration : IEntityTypeConfiguration<Barberia>
{
    public void Configure(EntityTypeBuilder<Barberia> b)
    {
        b.ToTable("barberias");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever(); // el Id lo genera el dominio

        b.Property(x => x.Slug).HasMaxLength(50).IsRequired();
        b.HasIndex(x => x.Slug).IsUnique();

        b.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        b.Property(x => x.Telefono).HasMaxLength(30);
        b.Property(x => x.Direccion).HasMaxLength(150);
        b.Property(x => x.Descripcion).HasMaxLength(300);
        b.Property(x => x.ZonaHoraria).HasMaxLength(60).IsRequired();
    }
}
