# Prueba la reserva de citas (publica). La API debe estar corriendo.
# Necesita un barbero con horarios: corre antes probar-barberos.ps1.
# Cada vez que lo corras, reserva horas distintas (las anteriores quedan ocupadas).
. "$PSScriptRoot\_comun.ps1"
if (-not (Preparar)) { return }

$barbero = (Invoke-RestMethod "$Base/barberos") | ForEach-Object { $_ } | Where-Object { $_.horarios.Count -gt 0 } | Select-Object -First 1
$servicio = (Invoke-RestMethod "$Base/servicios") | ForEach-Object { $_ } | Select-Object -First 1
if (-not $barbero) { Write-Host "No hay barberos con horarios. Corre antes probar-barberos.ps1" -ForegroundColor Red; return }

$lunes = ProximoLunes
$q = "barberoId=$($barbero.id)&servicioId=$($servicio.id)&fecha=$lunes"

# El "| ForEach-Object { $_ }" separa la lista en elementos (PowerShell 5.1 la entrega como un solo objeto)
function HorasLibres { Invoke-RestMethod "$Base/disponibilidad?$q" | ForEach-Object { $_ } }

$libres = @(HorasLibres)
if ($libres.Count -lt 3) { Write-Host "Ya no quedan horas libres el $lunes. Cancela citas o espera otra semana." -ForegroundColor Red; return }
Write-Host "Horas libres antes: $(($libres | ForEach-Object { $_.hora }) -join ' ')"

function Cuerpo($inicioUtc, $nombre = "Juan Perez", $tel = "3001234567") {
    @{ barberoId = $barbero.id; servicioId = $servicio.id; inicioUtc = $inicioUtc; clienteNombre = $nombre; clienteTelefono = $tel }
}

$hora = $libres[0].inicioUtc
Mostrar "1. Reservar la hora $($libres[0].hora) (sin token: es publico)" 201 (Llamar "Post" "$Base/citas" (Cuerpo $hora))
Mostrar "2. Reservar la MISMA hora" 409 (Llamar "Post" "$Base/citas" (Cuerpo $hora "Maria Lopez" "3109876543"))

$despues = @(HorasLibres)
Write-Host "`n== 3. Horas libres despues: $(($despues | ForEach-Object { $_.hora }) -join ' ')" -ForegroundColor Cyan
if ($despues | Where-Object { $_.hora -eq $libres[0].hora }) { Write-Host "ERROR: la hora reservada sigue apareciendo" -ForegroundColor Red }
else { Write-Host "Correcto: la hora reservada desaparecio" -ForegroundColor Green }

Mostrar "4. Telefono invalido" 400 (Llamar "Post" "$Base/citas" (Cuerpo $despues[0].inicioUtc "Pedro Gomez" "123"))
Mostrar "5. Hora fuera de horario (3 a.m.)" 409 (Llamar "Post" "$Base/citas" (Cuerpo "${lunes}T08:00:00Z"))
Mostrar "6. Hora sin zona horaria" 400 (Llamar "Post" "$Base/citas" (Cuerpo "${lunes}T10:00:00"))

$objetivo = $despues[0].inicioUtc
$bytes = [Text.Encoding]::UTF8.GetBytes((Cuerpo $objetivo "Carrera Uno" "3001112222" | ConvertTo-Json))
$trabajos = 1..2 | ForEach-Object {
    Start-Job -ScriptBlock {
        param($url, $b)
        try { [int](Invoke-WebRequest -Uri $url -Method Post -Body $b -ContentType "application/json; charset=utf-8" -UseBasicParsing).StatusCode }
        catch { [int]$_.Exception.Response.StatusCode }
    } -ArgumentList "$Base/citas", $bytes
}
$codigos = $trabajos | Wait-Job | Receive-Job
$trabajos | Remove-Job
Write-Host "`n== 7. Dos reservas simultaneas: codigos $($codigos -join ', ') (esperado: un 201 y un 409)" -ForegroundColor Cyan
