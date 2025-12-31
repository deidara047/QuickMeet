# Flujo 1: Registro Exitoso

Navegar a /auth/register
Llenar: email, username, fullName, password, passwordConfirmation
Enviar formulario
Verificar mensaje de éxito
Verificar redirección a login

# Flujo 2: Login Exitoso

Navegar a /auth/login
Llenar: email, password
Enviar formulario
Verificar redirección a /dashboard
Verificar que localStorage tiene access_token

# Flujo 3: Validaciones del Formulario

Email inválido → mostrar error
Username < 3 caracteres → mostrar error
Password sin mayúscula/número/especial → mostrar error
Contraseñas no coinciden → mostrar error

# Flujo 4: Registro Fallido (email ya existe)

Intentar registrar con email existente
Verificar mensaje de error del backend
Mantenerse en página de registro

# Flujo 5: Guards funcionando

Visitante intenta acceder a /dashboard → redirecciona a /login
Autenticado intenta acceder a /auth/login → redirecciona a /dashboard