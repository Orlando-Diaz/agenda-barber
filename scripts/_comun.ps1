# Funciones compartidas por los scripts de prueba. No se ejecuta solo: los otros scripts lo cargan.
# Usa una barberia de prueba ("barberia-demo") que se registra sola la primera vez.

$Api = "http://localhost:5068"
$Slug = "barberia-demo"
$Base = "$Api/api/barberias/$Slug"
$Email = "dueno@barberia-demo.test"
$Clave = "ClaveSegura123"
$global:Token = $null

# Envia una peticion y devuelve el codigo y el cuerpo (tambien cuando es un error 4xx).
function Llamar($metodo, $url, $cuerpo = $null, $token = $null) {
    $p = @{ Method = $metodo; Uri = $url; UseBasicParsing = $true }
    if ($token) { $p.Headers = @{ Authorization = "Bearer $token" } }
    if ($cuerpo) {
        # Bytes UTF-8 para que las tildes lleguen bien
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

# Entra con la barberia de prueba (la registra si no existe) y se asegura de que tenga un servicio.
function Preparar {
    $login = Llamar "Post" "$Api/api/auth/login" @{ email = $Email; password = $Clave }
    if ($login.Codigo -ne 200) {
        $reg = Llamar "Post" "$Api/api/auth/registro" @{ nombreBarberia = "Barberia Demo"; slug = $Slug; telefono = "3001234567"; email = $Email; password = $Clave }
        if ($reg.Codigo -ne 201) {
            Write-Host "No se pudo crear/entrar a la barberia de prueba: $($reg.Cuerpo)" -ForegroundColor Red
            return $false
        }
        $login = $reg
    }
    $global:Token = ($login.Cuerpo | ConvertFrom-Json).token

    $servicios = @(Invoke-RestMethod "$Base/servicios" | ForEach-Object { $_ })
    if ($servicios.Count -eq 0) {
        $null = Llamar "Post" "$Base/servicios" @{ nombre = "Corte clasico"; duracionMinutos = 30; precio = 20000 } $Token
    }
    return $true
}

function ProximoLunes {
    (1..7 | ForEach-Object { (Get-Date).AddDays($_) } | Where-Object { $_.DayOfWeek -eq "Monday" } | Select-Object -First 1).ToString("yyyy-MM-dd")
}
