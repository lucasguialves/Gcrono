using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Cronometragem.Data.Migrations
{
    /// <inheritdoc />
    public partial class Modalidade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "modalidade_id",
                table: "inscricoes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "modalidades",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    prova_id = table.Column<int>(type: "integer", nullable: false),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_modalidades", x => x.id);
                    table.ForeignKey(
                        name: "FK_modalidades_provas_prova_id",
                        column: x => x.prova_id,
                        principalTable: "provas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_inscricoes_modalidade_id",
                table: "inscricoes",
                column: "modalidade_id");

            migrationBuilder.CreateIndex(
                name: "IX_modalidades_prova_id",
                table: "modalidades",
                column: "prova_id");

            migrationBuilder.AddForeignKey(
                name: "FK_inscricoes_modalidades_modalidade_id",
                table: "inscricoes",
                column: "modalidade_id",
                principalTable: "modalidades",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_inscricoes_modalidades_modalidade_id",
                table: "inscricoes");

            migrationBuilder.DropTable(
                name: "modalidades");

            migrationBuilder.DropIndex(
                name: "IX_inscricoes_modalidade_id",
                table: "inscricoes");

            migrationBuilder.DropColumn(
                name: "modalidade_id",
                table: "inscricoes");
        }
    }
}
