using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OficinaMecanica.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTokenAprovacaoToOrdemServico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TokenAprovacao",
                table: "OrdensServico",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TokenUsado",
                table: "OrdensServico",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TokenAprovacao",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "TokenUsado",
                table: "OrdensServico");
        }
    }
}
