using AgendaBarber.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace AgendaBarber.Api.Seguridad;

public static class PoliticasDeAcceso
{
    public const string DuenoDeLaBarberia = "DuenoDeLaBarberia";
}

public class DuenoDeLaBarberiaRequirement : IAuthorizationRequirement { }

/// <summary>
/// Deja pasar solo si el token es de un dueño Y la barbería del token es la misma de la URL
/// (/api/barberias/{slug}/...). Así un dueño no puede tocar la barbería de otro.
/// </summary>
public class DuenoDeLaBarberiaHandler(IHttpContextAccessor accessor)
    : AuthorizationHandler<DuenoDeLaBarberiaRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, DuenoDeLaBarberiaRequirement requirement)
    {
        var slugRuta = accessor.HttpContext?.Request.RouteValues["slug"]?.ToString();
        var slugToken = context.User.FindFirst("slug")?.Value;
        var rol = context.User.FindFirst("rol")?.Value;

        if (slugRuta is not null
            && slugToken is not null
            && rol == nameof(RolUsuario.Dueno)
            && string.Equals(slugRuta, slugToken, StringComparison.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
