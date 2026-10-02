using AgendaBarber.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendaBarber.Infrastructure.Persistence.Configurations;

public class HorarioTrabajoConfiguration : IEntityTypeConfiguration<HorarioTrabajo>
{
    public void Configure(EntityTypeBuilder<HorarioTrabajo> b)
    {
        b.ToTable("horarios_trabajo");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.HasIndex(x => new { x.BarberoId, x.Dia });
    }
}
