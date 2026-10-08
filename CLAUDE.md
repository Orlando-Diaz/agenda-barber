# AgendaBarber

Reservas en línea para barberías: el cliente reserva sin cuenta y el dueño administra su agenda. Demo: https://agenda-barber-liard.vercel.app. Repo: Orlando-Diaz/agenda-barber. El detalle completo está en `README.md`.

## Cómo trabajar en este proyecto
- Aquí prefiero copiar y pegar el código yo mismo. Dame el código en bloques y explícame cada parte de lo que pegué: qué hace y por qué. Esto reemplaza la regla general de modificar los archivos directamente.
- Es mi proyecto para aprender .NET. Quiero entender la estructura y el porqué de cada parte, no solo que funcione. Compara con Java y Spring Boot cuando ayude.
- Dime cuándo conviene hacer un commit y con qué mensaje (Conventional Commits).
- Cada push a `main` se despliega solo en Render y Vercel. Avísame antes de un cambio que pueda romper producción.

## Estructura
- `src/AgendaBarber.Domain`: entidades y reglas de negocio, sin dependencias.
- `src/AgendaBarber.Application`: casos de uso (una clase por acción) e interfaces de repositorios.
- `src/AgendaBarber.Infrastructure`: EF Core, PostgreSQL y hash de contraseñas.
- `src/AgendaBarber.Api`: controladores, JWT, límite de intentos y manejo de errores.
- `tests/AgendaBarber.Tests`: xUnit.
- `web/`: Angular (página pública de reservas y panel del dueño).
- Las dependencias van hacia adentro: el dominio no conoce a EF Core ni a ASP.NET.

## Comandos
- Base de datos local: `docker compose up -d` (PostgreSQL en el puerto 5434).
- Migraciones: `dotnet ef database update --project src/AgendaBarber.Infrastructure --startup-project src/AgendaBarber.Api`
- API: `dotnet run --project src/AgendaBarber.Api` (http://localhost:5068).
- Web: `cd web`, `npm install`, `npm start` (http://localhost:4200).
- Pruebas: `dotnet test` y `cd web; npm test`.

## Reglas del proyecto
- La garantía de que no haya doble reserva vive en PostgreSQL (restricción de exclusión). No la quites ni la reemplaces por una validación solo en código.
- Un dueño nunca debe tocar datos de otra barbería: la política de autorización compara la barbería de la URL con la del token.
- Servicios y barberos no se borran, se marcan inactivos.
- Las horas se guardan en UTC y se muestran en America/Bogota. El reloj se inyecta con `TimeProvider`.
- Las credenciales no van en el repo. Se usan las variables de entorno `ConnectionStrings__AgendaBarber`, `Jwt__Clave` y `Cors__Origenes__0`.
- Si un cambio toca la base de datos, crea la migración antes (`dotnet ef migrations add ...`) y súbela en el mismo commit. La API la aplica al arrancar.
