# Prueba el endpoint de disponibilidad. La API debe estar corriendo.
# Usa el primer barbero que tenga horarios y el primer servicio de El Patron.

$base = "http://localhost:5068/api/barberias/el-patron"

function Consultar($url) {
    try {
        $r = Invoke-WebRequest -Uri $url -UseBasicParsing
        return @{ Codigo = [int]$r.StatusCode; Cuerpo = $r.Content }
    } catch {
        $resp = $_.Exception.Response
        if (-not $resp) { throw }
        return @{ Codigo = [int]$resp.StatusCode; Cuerpo = (New-Object IO.StreamReader($resp.GetResponseStream())).ReadToEnd() }
    }
}

$barbero = (Invoke-RestMethod "$base/barberos") | Where-Object { $_.horarios.Count -gt 0 } | Select-Object -First 1
$servicio = (Invoke-RestMethod "$base/servicios") | Select-Object -First 1
if (-not $barbero -or -not $servicio) { Write-Host "Falta un barbero con horarios o un servicio. Corre antes probar-barberos.ps1 y crea un servicio." -ForegroundColor Red; return }

Write-Host "Barbero: $($barbero.nombre) | Servicio: $($servicio.nombre) ($($servicio.duracionMinutos) min)"

# Proximo lunes (el barbero de prueba trabaja los lunes 09:00-12:00)
$lunes = (1..7 | ForEach-Object { (Get-Date).AddDays($_) } | Where-Object { $_.DayOfWeek -eq "Monday" } | Select-Object -First 1).ToString("yyyy-MM-dd")
$martes = (Get-Date $lunes).AddDays(1).ToString("yyyy-MM-dd")
$ayer = (Get-Date).AddDays(-1).ToString("yyyy-MM-dd")
$q = "barberoId=$($barbero.id)&servicioId=$($servicio.id)"

$r = Consultar "$base/disponibilidad?$q&fecha=$lunes"
Write-Host "`n== 1. Proximo lunes ($lunes): espera 200 y horas 09:00 a 11:30" -ForegroundColor Cyan
Write-Host "Codigo: $($r.Codigo)"; (($r.Cuerpo | ConvertFrom-Json) | ForEach-Object { $_.hora }) -join "  "

$r = Consultar "$base/disponibilidad?$q&fecha=$martes"
Write-Host "`n== 2. Martes ($martes): espera 200 y lista vacia []" -ForegroundColor Cyan
Write-Host "Codigo: $($r.Codigo)"; $r.Cuerpo

$r = Consultar "$base/disponibilidad?$q&fecha=$ayer"
Write-Host "`n== 3. Dia pasado ($ayer): espera 200 y lista vacia []" -ForegroundColor Cyan
Write-Host "Codigo: $($r.Codigo)"; $r.Cuerpo

$r = Consultar "$base/disponibilidad?barberoId=$([guid]::NewGuid())&servicioId=$($servicio.id)&fecha=$lunes"
Write-Host "`n== 4. Barbero inexistente: espera 404" -ForegroundColor Cyan
Write-Host "Codigo: $($r.Codigo)"; $r.Cuerpo
