using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MrTamal.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProyectado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AsignacionesSucursal_Sucursales_SucursalId",
                table: "AsignacionesSucursal");

            migrationBuilder.CreateTable(
                name: "DiasNoLaborables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Descripcion = table.Column<string>(type: "text", nullable: true),
                    SucursalId = table.Column<int>(type: "integer", nullable: true),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiasNoLaborables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiasNoLaborables_Sucursales_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "Sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MetasAnuales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Anio = table.Column<int>(type: "integer", nullable: false),
                    MetaVentas = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ExcluirDomingos = table.Column<bool>(type: "boolean", nullable: false),
                    SucursalId = table.Column<int>(type: "integer", nullable: true),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetasAnuales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetasAnuales_Sucursales_SucursalId",
                        column: x => x.SucursalId,
                        principalTable: "Sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiasNoLaborables_SucursalId",
                table: "DiasNoLaborables",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_MetasAnuales_Anio_SucursalId",
                table: "MetasAnuales",
                columns: new[] { "Anio", "SucursalId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MetasAnuales_SucursalId",
                table: "MetasAnuales",
                column: "SucursalId");

            migrationBuilder.AddForeignKey(
                name: "FK_AsignacionesSucursal_Sucursales_SucursalId",
                table: "AsignacionesSucursal",
                column: "SucursalId",
                principalTable: "Sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AsignacionesSucursal_Sucursales_SucursalId",
                table: "AsignacionesSucursal");

            migrationBuilder.DropTable(
                name: "DiasNoLaborables");

            migrationBuilder.DropTable(
                name: "MetasAnuales");

            migrationBuilder.AddForeignKey(
                name: "FK_AsignacionesSucursal_Sucursales_SucursalId",
                table: "AsignacionesSucursal",
                column: "SucursalId",
                principalTable: "Sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
