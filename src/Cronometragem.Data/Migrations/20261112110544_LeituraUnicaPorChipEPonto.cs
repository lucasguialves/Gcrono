using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cronometragem.Data.Migrations
{
    /// <inheritdoc />
    public partial class LeituraUnicaPorChipEPonto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove duplicatas que possam existir de antes dessa restrição (ex.: leituras
            // manuais lançadas duas vezes pro mesmo chip/ponto de controle), mantendo só a
            // leitura mais antiga de cada grupo — senão a criação do índice único abaixo falha.
            migrationBuilder.Sql(@"
                DELETE FROM leituras l
                USING leituras l2
                WHERE l.prova_id = l2.prova_id
                  AND l.chip_id = l2.chip_id
                  AND l.ponto_controle = l2.ponto_controle
                  AND (l.dta_hora_leitura, l.id) > (l2.dta_hora_leitura, l2.id);
            ");

            migrationBuilder.DropIndex(
                name: "IX_leituras_prova_id_chip_id_ponto_controle",
                table: "leituras");

            migrationBuilder.CreateIndex(
                name: "IX_leituras_prova_id_chip_id_ponto_controle",
                table: "leituras",
                columns: new[] { "prova_id", "chip_id", "ponto_controle" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_leituras_prova_id_chip_id_ponto_controle",
                table: "leituras");

            migrationBuilder.CreateIndex(
                name: "IX_leituras_prova_id_chip_id_ponto_controle",
                table: "leituras",
                columns: new[] { "prova_id", "chip_id", "ponto_controle" });
        }
    }
}
