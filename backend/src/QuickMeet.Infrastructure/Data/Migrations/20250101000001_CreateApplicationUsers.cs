using Microsoft.EntityFrameworkCore.Migrations;

namespace QuickMeet.Infrastructure.Data.Migrations
{
    public partial class CreateApplicationUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Crear usuario para la aplicación
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.sql_logins WHERE name = 'quickmeet_app')
                BEGIN
                    CREATE LOGIN [quickmeet_app] WITH PASSWORD = 'App123!@#Secure$Pass2025'
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'quickmeet_app')
                BEGIN
                    CREATE USER [quickmeet_app] FOR LOGIN [quickmeet_app]
                END
            ");

            // Crear usuario para migraciones
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.sql_logins WHERE name = 'quickmeet_migrations')
                BEGIN
                    CREATE LOGIN [quickmeet_migrations] WITH PASSWORD = 'Migrate123!@#Secure$Pass2025'
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'quickmeet_migrations')
                BEGIN
                    CREATE USER [quickmeet_migrations] FOR LOGIN [quickmeet_migrations]
                END
            ");

            // Crear usuario para lectura (auditoría/reportes)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.sql_logins WHERE name = 'quickmeet_readonly')
                BEGIN
                    CREATE LOGIN [quickmeet_readonly] WITH PASSWORD = 'ReadOnly123!@#Secure$Pass2025'
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'quickmeet_readonly')
                BEGIN
                    CREATE USER [quickmeet_readonly] FOR LOGIN [quickmeet_readonly]
                END
            ");

            // Asignar permisos - Aplicación (INSERT, UPDATE, DELETE, SELECT)
            migrationBuilder.Sql("ALTER ROLE [db_datawriter] ADD MEMBER [quickmeet_app]");
            migrationBuilder.Sql("ALTER ROLE [db_datareader] ADD MEMBER [quickmeet_app]");
            migrationBuilder.Sql("GRANT EXECUTE ON SCHEMA::[dbo] TO [quickmeet_app]");

            // Asignar permisos - Migraciones (DDL, ALTER, CREATE INDEX)
            migrationBuilder.Sql("ALTER ROLE [db_ddladmin] ADD MEMBER [quickmeet_migrations]");
            migrationBuilder.Sql("ALTER ROLE [db_datareader] ADD MEMBER [quickmeet_migrations]");
            migrationBuilder.Sql("GRANT ALTER ON SCHEMA::[dbo] TO [quickmeet_migrations]");

            // Asignar permisos - Lectura (SELECT solo)
            migrationBuilder.Sql("ALTER ROLE [db_datareader] ADD MEMBER [quickmeet_readonly]");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remover usuarios
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.database_principals WHERE name = 'quickmeet_app')
                BEGIN
                    DROP USER [quickmeet_app]
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.database_principals WHERE name = 'quickmeet_migrations')
                BEGIN
                    DROP USER [quickmeet_migrations]
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.database_principals WHERE name = 'quickmeet_readonly')
                BEGIN
                    DROP USER [quickmeet_readonly]
                END
            ");

            // Remover logins
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.sql_logins WHERE name = 'quickmeet_app')
                BEGIN
                    DROP LOGIN [quickmeet_app]
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.sql_logins WHERE name = 'quickmeet_migrations')
                BEGIN
                    DROP LOGIN [quickmeet_migrations]
                END
            ");

            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.sql_logins WHERE name = 'quickmeet_readonly')
                BEGIN
                    DROP LOGIN [quickmeet_readonly]
                END
            ");
        }
    }
}
