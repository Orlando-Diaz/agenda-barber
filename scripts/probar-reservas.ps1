# Prueba la reserva de citas. La API debe estar corriendo.
# Cada vez que lo corras, reserva horas distintas (las anteriores quedan ocupadas).

$base = "http://localhost:5068/api/barberias/el-patron"

function Llamar($metodo, $url, $cuerpo = $null) {
    $p = @{ Method = $metodo; Uri = $url; UseBasicParsing = $true }
    if ($cuerpo) {
        $p.Body = [Text.Encoding]::UTF8.GetBytes(($cuerpo | ConvertTo-Json))
        $p.ContentType = "application/json; charset=utf-8"
    }
    try {
        $r = Invoke-WebRequest @p
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

$barbero = (Invoke-RestMethod "$base/barberos") | Where-Object { $_.horarios.Count -gt 0 } | Select-Object -First 1
$servicio = (Invoke-RestMethod "$base/servicios") | Select-Object -First 1
$lunes = (1..7 | ForEach-Object { (Get-Date).AddDays($_) } | Where-Object { $_.DayOfWeek -eq "Monday" } | Select-Object -First 1).ToString("yyyy-MM-dd")
$q = "barberoId=$($barbero.id)&servicioId=$($servicio.id)&fecha=$lunes"

# El "| ForEach-Object { $_ }" separa la lista en elementos (PowerShell 5.1 la entrega como un solo objeto)
function HorasLibres { Invoke-RestMethod "$base/disponibilidad?$q" | ForEach-Object { $_ } }

$libres = @(HorasLibres)
if ($libres.Count -lt 3) { Write-Host "Ya no quedan horas libres el $lunes. Crea otro horario o espera otra semana." -ForegroundColor Red; return }
Write-Host "Horas libres antes: $(($libres | ForEach-Object { $_.hora }) -join ' ')"

function Cuerpo($inicioUtc, $nombre = "Juan Perez", $tel = "3001234567") {
    @{ barberoId = $barbero.id; servicioId = $servicio.id; inicioUtc = $inicioUtc; clienteNombre = $nombre; clienteTelefono = $tel }
}

# 1. Reserva valida (primera hora libre)
$hora = $libres[0].inicioUtc
$r = Llamar "Post" "$base/citas" (Cuerpo $hora)
Mostrar "1. Reservar la hora $($libres[0].hora)" 201 $r

# 2. La misma hora otra vez
$r = Llamar "Post" "$base/citas" (Cuerpo $hora "Maria Lopez" "3109876543")
Mostrar "2. Reservar la MISMA hora" 409 $r

# 3. La hora reservada ya no aparece en la disponibilidad
$despues = @(HorasLibres)
Write-Host "`n== 3. Horas libres despues: $(($despues | ForEach-Object { $_.hora }) -join ' ')" -ForegroundColor Cyan
$sigue = $despues | Where-Object { $_.hora -eq $libres[0].hora }
if ($sigue) { Write-Host "ERROR: la hora reservada sigue apareciendo" -ForegroundColor Red } else { Write-Host "Correcto: la hora reservada desaparecio" -ForegroundColor Green }

# 4. Telefono invalido
$r = Llamar "Post" "$base/citas" (Cuerpo $despues[0].inicioUtc "Pedro Gomez" "123")
Mostrar "4. Telefono invalido" 400 $r

# 5. Hora fuera del horario del barbero (03:00 de la manana en Bogota = 08:00Z)
$r = Llamar "Post" "$base/citas" (Cuerpo "${lunes}T08:00:00Z")
Mostrar "5. Hora fuera de horario (3 a.m.)" 409 $r

# 6. Hora sin zona horaria
$r = Llamar "Post" "$base/citas" (Cuerpo "${lunes}T10:00:00")
Mostrar "6. Hora sin zona horaria" 400 $r

# 7. Dos reservas simultaneas a la misma hora: una debe ganar y la otra recibir 409
$objetivo = $despues[0].inicioUtc
$bytes = [Text.Encoding]::UTF8.GetBytes((Cuerpo $objetivo "Carrera Uno" "3001112222" | ConvertTo-Json))
$trabajos = 1..2 | ForEach-Object {
    Start-Job -ScriptBlock {
        param($url, $b)
        try { [int](Invoke-WebRequest -Uri $url -Method Post -Body $b -ContentType "application/json; charset=utf-8" -UseBasicParsing).StatusCode }
        catch { [int]$_.Exception.Response.StatusCode }
    } -ArgumentList "$base/citas", $bytes
}
$codigos = $trabajos | Wait-Job | Receive-Job
$trabajos | Remove-Job
Write-Host "`n== 7. Dos reservas simultaneas: codigos $($codigos -join ', ') (esperado: un 201 y un 409)" -ForegroundColor Cyan
