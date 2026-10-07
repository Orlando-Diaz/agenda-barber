using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgendaBarber.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPerfilBarberia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "barberias",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                table: "barberias",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FotoActualizadaUtc",
                table: "barberias",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "fotos_barberia",
                columns: table => new
                {
                    BarberiaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Datos = table.Column<byte[]>(type: "bytea", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fotos_barberia", x => x.BarberiaId);
                    table.ForeignKey(
                        name: "FK_fotos_barberia_barberias_BarberiaId",
                        column: x => x.BarberiaId,
                        principalTable: "barberias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fotos_barberia");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "barberias");

            migrationBuilder.DropColumn(
                name: "Direccion",
                table: "barberias");

            migrationBuilder.DropColumn(
                name: "FotoActualizadaUtc",
                table: "barberias");
        }
    }
}
