# Prueba los endpoints de barberos y horarios, uno tras otro.
# La API debe estar corriendo (dotnet run --project src/AgendaBarber.Api).

$base = "http://localhost:5068/api/barberias/el-patron/barberos"

function Enviar($metodo, $url, $cuerpo = $null) {
    $p = @{ Method = $metodo; Uri = $url; UseBasicParsing = $true }
    if ($cuerpo) {
        # Se envia en bytes UTF-8 para que las tildes lleguen bien
        $p.Body = [Text.Encoding]::UTF8.GetBytes(($cuerpo | ConvertTo-Json))
        $p.ContentType = "application/json; charset=utf-8"
    }
    try {
        $r = Invoke-WebRequest @p
        return @{ Codigo = [int]$r.StatusCode; Cuerpo = $r.Content }
    } catch {
        $resp = $_.Exception.Response
        if (-not $resp) { throw }
        $texto = (New-Object IO.StreamReader($resp.GetResponseStream())).ReadToEnd()
        return @{ Codigo = [int]$resp.StatusCode; Cuerpo = $texto }
    }
}

function Mostrar($titulo, $esperado, $r) {
    Write-Host ""
    Write-Host "== $titulo" -ForegroundColor Cyan
    $ok = ($r.Codigo -eq $esperado)
    $color = if ($ok) { "Green" } else { "Red" }
    Write-Host ("Codigo: {0} (esperado {1})" -f $r.Codigo, $esperado) -ForegroundColor $color
    Write-Host $r.Cuerpo
}

# 1. Crear barbero
$r = Enviar "Post" $base @{ nombre = "Carlos" }
Mostrar "1. Crear barbero" 201 $r
$id = ($r.Cuerpo | ConvertFrom-Json).id

# 2. Horario valido: lunes 09:00-12:00
$r = Enviar "Post" "$base/$id/horarios" @{ dia = 1; inicio = "09:00"; fin = "12:00" }
Mostrar "2. Horario lunes 09:00-12:00" 200 $r

# 3. Horario que se cruza con el anterior
$r = Enviar "Post" "$base/$id/horarios" @{ dia = 1; inicio = "11:00"; fin = "13:00" }
Mostrar "3. Horario que se cruza" 400 $r

# 4. Dia invalido
$r = Enviar "Post" "$base/$id/horarios" @{ dia = 9; inicio = "09:00"; fin = "12:00" }
Mostrar "4. Dia invalido (9)" 400 $r

# 5. Barbero que no existe
$falso = [guid]::NewGuid()
$r = Enviar "Post" "$base/$falso/horarios" @{ dia = 2; inicio = "09:00"; fin = "12:00" }
Mostrar "5. Barbero inexistente" 404 $r

# 6. Listar barberos con sus horarios
$r = Enviar "Get" $base
Mostrar "6. Listar barberos" 200 $r
