# Prueba la agenda del dueno y los cambios de estado. La API debe estar corriendo.
# Necesita citas pendientes el proximo lunes: corre antes probar-reservas.ps1.
. "$PSScriptRoot\_comun.ps1"
if (-not (Preparar)) { return }

$lunes = ProximoLunes
$r = Llamar "Get" "$Base/agenda?fecha=$lunes" $null $Token
Mostrar "1. Agenda del lunes $lunes" 200 $r
$citas = @($r.Cuerpo | ConvertFrom-Json | ForEach-Object { $_ })
Write-Host "`nResumen:"
$citas | ForEach-Object { Write-Host ("  {0}-{1}  {2,-12} {3,-14} {4}" -f $_.horaInicio, $_.horaFin, $_.clienteNombre, $_.servicio, $_.estado) }

$cita = $citas | Where-Object { $_.estado -eq "Pendiente" } | Select-Object -First 1
if (-not $cita) { Write-Host "No hay citas pendientes. Corre antes probar-reservas.ps1" -ForegroundColor Red; return }
$url = "$Base/citas/$($cita.id)"
Write-Host "`nUsando la cita de las $($cita.horaInicio) ($($cita.clienteNombre))"

Mostrar "2. Confirmar SIN token" 401 (Llamar "Post" "$url/confirmar")
Mostrar "3. Confirmar la cita" 200 (Llamar "Post" "$url/confirmar" $null $Token)
Mostrar "4. Confirmar de nuevo" 400 (Llamar "Post" "$url/confirmar" $null $Token)
Mostrar "5. Marcar atendida (todavia no empieza)" 400 (Llamar "Post" "$url/atendida" $null $Token)
Mostrar "6. Cancelar la cita" 200 (Llamar "Post" "$url/cancelar" $null $Token)

$barbero = (Invoke-RestMethod "$Base/barberos") | ForEach-Object { $_ } | Where-Object { $_.horarios.Count -gt 0 } | Select-Object -First 1
$servicio = (Invoke-RestMethod "$Base/servicios") | ForEach-Object { $_ } | Select-Object -First 1
$libres = @(Invoke-RestMethod "$Base/disponibilidad?barberoId=$($barbero.id)&servicioId=$($servicio.id)&fecha=$lunes" | ForEach-Object { $_ })
Write-Host "`n== 7. Horas libres despues de cancelar: $(($libres | ForEach-Object { $_.hora }) -join ' ')" -ForegroundColor Cyan
if ($libres | Where-Object { $_.hora -eq $cita.horaInicio }) { Write-Host "Correcto: la hora $($cita.horaInicio) volvio a estar libre" -ForegroundColor Green }
else { Write-Host "ERROR: la hora $($cita.horaInicio) sigue ocupada" -ForegroundColor Red }

Mostrar "8. Cancelar de nuevo" 400 (Llamar "Post" "$url/cancelar" $null $Token)
Mostrar "9. Cita inexistente" 404 (Llamar "Post" "$Base/citas/$([guid]::NewGuid())/confirmar" $null $Token)
