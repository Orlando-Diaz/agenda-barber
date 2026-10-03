using System.Security.Cryptography;
using AgendaBarber.Application.Autenticacion;

namespace AgendaBarber.Infrastructure.Seguridad;

/// <summary>
/// Guarda las contraseñas con PBKDF2: una "sal" aleatoria distinta por usuario y muchas vueltas de
/// cálculo, para que adivinar contraseñas a la fuerza sea carísimo. Formato: v1.vueltas.sal.hash
/// </summary>
public sealed class HasheadorContrasenas : IHasheadorContrasenas
{
    private const int TamanoSal = 16;
    private const int TamanoHash = 32;
    private const int Vueltas = 600_000;

    public string Hashear(string contrasena)
    {
        var sal = RandomNumberGenerator.GetBytes(TamanoSal);
        var hash = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, Vueltas, HashAlgorithmName.SHA256, TamanoHash);
        return $"v1.{Vueltas}.{Convert.ToBase64String(sal)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verificar(string hashGuardado, string contrasena)
    {
        var partes = hashGuardado.Split('.');
        if (partes.Length != 4 || partes[0] != "v1" || !int.TryParse(partes[1], out var vueltas) || vueltas is < 1 or > 5_000_000)
            return false;

        try
        {
            var sal = Convert.FromBase64String(partes[2]);
            var esperado = Convert.FromBase64String(partes[3]);
            var calculado = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, vueltas, HashAlgorithmName.SHA256, esperado.Length);
            return CryptographicOperations.FixedTimeEquals(calculado, esperado); // compara en tiempo constante
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
