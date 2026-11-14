using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ApiUsuario.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "departamentos",
                columns: table => new
                {
                    id_departamento = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre_departamento = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_departamentos", x => x.id_departamento);
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    celular = table.Column<string>(type: "text", nullable: true),
                    fecha_nacimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    genero = table.Column<char>(type: "character(1)", nullable: false),
                    estado = table.Column<short>(type: "smallint", nullable: false),
                    fecha_alta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario", x => x.id_usuario);
                });

            migrationBuilder.CreateTable(
                name: "ciudades",
                columns: table => new
                {
                    id_ciudad = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre_ciudad = table.Column<string>(type: "text", nullable: false),
                    id_departamento = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ciudades", x => x.id_ciudad);
                    table.ForeignKey(
                        name: "FK_ciudades_departamentos_id_departamento",
                        column: x => x.id_departamento,
                        principalTable: "departamentos",
                        principalColumn: "id_departamento",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuario_detalle",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    id_departamento = table.Column<int>(type: "integer", nullable: true),
                    id_ciudad = table.Column<int>(type: "integer", nullable: true),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    fecha_ultima_actualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_detalle", x => x.id_usuario);
                    table.ForeignKey(
                        name: "FK_usuario_detalle_ciudades_id_ciudad",
                        column: x => x.id_ciudad,
                        principalTable: "ciudades",
                        principalColumn: "id_ciudad",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_usuario_detalle_departamentos_id_departamento",
                        column: x => x.id_departamento,
                        principalTable: "departamentos",
                        principalColumn: "id_departamento",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_usuario_detalle_usuario_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ciudades_id_departamento",
                table: "ciudades",
                column: "id_departamento");

            migrationBuilder.CreateIndex(
                name: "ux_usuario_email",
                table: "usuario",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuario_detalle_id_ciudad",
                table: "usuario_detalle",
                column: "id_ciudad");

            migrationBuilder.CreateIndex(
                name: "ix_usuario_detalle_id_departamento",
                table: "usuario_detalle",
                column: "id_departamento");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "usuario_detalle");

            migrationBuilder.DropTable(
                name: "ciudades");

            migrationBuilder.DropTable(
                name: "usuario");

            migrationBuilder.DropTable(
                name: "departamentos");
        }
    }
}
