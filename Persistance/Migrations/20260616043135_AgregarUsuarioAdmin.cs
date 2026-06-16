using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AgregarUsuarioAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Usuario",
                columns: new[] { "Id", "Apellido", "Email", "Estado", "Nombre", "NombreUsuario", "PasswordHash", "Rol" },
                values: new object[] { 1, "Sistemas", "admin@evote360.com", true, "Admin", "admin", "jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=", 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Usuario",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
