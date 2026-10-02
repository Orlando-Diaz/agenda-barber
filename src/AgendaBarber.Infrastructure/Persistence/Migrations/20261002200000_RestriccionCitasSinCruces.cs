using AgendaBarber.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgendaBarber.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Reglas que EF Core no sabe expresar con su API y que viven en la base de datos:
    /// 1) una cita no puede terminar antes de empezar;
    /// 2) un barbero no puede tener dos citas activas que se crucen en el tiempo.
    /// La segunda es la defensa real contra el "doble agendamiento": aunque dos clientes reserven
    /// al mismo tiempo y la aplicación no lo detecte, PostgreSQL rechaza el segundo INSERT.
    /// </summary>
    [DbContext(typeof(AgendaDbContext))]
    [Migration("20261002200000_RestriccionCitasSinCruces")]
    public partial class RestriccionCitasSinCruces : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // btree_gist permite combinar "=" (el barbero) con "&&" (rangos que se cruzan) en un mismo índice.
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");

            migrationBuilder.Sql("""
                ALTER TABLE citas
                ADD CONSTRAINT ck_citas_fin_despues_inicio CHECK ("FinUtc" > "InicioUtc");
                """);

            // '[)' = el inicio cuenta y el fin no: una cita 9:00–9:30 y otra 9:30–10:00 NO se cruzan.
            // Solo aplica a citas Pendiente o Confirmada: al cancelar una, su hueco queda libre.
            migrationBuilder.Sql("""
                ALTER TABLE citas
                ADD CONSTRAINT ex_citas_sin_cruces
                EXCLUDE USING gist (
                    "BarberoId" WITH =,
                    tstzrange("InicioUtc", "FinUtc", '[)') WITH &&
                )
                WHERE ("Estado" IN ('Pendiente', 'Confirmada'));
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE citas DROP CONSTRAINT IF EXISTS ex_citas_sin_cruces;");
            migrationBuilder.Sql("ALTER TABLE citas DROP CONSTRAINT IF EXISTS ck_citas_fin_despues_inicio;");
            // La extensión btree_gist se deja instalada: otras cosas podrían usarla.
        }
    }
}
