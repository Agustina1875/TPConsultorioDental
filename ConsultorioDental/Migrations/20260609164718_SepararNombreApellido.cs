using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsultorioDental.Migrations
{
    /// <inheritdoc />
    public partial class SepararNombreApellido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NombreCompleto",
                table: "Pacientes",
                newName: "Nombre");

            migrationBuilder.AddColumn<string>(
                name: "Apellido",
                table: "Pacientes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Apellido",
                table: "Pacientes");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "Pacientes",
                newName: "NombreCompleto");
        }
    }
}
