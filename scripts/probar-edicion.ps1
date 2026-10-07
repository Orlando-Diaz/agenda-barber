# Prueba editar y quitar servicios, barberos y horarios. Requiere la API corriendo (dotnet run --project src/AgendaBarber.Api).
. "$PSScriptRoot\_comun.ps1"
if (-not (Preparar)) { exit 1 }

# --- Servicios ---
$s = Llamar "Post" "$Base/servicios" @{ nombre = "Servicio temporal"; duracionMinutos = 30; precio = 15000 } $Token
Mostrar "Crear servicio temporal" 201 $s
$sid = ($s.Cuerpo | ConvertFrom-Json).id

Mostrar "Editar servicio (200)" 200 (Llamar "Put" "$Base/servicios/$sid" @{ nombre = "Servicio editado"; duracionMinutos = 45; precio = 18000 } $Token)
Mostrar "Editar con duracion invalida (400)" 400 (Llamar "Put" "$Base/servicios/$sid" @{ nombre = "X"; duracionMinutos = 1; precio = 1 } $Token)
Mostrar "Editar sin token (401)" 401 (Llamar "Put" "$Base/servicios/$sid" @{ nombre = "Hack"; duracionMinutos = 30; precio = 1 })
Mostrar "Editar servicio inexistente (404)" 404 (Llamar "Put" "$Base/servicios/$([guid]::NewGuid())" @{ nombre = "Nada"; duracionMinutos = 30; precio = 1 } $Token)
Mostrar "Quitar servicio (204)" 204 (Llamar "Delete" "$Base/servicios/$sid" $null $Token)
Mostrar "Quitar otra vez (404)" 404 (Llamar "Delete" "$Base/servicios/$sid" $null $Token)

# --- Barberos y horarios ---
$b = Llamar "Post" "$Base/barberos" @{ nombre = "Barbero temporal" } $Token
Mostrar "Crear barbero temporal" 201 $b
$bid = ($b.Cuerpo | ConvertFrom-Json).id

Mostrar "Renombrar barbero (200)" 200 (Llamar "Put" "$Base/barberos/$bid" @{ nombre = "Barbero renombrado" } $Token)
$h = Llamar "Post" "$Base/barberos/$bid/horarios" @{ dia = 1; inicio = "09:00"; fin = "12:00" } $Token
Mostrar "Agregar horario (200)" 200 $h
$hid = (($h.Cuerpo | ConvertFrom-Json).horarios | Select-Object -First 1).id
Mostrar "Quitar horario (200, sin horarios)" 200 (Llamar "Delete" "$Base/barberos/$bid/horarios/$hid" $null $Token)
Mostrar "Quitar horario otra vez (404)" 404 (Llamar "Delete" "$Base/barberos/$bid/horarios/$hid" $null $Token)
Mostrar "Quitar barbero (204)" 204 (Llamar "Delete" "$Base/barberos/$bid" $null $Token)
Mostrar "Quitar barbero otra vez (404)" 404 (Llamar "Delete" "$Base/barberos/$bid" $null $Token)
