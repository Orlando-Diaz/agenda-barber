using AgendaBarber.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendaBarber.Infrastructure.Persistence.Configurations;

public class FotoBarberiaConfiguration : IEntityTypeConfiguration<FotoBarberia>
{
    public void Configure(EntityTypeBuilder<FotoBarberia> b)
    {
        b.ToTable("fotos_barberia");
        b.HasKey(x => x.BarberiaId);
        b.Property(x => x.BarberiaId).ValueGeneratedNever();
        b.Property(x => x.Datos).IsRequired();
        b.Property(x => x.Tipo).HasMaxLength(30).IsRequired();
        b.HasOne<Barberia>().WithOne().HasForeignKey<FotoBarberia>(x => x.BarberiaId).OnDelete(DeleteBehavior.Cascade);
    }
}
