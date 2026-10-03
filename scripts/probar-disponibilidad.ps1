# Prueba el endpoint publico de disponibilidad. La API debe estar corriendo.
# Necesita un barbero con horarios: corre antes probar-barberos.ps1 (esta vez, sobre la barberia de prueba).
. "$PSScriptRoot\_comun.ps1"
if (-not (Preparar)) { return }

$barbero = (Invoke-RestMethod "$Base/barberos") | ForEach-Object { $_ } | Where-Object { $_.horarios.Count -gt 0 } | Select-Object -First 1
$servicio = (Invoke-RestMethod "$Base/servicios") | ForEach-Object { $_ } | Select-Object -First 1
if (-not $barbero) { Write-Host "No hay barberos con horarios. Corre antes probar-barberos.ps1" -ForegroundColor Red; return }

Write-Host "Barbero: $($barbero.nombre) | Servicio: $($servicio.nombre) ($($servicio.duracionMinutos) min)"
$lunes = ProximoLunes
$martes = (Get-Date $lunes).AddDays(1).ToString("yyyy-MM-dd")
$ayer = (Get-Date).AddDays(-1).ToString("yyyy-MM-dd")
$q = "barberoId=$($barbero.id)&servicioId=$($servicio.id)"

$r = Llamar "Get" "$Base/disponibilidad?$q&fecha=$lunes"
Write-Host "`n== 1. Proximo lunes ($lunes): espera 200 y horas 09:00 a 11:30 (menos las ya reservadas)" -ForegroundColor Cyan
Write-Host "Codigo: $($r.Codigo)"; (($r.Cuerpo | ConvertFrom-Json) | ForEach-Object { $_.hora }) -join "  "

Mostrar "2. Martes ($martes): lista vacia" 200 (Llamar "Get" "$Base/disponibilidad?$q&fecha=$martes")
Mostrar "3. Dia pasado ($ayer): lista vacia" 200 (Llamar "Get" "$Base/disponibilidad?$q&fecha=$ayer")
Mostrar "4. Barbero inexistente" 404 (Llamar "Get" "$Base/disponibilidad?barberoId=$([guid]::NewGuid())&servicioId=$($servicio.id)&fecha=$lunes")
