namespace AgendaBarber.Domain.Entities;

/// <summary>Un tramo de trabajo de un barbero en un día de la semana, en hora local de la barbería.</summary>
public class HorarioTrabajo
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public Guid BarberoId { get; private set; }
    public DayOfWeek Dia { get; private set; }
    public TimeOnly Inicio { get; private set; }
    public TimeOnly Fin { get; private set; }

    private HorarioTrabajo() { }

    internal HorarioTrabajo(Guid barberoId, DayOfWeek dia, TimeOnly inicio, TimeOnly fin)
    {
        if (fin <= inicio)
            throw new DomainException("La hora de fin debe ser posterior a la de inicio.");

        BarberoId = barberoId;
        Dia = dia;
        Inicio = inicio;
        Fin = fin;
    }
}
