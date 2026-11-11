using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Cronometragem.Data.Migrations
{
    /// <inheritdoc />
    public partial class InicialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "atletas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    documento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    data_nascimento = table.Column<DateOnly>(type: "date", nullable: true),
                    sexo = table.Column<char>(type: "character(1)", maxLength: 1, nullable: true),
                    telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_atletas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "provas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    data_prova = table.Column<DateOnly>(type: "date", nullable: false),
                    hora_largada_oficial = table.Column<TimeOnly>(type: "time without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "categorias",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    prova_id = table.Column<int>(type: "integer", nullable: false),
                    nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    idade_min = table.Column<int>(type: "integer", nullable: true),
                    idade_max = table.Column<int>(type: "integer", nullable: true),
                    sexo = table.Column<char>(type: "character(1)", maxLength: 1, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias", x => x.id);
                    table.ForeignKey(
                        name: "FK_categorias_provas_prova_id",
                        column: x => x.prova_id,
                        principalTable: "provas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "leituras",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    prova_id = table.Column<int>(type: "integer", nullable: false),
                    chip_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ponto_controle = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    dta_hora_leitura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    origem_leitor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leituras", x => x.id);
                    table.ForeignKey(
                        name: "FK_leituras_provas_prova_id",
                        column: x => x.prova_id,
                        principalTable: "provas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "inscricoes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    prova_id = table.Column<int>(type: "integer", nullable: false),
                    atleta_id = table.Column<int>(type: "integer", nullable: false),
                    numero_peito = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    categoria_id = table.Column<int>(type: "integer", nullable: true),
                    chip_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inscricoes", x => x.id);
                    table.ForeignKey(
                        name: "FK_inscricoes_atletas_atleta_id",
                        column: x => x.atleta_id,
                        principalTable: "atletas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_inscricoes_categorias_categoria_id",
                        column: x => x.categoria_id,
                        principalTable: "categorias",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_inscricoes_provas_prova_id",
                        column: x => x.prova_id,
                        principalTable: "provas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_categorias_prova_id",
                table: "categorias",
                column: "prova_id");

            migrationBuilder.CreateIndex(
                name: "IX_inscricoes_atleta_id",
                table: "inscricoes",
                column: "atleta_id");

            migrationBuilder.CreateIndex(
                name: "IX_inscricoes_categoria_id",
                table: "inscricoes",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "IX_inscricoes_prova_id_chip_id",
                table: "inscricoes",
                columns: new[] { "prova_id", "chip_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inscricoes_prova_id_numero_peito",
                table: "inscricoes",
                columns: new[] { "prova_id", "numero_peito" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_leituras_prova_id_chip_id_ponto_controle",
                table: "leituras",
                columns: new[] { "prova_id", "chip_id", "ponto_controle" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inscricoes");

            migrationBuilder.DropTable(
                name: "leituras");

            migrationBuilder.DropTable(
                name: "atletas");

            migrationBuilder.DropTable(
                name: "categorias");

            migrationBuilder.DropTable(
                name: "provas");
        }
    }
}
