using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OficinaMecanica.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class M3_HistoricoEStatusRejeitada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Observacoes",
                table: "OrdensServico",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "HistoricoStatusOS",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrdemServicoId = table.Column<Guid>(type: "uuid", nullable: false),
                    StatusAnterior = table.Column<int>(type: "integer", nullable: true),
                    StatusNovo = table.Column<int>(type: "integer", nullable: false),
                    AlteradoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AlteradoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoStatusOS", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricoStatusOS_OrdensServico_OrdemServicoId",
                        column: x => x.OrdemServicoId,
                        principalTable: "OrdensServico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoStatusOS_OrdemServicoId_AlteradoEm",
                table: "HistoricoStatusOS",
                columns: new[] { "OrdemServicoId", "AlteradoEm" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoricoStatusOS");

            migrationBuilder.AlterColumn<string>(
                name: "Observacoes",
                table: "OrdensServico",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);
        }
    }
}
