using AgendaBarber.Application.Barberias;
using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Application.Citas;

public enum AccionCita { Confirmar, Cancelar, Atendida, NoAsistio }

public class CambiarEstadoCita(IBarberiaRepositorio barberias, ICitaRepositorio citas, TimeProvider reloj)
{
    /// <summary>Aplica la acción a la cita. Devuelve null si la barbería o la cita no existen.</summary>
    public async Task<Cita?> EjecutarAsync(string slug, Guid citaId, AccionCita accion, CancellationToken ct = default)
    {
        var barberia = await barberias.ObtenerPorSlugAsync(slug.Trim().ToLowerInvariant(), ct);
        if (barberia is null) return null;

        var cita = await citas.ObtenerAsync(barberia.Id, citaId, ct);
        if (cita is null) return null;

        var ahoraUtc = reloj.GetUtcNow().UtcDateTime;
        switch (accion)
        {
            case AccionCita.Confirmar: cita.Confirmar(); break;
            case AccionCita.Cancelar: cita.Cancelar(); break;
            case AccionCita.Atendida: cita.MarcarAtendida(ahoraUtc); break;
            case AccionCita.NoAsistio: cita.MarcarNoAsistio(ahoraUtc); break;
        }

        await citas.GuardarCambiosAsync(ct); // las reglas de transición las valida el dominio
        return cita;
    }
}
