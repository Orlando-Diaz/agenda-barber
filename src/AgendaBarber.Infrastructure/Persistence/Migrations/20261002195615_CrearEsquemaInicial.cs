using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgendaBarber.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CrearEsquemaInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "barberias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ZonaHoraria = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    CreadaEnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_barberias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "barberos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BarberiaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_barberos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_barberos_barberias_BarberiaId",
                        column: x => x.BarberiaId,
                        principalTable: "barberias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "servicios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BarberiaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    DuracionMinutos = table.Column<int>(type: "integer", nullable: false),
                    Precio = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_servicios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_servicios_barberias_BarberiaId",
                        column: x => x.BarberiaId,
                        principalTable: "barberias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "horarios_trabajo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BarberoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Dia = table.Column<int>(type: "integer", nullable: false),
                    Inicio = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Fin = table.Column<TimeOnly>(type: "time without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_horarios_trabajo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_horarios_trabajo_barberos_BarberoId",
                        column: x => x.BarberoId,
                        principalTable: "barberos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "citas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BarberiaId = table.Column<Guid>(type: "uuid", nullable: false),
                    BarberoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServicioId = table.Column<Guid>(type: "uuid", nullable: false),
                    InicioUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FinUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClienteNombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ClienteTelefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Precio = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreadaEnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_citas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_citas_barberias_BarberiaId",
                        column: x => x.BarberiaId,
                        principalTable: "barberias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_citas_barberos_BarberoId",
                        column: x => x.BarberoId,
                        principalTable: "barberos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_citas_servicios_ServicioId",
                        column: x => x.ServicioId,
                        principalTable: "servicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_barberias_Slug",
                table: "barberias",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_barberos_BarberiaId",
                table: "barberos",
                column: "BarberiaId");

            migrationBuilder.CreateIndex(
                name: "IX_citas_BarberiaId_InicioUtc",
                table: "citas",
                columns: new[] { "BarberiaId", "InicioUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_citas_BarberoId_InicioUtc",
                table: "citas",
                columns: new[] { "BarberoId", "InicioUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_citas_ServicioId",
                table: "citas",
                column: "ServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_horarios_trabajo_BarberoId_Dia",
                table: "horarios_trabajo",
                columns: new[] { "BarberoId", "Dia" });

            migrationBuilder.CreateIndex(
                name: "IX_servicios_BarberiaId",
                table: "servicios",
                column: "BarberiaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "citas");

            migrationBuilder.DropTable(
                name: "horarios_trabajo");

            migrationBuilder.DropTable(
                name: "servicios");

            migrationBuilder.DropTable(
                name: "barberos");

            migrationBuilder.DropTable(
                name: "barberias");
        }
    }
}
