namespace AgendaBarber.Api.Seguridad;

/// <summary>Se llena desde la sección "Jwt" de appsettings (o de variables de entorno en producción).</summary>
public class JwtOpciones
{
    /// <summary>El secreto con el que se firman los tokens. Mínimo 32 caracteres. Nunca va en el repositorio de producción.</summary>
    public string Clave { get; set; } = "";
    public string Emisor { get; set; } = "AgendaBarber";
    public string Audiencia { get; set; } = "AgendaBarber";
    public int HorasDeVida { get; set; } = 12;
}
