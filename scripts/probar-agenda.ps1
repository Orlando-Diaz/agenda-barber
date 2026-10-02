# Prueba la agenda del dueno y los cambios de estado. La API debe estar corriendo.
# Necesita citas pendientes el proximo lunes: corre antes probar-reservas.ps1.

$base = "http://localhost:5068/api/barberias/el-patron"

function Llamar($metodo, $url) {
    try {
        $r = Invoke-WebRequest -Method $metodo -Uri $url -UseBasicParsing
        return @{ Codigo = [int]$r.StatusCode; Cuerpo = $r.Content }
    } catch {
        $resp = $_.Exception.Response
        if (-not $resp) { throw }
        return @{ Codigo = [int]$resp.StatusCode; Cuerpo = (New-Object IO.StreamReader($resp.GetResponseStream())).ReadToEnd() }
    }
}

function Mostrar($titulo, $esperado, $r) {
    Write-Host ""
    Write-Host "== $titulo" -ForegroundColor Cyan
    $color = if ($r.Codigo -eq $esperado) { "Green" } else { "Red" }
    Write-Host ("Codigo: {0} (esperado {1})" -f $r.Codigo, $esperado) -ForegroundColor $color
    Write-Host $r.Cuerpo
}

$lunes = (1..7 | ForEach-Object { (Get-Date).AddDays($_) } | Where-Object { $_.DayOfWeek -eq "Monday" } | Select-Object -First 1).ToString("yyyy-MM-dd")

# 1. Agenda del lunes
$r = Llamar "Get" "$base/agenda?fecha=$lunes"
Mostrar "1. Agenda del lunes $lunes" 200 $r
$citas = @($r.Cuerpo | ConvertFrom-Json | ForEach-Object { $_ })
Write-Host "`nResumen:"
$citas | ForEach-Object { Write-Host ("  {0}-{1}  {2,-12} {3,-14} {4}" -f $_.horaInicio, $_.horaFin, $_.clienteNombre, $_.servicio, $_.estado) }

$cita = $citas | Where-Object { $_.estado -eq "Pendiente" } | Select-Object -First 1
if (-not $cita) { Write-Host "No hay citas pendientes. Corre antes probar-reservas.ps1" -ForegroundColor Red; return }
$url = "$base/citas/$($cita.id)"
Write-Host "`nUsando la cita de las $($cita.horaInicio) ($($cita.clienteNombre))"

# 2. Confirmar
Mostrar "2. Confirmar la cita" 200 (Llamar "Post" "$url/confirmar")

# 3. Confirmar otra vez
Mostrar "3. Confirmar de nuevo (ya esta confirmada)" 400 (Llamar "Post" "$url/confirmar")

# 4. Marcar atendida antes de que empiece
Mostrar "4. Marcar atendida (todavia no empieza)" 400 (Llamar "Post" "$url/atendida")

# 5. Cancelar
Mostrar "5. Cancelar la cita" 200 (Llamar "Post" "$url/cancelar")

# 6. La hora debe quedar libre otra vez
$barbero = (Invoke-RestMethod "$base/barberos") | Where-Object { $_.horarios.Count -gt 0 } | Select-Object -First 1
$servicio = (Invoke-RestMethod "$base/servicios") | Select-Object -First 1
$libres = @(Invoke-RestMethod "$base/disponibilidad?barberoId=$($barbero.id)&servicioId=$($servicio.id)&fecha=$lunes" | ForEach-Object { $_ })
Write-Host "`n== 6. Horas libres despues de cancelar: $(($libres | ForEach-Object { $_.hora }) -join ' ')" -ForegroundColor Cyan
if ($libres | Where-Object { $_.hora -eq $cita.horaInicio }) { Write-Host "Correcto: la hora $($cita.horaInicio) volvio a estar libre" -ForegroundColor Green }
else { Write-Host "ERROR: la hora $($cita.horaInicio) sigue ocupada" -ForegroundColor Red }

# 7. Cancelar de nuevo
Mostrar "7. Cancelar de nuevo" 400 (Llamar "Post" "$url/cancelar")

# 8. Cita inexistente
Mostrar "8. Cita inexistente" 404 (Llamar "Post" "$base/citas/$([guid]::NewGuid())/confirmar")
