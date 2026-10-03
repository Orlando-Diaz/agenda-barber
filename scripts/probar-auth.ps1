# Prueba el registro, el login y la proteccion de los endpoints. La API debe estar corriendo.
. "$PSScriptRoot\_comun.ps1"
if (-not (Preparar)) { return }

# --- Registro y login ---
Mostrar "1. Registrar con un enlace que ya existe" 400 (Llamar "Post" "$Api/api/auth/registro" @{ nombreBarberia = "Otra"; slug = $Slug; telefono = $null; email = "otro@correo.test"; password = "ClaveSegura123" })
Mostrar "2. Registrar con un correo que ya existe" 400 (Llamar "Post" "$Api/api/auth/registro" @{ nombreBarberia = "Otra"; slug = "otra-barberia-x"; telefono = $null; email = $Email; password = "ClaveSegura123" })
Mostrar "3. Contrasena demasiado corta" 400 (Llamar "Post" "$Api/api/auth/registro" @{ nombreBarberia = "Otra"; slug = "otra-barberia-x"; telefono = $null; email = "nuevo@correo.test"; password = "123" })
Mostrar "4. Login con contrasena incorrecta" 401 (Llamar "Post" "$Api/api/auth/login" @{ email = $Email; password = "incorrecta999" })
Mostrar "5. Login con correo inexistente" 401 (Llamar "Post" "$Api/api/auth/login" @{ email = "nadie@correo.test"; password = "ClaveSegura123" })
$r = Llamar "Post" "$Api/api/auth/login" @{ email = $Email; password = $Clave }
Write-Host ""; Write-Host "== 6. Login correcto" -ForegroundColor Cyan
$color = if ($r.Codigo -eq 200) { "Green" } else { "Red" }
Write-Host ("Codigo: {0} (esperado 200)" -f $r.Codigo) -ForegroundColor $color
$sesion = $r.Cuerpo | ConvertFrom-Json
Write-Host "Token (primeros 40 caracteres): $($sesion.token.Substring(0, 40))..."
Write-Host "Barberia: $($sesion.slug)  Expira (UTC): $($sesion.expiraUtc)"

# --- Proteccion ---
$lunes = ProximoLunes
Mostrar "7. Agenda SIN token" 401 (Llamar "Get" "$Base/agenda?fecha=$lunes")
Mostrar "8. Agenda con token del dueno" 200 (Llamar "Get" "$Base/agenda?fecha=$lunes" $null $Token)
Mostrar "9. Crear servicio SIN token" 401 (Llamar "Post" "$Base/servicios" @{ nombre = "Intruso"; duracionMinutos = 30; precio = 1000 })
Mostrar "10. Token con basura" 401 (Llamar "Get" "$Base/agenda?fecha=$lunes" $null "esto.no.es-un-token")

# --- Otro dueno no puede tocar esta barberia ---
$otro = Llamar "Post" "$Api/api/auth/registro" @{ nombreBarberia = "Barberia Otra"; slug = "barberia-otra"; telefono = $null; email = "dueno@barberia-otra.test"; password = "ClaveSegura123" }
if ($otro.Codigo -ne 201) { $otro = Llamar "Post" "$Api/api/auth/login" @{ email = "dueno@barberia-otra.test"; password = "ClaveSegura123" } }
$tokenOtro = ($otro.Cuerpo | ConvertFrom-Json).token
Mostrar "11. Dueno de OTRA barberia intenta ver esta agenda" 403 (Llamar "Get" "$Base/agenda?fecha=$lunes" $null $tokenOtro)
Mostrar "12. Ese mismo dueno ve la agenda de SU barberia" 200 (Llamar "Get" "$Api/api/barberias/barberia-otra/agenda?fecha=$lunes" $null $tokenOtro)

# --- Lo publico sigue publico ---
Mostrar "13. Ver la barberia sin token" 200 (Llamar "Get" $Base)
Mostrar "14. Ver los servicios sin token" 200 (Llamar "Get" "$Base/servicios")
