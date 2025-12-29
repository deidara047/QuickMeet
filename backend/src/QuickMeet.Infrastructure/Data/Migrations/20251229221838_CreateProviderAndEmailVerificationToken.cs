using Microsoft.EntityFrameworkCore.Migrations;

namespace QuickMeet.Infrastructure.Data.Migrations
{
    public partial class CreateProviderAndEmailVerificationToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ============================================================================
            // Crear tabla Providers (usuarios de la aplicación)
            // ============================================================================
            migrationBuilder.CreateTable(
                name: "Providers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(256)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(2048)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    AppointmentDurationMinutes = table.Column<int>(type: "int", nullable: false, defaultValue: 30),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmailVerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Providers", x => x.Id);
                });

            // ============================================================================
            // Crear índices para Providers
            // ============================================================================
            migrationBuilder.CreateIndex(
                name: "IX_Provider_Email_Unique",
                table: "Providers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Provider_Username_Unique",
                table: "Providers",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Provider_Status",
                table: "Providers",
                column: "Status");

            // ============================================================================
            // Crear tabla EmailVerificationTokens
            // ============================================================================
            migrationBuilder.CreateTable(
                name: "EmailVerificationTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailVerificationTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailVerificationTokens_Providers_ProviderId",
                        column: x => x.ProviderId,
                        principalTable: "Providers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // ============================================================================
            // Crear índices para EmailVerificationTokens
            // ============================================================================
            migrationBuilder.CreateIndex(
                name: "IX_EmailVerificationToken_Token_Unique",
                table: "EmailVerificationTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailVerificationToken_ProviderId_IsUsed",
                table: "EmailVerificationTokens",
                columns: new[] { "ProviderId", "IsUsed" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Eliminar tabla de tokens primero (FK constraint)
            migrationBuilder.DropTable(
                name: "EmailVerificationTokens");

            // Luego eliminar tabla de proveedores
            migrationBuilder.DropTable(
                name: "Providers");
        }
    }
}
