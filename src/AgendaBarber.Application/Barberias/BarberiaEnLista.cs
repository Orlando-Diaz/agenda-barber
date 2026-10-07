namespace AgendaBarber.Application.Barberias;

/// <summary>Lo que se muestra de cada barbería en la lista de la portada.</summary>
public record BarberiaEnLista(
    string Nombre, string Slug, string? Telefono, string? Direccion, string? Descripcion,
    DateTime? FotoActualizadaUtc, decimal? PrecioDesde);
