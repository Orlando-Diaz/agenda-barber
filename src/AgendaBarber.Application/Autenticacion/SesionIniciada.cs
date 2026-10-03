namespace AgendaBarber.Application.Autenticacion;

public record SesionIniciada(string Token, DateTime ExpiraUtc, string Slug, string NombreBarberia);
