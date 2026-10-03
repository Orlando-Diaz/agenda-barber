namespace AgendaBarber.Application.Autenticacion;

public interface IHasheadorContrasenas
{
    string Hashear(string contrasena);
    bool Verificar(string hashGuardado, string contrasena);
}
