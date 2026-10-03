using AgendaBarber.Domain.Entities;

namespace AgendaBarber.Application.Autenticacion;

public record TokenGenerado(string Token, DateTime ExpiraUtc);

public interface IGeneradorToken
{
    TokenGenerado Generar(Usuario usuario, Barberia barberia);
}
