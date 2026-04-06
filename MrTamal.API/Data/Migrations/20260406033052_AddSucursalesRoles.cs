using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MrTamal.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSucursalesRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                table: "Usuarios",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                table: "Ingresos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SucursalId",
                table: "Egresos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Sucursales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Pais = table.Column<string>(type: "text", nullable: false, defaultValue: "US"),
                    Moneda = table.Column<string>(type: "text", nullable: false, defaultValue: "USD"),
                    SimboloMoneda = table.Column<string>(type: "text", nullable: false, defaultValue: "$"),
                    Direccion = table.Column<string>(type: "text", nullable: true),
                    Telefono = table.Column<string>(type: "text", nullable: true),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sucursales", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_SucursalId",
                table: "Usuarios",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_Ingresos_SucursalId",
                table: "Ingresos",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_Egresos_SucursalId",
                table: "Egresos",
                column: "SucursalId");

            migrationBuilder.AddForeignKey(
                name: "FK_Egresos_Sucursales_SucursalId",
                table: "Egresos",
                column: "SucursalId",
                principalTable: "Sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Ingresos_Sucursales_SucursalId",
                table: "Ingresos",
                column: "SucursalId",
                principalTable: "Sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Sucursales_SucursalId",
                table: "Usuarios",
                column: "SucursalId",
                principalTable: "Sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Egresos_Sucursales_SucursalId",
                table: "Egresos");

            migrationBuilder.DropForeignKey(
                name: "FK_Ingresos_Sucursales_SucursalId",
                table: "Ingresos");

            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Sucursales_SucursalId",
                table: "Usuarios");

            migrationBuilder.DropTable(
                name: "Sucursales");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_SucursalId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Ingresos_SucursalId",
                table: "Ingresos");

            migrationBuilder.DropIndex(
                name: "IX_Egresos_SucursalId",
                table: "Egresos");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "Ingresos");

            migrationBuilder.DropColumn(
                name: "SucursalId",
                table: "Egresos");
        }
    }
}
