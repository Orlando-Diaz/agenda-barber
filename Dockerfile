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
# Ajustes para que .NET arranque en entornos de contenedor muy limitados (plan gratuito de Render: 512 MB y poca CPU).
# Sin ellos la imagen se cerraba con "status 139" (fallo de memoria) antes de escribir un solo log.
#  - GCHeapHardLimit: limita el heap a ~320 MB en vez de reservar un espacio de memoria enorme
#  - EnableDiagnostics=0: no abre los canales de diagnóstico (no se usan en producción)
#  - EnableWriteXorExecute=0, TieredPGO=0, gcConcurrent=0, EnableAVX512F=0: evitan fallos conocidos del JIT y del GC
ENV ASPNETCORE_ENVIRONMENT=Production \
    Base__MigrarAlIniciar=true \
    DOTNET_GCHeapHardLimit=0x14000000 \
    DOTNET_EnableDiagnostics=0 \
    DOTNET_EnableWriteXorExecute=0 \
    DOTNET_TieredPGO=0 \
    DOTNET_gcConcurrent=0 \
    DOTNET_EnableAVX512F=0
# Render y similares indican el puerto en PORT; si no existe, usa 8080
CMD ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} exec dotnet AgendaBarber.Api.dll"]
