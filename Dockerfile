# Imagen de la API. Construir desde la raíz del repo:  docker build -t agendabarber-api .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Primero solo los .csproj: así Docker reutiliza la restauración de paquetes si no cambian
COPY src/AgendaBarber.Domain/*.csproj src/AgendaBarber.Domain/
COPY src/AgendaBarber.Application/*.csproj src/AgendaBarber.Application/
COPY src/AgendaBarber.Infrastructure/*.csproj src/AgendaBarber.Infrastructure/
COPY src/AgendaBarber.Api/*.csproj src/AgendaBarber.Api/
RUN dotnet restore src/AgendaBarber.Api/AgendaBarber.Api.csproj

COPY src/ src/
RUN dotnet publish src/AgendaBarber.Api/AgendaBarber.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
# DOTNET_EnableWriteXorExecute=0: .NET se cierra con "status 139" (fallo de memoria) al arrancar en algunos
# entornos de contenedor restringidos como el plan gratuito de Render; esta opción lo evita.
ENV ASPNETCORE_ENVIRONMENT=Production \
    Base__MigrarAlIniciar=true \
    DOTNET_EnableWriteXorExecute=0 \
    DOTNET_TieredPGO=0
# Render y similares indican el puerto en PORT; si no existe, usa 8080
CMD ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} exec dotnet AgendaBarber.Api.dll"]
