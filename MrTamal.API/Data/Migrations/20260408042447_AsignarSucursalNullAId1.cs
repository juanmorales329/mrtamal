using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MrTamal.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AsignarSucursalNullAId1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Asignar SucursalId = 1 a todos los ingresos y egresos que no tienen sucursal asignada
            migrationBuilder.Sql(
                "UPDATE \"Ingresos\" SET \"SucursalId\" = 1 WHERE \"SucursalId\" IS NULL;");
            migrationBuilder.Sql(
                "UPDATE \"Egresos\" SET \"SucursalId\" = 1 WHERE \"SucursalId\" IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revertir: quitar la asignación (no podemos saber cuáles eran null originalmente)
            // Se deja vacío intencionalmente
        }
    }
}
