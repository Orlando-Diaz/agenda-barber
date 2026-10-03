using AgendaBarber.Application.Autenticacion;
using AgendaBarber.Domain;
using Microsoft.AspNetCore.Mvc;

namespace AgendaBarber.Api.Controllers;

public record RegistroRequest(string NombreBarberia, string Slug, string? Telefono, string Email, string Password);
public record LoginRequest(string Email, string Password);

[ApiController]
[Route("api/auth")]
public class AuthController(RegistrarDueno registrarDueno, IniciarSesion iniciarSesion) : ControllerBase
{
    [HttpPost("registro")]
    public async Task<IActionResult> Registro(RegistroRequest request, CancellationToken ct)
    {
        try
        {
            var sesion = await registrarDueno.EjecutarAsync(
                request.NombreBarberia, request.Slug, request.Telefono, request.Email, request.Password, ct);
            return Created($"/api/barberias/{sesion.Slug}", Respuesta(sesion));
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        var sesion = await iniciarSesion.EjecutarAsync(request.Email, request.Password, ct);
        if (sesion is null)
            return Unauthorized(new { error = "Correo o contraseña incorrectos." });

        return Ok(Respuesta(sesion));
    }

    private static object Respuesta(SesionIniciada s) => new { s.Token, s.ExpiraUtc, s.Slug, s.NombreBarberia };
}
