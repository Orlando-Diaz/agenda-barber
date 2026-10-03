# Prueba los endpoints de barberos y horarios. La API debe estar corriendo.
. "$PSScriptRoot\_comun.ps1"
if (-not (Preparar)) { return }
$b = "$Base/barberos"

Mostrar "0. Crear barbero SIN token" 401 (Llamar "Post" $b @{ nombre = "Intruso" })

$r = Llamar "Post" $b @{ nombre = "Carlos" } $Token
Mostrar "1. Crear barbero" 201 $r
$id = ($r.Cuerpo | ConvertFrom-Json).id

Mostrar "2. Horario lunes 09:00-12:00" 200 (Llamar "Post" "$b/$id/horarios" @{ dia = 1; inicio = "09:00"; fin = "12:00" } $Token)
Mostrar "3. Horario que se cruza" 400 (Llamar "Post" "$b/$id/horarios" @{ dia = 1; inicio = "11:00"; fin = "13:00" } $Token)
Mostrar "4. Dia invalido (9)" 400 (Llamar "Post" "$b/$id/horarios" @{ dia = 9; inicio = "09:00"; fin = "12:00" } $Token)
Mostrar "5. Barbero inexistente" 404 (Llamar "Post" "$b/$([guid]::NewGuid())/horarios" @{ dia = 2; inicio = "09:00"; fin = "12:00" } $Token)
Mostrar "6. Listar barberos (publico)" 200 (Llamar "Get" $b)
