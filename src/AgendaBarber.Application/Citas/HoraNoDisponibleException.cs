namespace AgendaBarber.Application.Citas;

/// <summary>La hora elegida ya no se puede reservar (la tomó otro cliente o no está en el horario).</summary>
public class HoraNoDisponibleException(string mensaje = "Esa hora ya fue tomada. Elige otra.")
    : Exception(mensaje);
