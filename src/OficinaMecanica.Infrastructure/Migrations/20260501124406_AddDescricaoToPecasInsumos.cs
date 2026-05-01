using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OficinaMecanica.Infrastructure.Migrations
{
    public partial class AddDescricaoToPecasInsumos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Adicionar coluna Descricao na tabela PecasInsumos
            migrationBuilder.Sql(@"
                ALTER TABLE ""PecasInsumos"" 
                ADD COLUMN IF NOT EXISTS ""Descricao"" VARCHAR(500);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remover coluna Descricao
            migrationBuilder.Sql(@"
                ALTER TABLE ""PecasInsumos"" 
                DROP COLUMN IF EXISTS ""Descricao"";
            ");
        }
    }
}