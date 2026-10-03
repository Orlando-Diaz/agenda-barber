using System.Security.Claims;
using System.Text;
using AgendaBarber.Application.Autenticacion;
using AgendaBarber.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AgendaBarber.Api.Seguridad;

/// <summary>Fabrica el token JWT: un texto firmado que dice quién eres y de qué barbería eres dueño.</summary>
public class GeneradorTokenJwt(IOptions<JwtOpciones> opciones, TimeProvider reloj) : IGeneradorToken
{
    public TokenGenerado Generar(Usuario usuario, Barberia barberia)
    {
        var o = opciones.Value;
        var ahora = reloj.GetUtcNow().UtcDateTime;
        var expira = ahora.AddHours(o.HorasDeVida);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = o.Emisor,
            Audience = o.Audiencia,
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("sub", usuario.Id.ToString()),
                new Claim("slug", barberia.Slug),
                new Claim("rol", usuario.Rol.ToString()),
            }),
            NotBefore = ahora,
            IssuedAt = ahora,
            Expires = expira,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(o.Clave)), SecurityAlgorithms.HmacSha256),
        };

        return new TokenGenerado(new JsonWebTokenHandler().CreateToken(descriptor), expira);
    }
}
