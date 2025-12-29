# 🔒 Estrategia de Seguridad: Mínimo Privilegio en BD

## 1. Arquitectura Actual

### Dev (Local)
```
┌─────────────────┐
│  Windows Auth   │  Integrated Security
│  (sa-like)      │  ✅ Máximo privilegio permitido en dev
└─────────────────┘
```

**Connection String:**
```
Server=.;Database=QuickMeet;Integrated Security=true;Encrypt=false;TrustServerCertificate=true;
```

**Ventaja:** Sin credenciales hardcodeadas, usa la identidad del usuario Windows.

---

### Producción & CI/CD (Planeado)
```
┌─────────────────────────────────────┐
│ Usuario: quickmeet_app              │  Mínimo Privilegio
│ Password: (Secret Vault)            │  ✅ Solo permisos necesarios
│ Roles: db_datareader, db_datawriter │
│         db_ddladmin (para EF Core)  │
└─────────────────────────────────────┘
```

**Connection String:**
```
Server=servidor;Database=QuickMeet;User Id=quickmeet_app;Password=***;Encrypt=true;TrustServerCertificate=false;
```

---

## 2. Proceso de Creación de Usuario

### Opción A: Script SQL (Recomendado)

**Archivo:** `InitScripts/02-create-limited-user.sql`

**Cómo ejecutar:**

#### En Windows (Local):
```powershell
sqlcmd -S . -E -i backend/src/QuickMeet.Infrastructure/Data/InitScripts/02-create-limited-user.sql
```

#### En Docker:
```bash
docker exec quickmeet-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "PASSWORD" -i /script/02-create-limited-user.sql
```

#### En GitHub Actions (CI/CD):
```yaml
- name: Create App User with Minimal Privileges
  run: |
    sqlcmd -S ${{ secrets.DB_SERVER }} \
           -U sa \
           -P ${{ secrets.DB_SA_PASSWORD }} \
           -i backend/src/QuickMeet.Infrastructure/Data/InitScripts/02-create-limited-user.sql
```

---

## 3. Secretos en CI/CD (GitHub Actions)

### Secrets Requeridos

En **Settings → Secrets and Variables → Repository Secrets**:

| Secret Name | Valor | Ejemplo |
|---|---|---|
| `DB_SERVER` | SQL Server host | `prod-sqlserver.database.windows.net` |
| `DB_SA_PASSWORD` | Password SA (solo para setup) | `ComplexPassword123!@#` |
| `DB_APP_PASSWORD` | Password quickmeet_app | `QuickMeet$App.Secure2025!` |
| `DB_APP_USER` | Username app | `quickmeet_app` |

### Workflow Ejemplo (GitHub Actions)

```yaml
name: Deploy to Production

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup DB User (si no existe)
        run: |
          sqlcmd -S ${{ secrets.DB_SERVER }} \
                 -U sa \
                 -P "${{ secrets.DB_SA_PASSWORD }}" \
                 -i backend/src/QuickMeet.Infrastructure/Data/InitScripts/02-create-limited-user.sql
      
      - name: Apply Migrations
        env:
          ConnectionStrings__DefaultConnection: "Server=${{ secrets.DB_SERVER }};Database=QuickMeet;User Id=${{ secrets.DB_APP_USER }};Password=${{ secrets.DB_APP_PASSWORD }};Encrypt=true;TrustServerCertificate=false;"
        run: |
          cd backend
          dotnet ef database update -s src/QuickMeet.API -p src/QuickMeet.Infrastructure
      
      - name: Deploy API
        run: |
          # Deploy logic here
```

---

## 4. Connection Strings por Ambiente

### appsettings.json (Production)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-sqlserver.database.windows.net;Database=QuickMeet;User Id=quickmeet_app;Password=***;Encrypt=true;TrustServerCertificate=false;"
  }
}
```

**Nota:** Password viene de variable de entorno o secret vault, NO hardcodeado.

### appsettings.Docker.json (Docker Local)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=sql-server,1433;Database=QuickMeet;User Id=quickmeet_app;Password=QuickMeet$App.Secure2025!;Encrypt=false;TrustServerCertificate=true;"
  }
}
```

---

## 5. Permisos del Usuario `quickmeet_app`

### ✅ Permitidos:
- `SELECT` en todas las tablas
- `INSERT, UPDATE, DELETE` en todas las tablas
- `CREATE TABLE, ALTER TABLE, DROP TABLE` (para EF Core migrations)
- `EXECUTE` (para stored procedures futuros)
- `VIEW DEFINITION` (para ver estructura)

### ❌ NO Permitidos:
- `CREATE DATABASE` (no puede crear BDs)
- `ALTER LOGIN` (no puede modificar usuarios)
- `VIEW SERVER STATE` (no puede ver otros datos del servidor)
- `GRANT/REVOKE` (no puede cambiar permisos)

---

## 6. Validación de Seguridad

### Comando para verificar permisos:
```sql
-- Ejecutar como SA
SELECT 
    'Rol' AS TipoPermiso,
    rp.name AS Permiso
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
```

---

## 7. Plan de Implementación

### Fase 1: Desarrollo (Actual)
- ✅ Integrated Security (Windows Auth)
- ✅ Sin cambios necesarios

### Fase 2: Docker Local
- [ ] Crear usuario `quickmeet_app` en contenedor
- [ ] Actualizar `appsettings.Docker.json`

### Fase 3: CI/CD (GitHub Actions)
- [ ] Configurar secrets en GitHub
- [ ] Script SQL ejecutado antes de migrations
- [ ] Connection string desde secrets

### Fase 4: Producción (Azure SQL / AWS RDS)
- [ ] Usar Azure Key Vault o AWS Secrets Manager
- [ ] Rotar passwords periódicamente
- [ ] Auditoría de acceso habilitada

---

## 8. Mejores Prácticas

| Ambiente | Auth | Password | Encrypt | TrustServerCertificate |
|----------|------|----------|---------|------------------------|
| **Dev** | Integrated | N/A | false | true |
| **Docker Local** | User/Pass | Secured | false | true |
| **CI/CD (Test)** | User/Pass | From Secret | true | false |
| **Production** | User/Pass | Key Vault | **true** | **false** |

---

## 9. Checklist de Seguridad

- [ ] No hardcodear contraseñas en Git
- [ ] Usar environment variables o secret vaults
- [ ] Encryption habilitada en Producción
- [ ] TrustServerCertificate=false en Producción
- [ ] Auditoría de BD habilitada
- [ ] Contraseñas complejas (min 12 chars, mayús, minús, números, símbolos)
- [ ] Rotar contraseñas cada 90 días
- [ ] Monitores de intentos de login fallidos
