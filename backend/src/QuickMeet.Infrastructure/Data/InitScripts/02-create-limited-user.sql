-- ============================================================================
-- Script: Crear usuario con mínimo privilegio para QuickMeet
-- ============================================================================
-- PROPÓSITO:
--   Crear un usuario SQL Server (quickmeet_app) con SOLO los permisos 
--   necesarios para que la aplicación funcione.
--
-- SEGURIDAD:
--   - Usuario NO puede crear bases de datos
--   - Usuario NO puede modificar usuarios/logins
--   - Usuario NO puede ver otros datos
--   - Solo LECTURA, ESCRITURA, y DDL en QuickMeet BD
--
-- EJECUCIÓN (Windows PowerShell):
--   sqlcmd -S . -E -i 02-create-limited-user.sql
--
-- EJECUCIÓN (En contenedor Docker):
--   docker exec quickmeet-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "PASSWORD" -i script.sql
-- ============================================================================

USE master;
GO

-- Verificar si el login ya existe
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'quickmeet_app')
BEGIN
    PRINT 'Creando login: quickmeet_app'
    CREATE LOGIN [quickmeet_app] WITH PASSWORD = 'QuickMeet$App.Secure2025!';
END
ELSE
BEGIN
    PRINT 'Login quickmeet_app ya existe'
END
GO

USE QuickMeet;
GO

-- Verificar si el usuario ya existe en la BD
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'quickmeet_app')
BEGIN
    PRINT 'Creando usuario de BD: quickmeet_app'
    CREATE USER [quickmeet_app] FOR LOGIN [quickmeet_app];
END
ELSE
BEGIN
    PRINT 'Usuario quickmeet_app ya existe en BD QuickMeet'
END
GO

-- ============================================================================
-- ASIGNAR ROLES (Mínimo Privilegio)
-- ============================================================================

-- db_datareader: Permitir SELECT en todas las tablas
ALTER ROLE [db_datareader] ADD MEMBER [quickmeet_app];

-- db_datawriter: Permitir INSERT, UPDATE, DELETE en todas las tablas
ALTER ROLE [db_datawriter] ADD MEMBER [quickmeet_app];

-- db_ddladmin: Necesario para Entity Framework Migrations
--   (Crear/modificar tablas, índices, restricciones)
ALTER ROLE [db_ddladmin] ADD MEMBER [quickmeet_app];

PRINT 'Roles asignados exitosamente'
GO

-- ============================================================================
-- PERMISOS EXPLÍCITOS (Específicos para la aplicación)
-- ============================================================================

-- Permitir EXECUTE (para stored procedures si existen)
GRANT EXECUTE ON DATABASE::[QuickMeet] TO [quickmeet_app];

-- Ver definición de objetos (necesario para EF Core)
GRANT VIEW DEFINITION ON DATABASE::[QuickMeet] TO [quickmeet_app];

PRINT 'Permisos explícitos asignados exitosamente'
GO

-- ============================================================================
-- VERIFICACIÓN
-- ============================================================================

PRINT '==================================================================='
PRINT 'RESUMEN DE PERMISOS PARA quickmeet_app:'
PRINT '==================================================================='

SELECT 
    'Rol' AS TipoPermiso,
    name AS Permiso
FROM sys.database_role_members rm
JOIN sys.database_principals rp ON rm.role_principal_id = rp.principal_id
JOIN sys.database_principals mp ON rm.member_principal_id = mp.principal_id
WHERE mp.name = 'quickmeet_app'
UNION ALL
SELECT 
    'Permiso Explícito',
    permission_name
FROM sys.database_permissions
WHERE grantee_principal_id = DATABASE_PRINCIPAL_ID('quickmeet_app')
ORDER BY TipoPermiso, Permiso;

PRINT '==================================================================='
PRINT 'Usuario creado/verificado exitosamente!'
PRINT '==================================================================='
