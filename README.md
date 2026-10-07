# AgendaBarber

Reservas en línea para barberías. Cada barbería tiene su página pública (`/b/{slug}`) donde los clientes reservan sin crear cuenta, y un panel privado donde el dueño ve su agenda y administra servicios, barberos y horarios.

**Stack:** .NET 10 (ASP.NET Core, EF Core) · PostgreSQL 16 · Angular 21 · JWT

## Arquitectura

```
src/
  AgendaBarber.Domain          Entidades y reglas de negocio (sin dependencias)
  AgendaBarber.Application     Casos de uso (una clase por acción) e interfaces de repositorios
  AgendaBarber.Infrastructure  EF Core, PostgreSQL, hash de contraseñas (PBKDF2)
  AgendaBarber.Api             Controladores, JWT, límite de intentos, manejo de errores
tests/AgendaBarber.Tests       xUnit: dominio, casos de uso y autenticación
web/                           Angular: página pública de reservas y panel del dueño
scripts/                       Pruebas manuales de la API en PowerShell
```

Decisiones que vale la pena conocer:

- **Sin doble reserva:** una restricción de exclusión en PostgreSQL impide que dos citas del mismo barbero se crucen, aunque dos clientes reserven al mismo milisegundo (responde 409).
- **Multi-barbería:** una política de autorización compara la barbería de la URL con la del token; un dueño nunca toca datos de otra barbería.
- **Quitar = desactivar:** servicios y barberos no se borran, se marcan inactivos; las citas pasadas conservan su información.
- **Horas:** se guardan en UTC y se muestran en `America/Bogota`.

## Correr en local

Necesitas .NET 10 SDK, Node 22+ y Docker.

```powershell
docker compose up -d                                   # PostgreSQL en el puerto 5434
dotnet ef database update --project src/AgendaBarber.Infrastructure --startup-project src/AgendaBarber.Api
dotnet run --project src/AgendaBarber.Api              # API en http://localhost:5068
```

En otra terminal:

```powershell
cd web
npm install
npm start                                              # Angular en http://localhost:4200
```

Tests: `dotnet test` (backend) y `cd web; npm test` (frontend).
Pruebas manuales de la API: `.\scripts\probar-*.ps1` (con la API corriendo).

## Producción

La API se empaqueta con el `Dockerfile` de la raíz:

```powershell
docker build -t agendabarber-api .
```

Variables de entorno obligatorias:

| Variable | Qué es |
|---|---|
| `ConnectionStrings__AgendaBarber` | Cadena de conexión a PostgreSQL |
| `Jwt__Clave` | Secreto para firmar tokens (32+ caracteres, aleatorio). La API se niega a arrancar con la clave de desarrollo |
| `Cors__Origenes__0` | URL del frontend, ej. `https://mi-barberia.vercel.app` |

Opcionales: `PORT` (por defecto 8080), `Base__MigrarAlIniciar` (ya viene en `true` en la imagen: aplica las migraciones al arrancar).

Al frontend se le indica la URL de la API en `web/src/app/core/api.config.ts` antes de compilar (`npm run build`); el resultado queda en `web/dist/web/browser` y se puede subir a cualquier hosting estático. Como es una SPA, el hosting debe devolver `index.html` para rutas desconocidas.

Detrás de un proxy (Render, nginx) la API lee la IP real del cliente de `X-Forwarded-For` para el límite de intentos del login (10 por minuto por IP).
