using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComprarProgramada.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CestasTopFive",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ativa = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataDesativacao = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CestasTopFive", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cpf = table.Column<string>(type: "varchar(11)", maxLength: 11, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ValorMensal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataAdesao = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DataSaida = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ContasMaster",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NumeroConta = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContasMaster", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CestaItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CestaTopFiveId = table.Column<int>(type: "int", nullable: false),
                    Ticker = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Percentual = table.Column<decimal>(type: "decimal(6,3)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CestaItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CestaItens_CestasTopFive_CestaTopFiveId",
                        column: x => x.CestaTopFiveId,
                        principalTable: "CestasTopFive",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ContasFilhote",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NumeroConta = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContasFilhote", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContasFilhote_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CustodiasMaster",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ContaMasterId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustodiasMaster", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustodiasMaster_ContasMaster_ContaMasterId",
                        column: x => x.ContaMasterId,
                        principalTable: "ContasMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrdensCompra",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ContaMasterId = table.Column<int>(type: "int", nullable: false),
                    CestaTopFiveId = table.Column<int>(type: "int", nullable: false),
                    DataCompra = table.Column<DateOnly>(type: "date", nullable: false),
                    ValorTotalConsolidado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdensCompra", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdensCompra_CestasTopFive_CestaTopFiveId",
                        column: x => x.CestaTopFiveId,
                        principalTable: "CestasTopFive",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdensCompra_ContasMaster_ContaMasterId",
                        column: x => x.ContaMasterId,
                        principalTable: "ContasMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CustodiasFilhote",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ContaFilhoteId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustodiasFilhote", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustodiasFilhote_ContasFilhote_ContaFilhoteId",
                        column: x => x.ContaFilhoteId,
                        principalTable: "ContasFilhote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CustodiasMasterItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CustodiaMasterId = table.Column<int>(type: "int", nullable: false),
                    Ticker = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantidade = table.Column<int>(type: "int", nullable: false),
                    PrecoMedio = table.Column<decimal>(type: "decimal(18,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustodiasMasterItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustodiasMasterItens_CustodiasMaster_CustodiaMasterId",
                        column: x => x.CustodiaMasterId,
                        principalTable: "CustodiasMaster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Distribuicoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OrdemCompraId = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ContaFilhoteId = table.Column<int>(type: "int", nullable: false),
                    DataDistribuicao = table.Column<DateOnly>(type: "date", nullable: false),
                    ValorAporte = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValorTotalIrDedoDuro = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Distribuicoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Distribuicoes_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Distribuicoes_OrdensCompra_OrdemCompraId",
                        column: x => x.OrdemCompraId,
                        principalTable: "OrdensCompra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrdensCompraItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OrdemCompraId = table.Column<int>(type: "int", nullable: false),
                    Ticker = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    QuantidadeLotePadrao = table.Column<int>(type: "int", nullable: false),
                    QuantidadeFracionario = table.Column<int>(type: "int", nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "decimal(18,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdensCompraItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdensCompraItens_OrdensCompra_OrdemCompraId",
                        column: x => x.OrdemCompraId,
                        principalTable: "OrdensCompra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CustodiasFilhoteItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CustodiaFilhoteId = table.Column<int>(type: "int", nullable: false),
                    Ticker = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantidade = table.Column<int>(type: "int", nullable: false),
                    PrecoMedio = table.Column<decimal>(type: "decimal(18,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustodiasFilhoteItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustodiasFilhoteItens_CustodiasFilhote_CustodiaFilhoteId",
                        column: x => x.CustodiaFilhoteId,
                        principalTable: "CustodiasFilhote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DistribuicaoItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DistribuicaoId = table.Column<int>(type: "int", nullable: false),
                    Ticker = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantidade = table.Column<int>(type: "int", nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    ValorOperacao = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ValorIrDedoDuro = table.Column<decimal>(type: "decimal(18,6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistribuicaoItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DistribuicaoItens_Distribuicoes_DistribuicaoId",
                        column: x => x.DistribuicaoId,
                        principalTable: "Distribuicoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CestaItens_CestaTopFiveId_Ticker",
                table: "CestaItens",
                columns: new[] { "CestaTopFiveId", "Ticker" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CestasTopFive_Ativa",
                table: "CestasTopFive",
                column: "Ativa");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Cpf",
                table: "Clientes",
                column: "Cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContasFilhote_ClienteId",
                table: "ContasFilhote",
                column: "ClienteId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContasFilhote_NumeroConta",
                table: "ContasFilhote",
                column: "NumeroConta",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContasMaster_NumeroConta",
                table: "ContasMaster",
                column: "NumeroConta",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustodiasFilhote_ContaFilhoteId",
                table: "CustodiasFilhote",
                column: "ContaFilhoteId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustodiasFilhoteItens_CustodiaFilhoteId_Ticker",
                table: "CustodiasFilhoteItens",
                columns: new[] { "CustodiaFilhoteId", "Ticker" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustodiasMaster_ContaMasterId",
                table: "CustodiasMaster",
                column: "ContaMasterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustodiasMasterItens_CustodiaMasterId_Ticker",
                table: "CustodiasMasterItens",
                columns: new[] { "CustodiaMasterId", "Ticker" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DistribuicaoItens_DistribuicaoId",
                table: "DistribuicaoItens",
                column: "DistribuicaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Distribuicoes_ClienteId",
                table: "Distribuicoes",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Distribuicoes_OrdemCompraId_ClienteId",
                table: "Distribuicoes",
                columns: new[] { "OrdemCompraId", "ClienteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdensCompra_CestaTopFiveId",
                table: "OrdensCompra",
                column: "CestaTopFiveId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdensCompra_ContaMasterId",
                table: "OrdensCompra",
                column: "ContaMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdensCompra_DataCompra",
                table: "OrdensCompra",
                column: "DataCompra",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdensCompraItens_OrdemCompraId_Ticker",
                table: "OrdensCompraItens",
                columns: new[] { "OrdemCompraId", "Ticker" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CestaItens");

            migrationBuilder.DropTable(
                name: "CustodiasFilhoteItens");

            migrationBuilder.DropTable(
                name: "CustodiasMasterItens");

            migrationBuilder.DropTable(
                name: "DistribuicaoItens");

            migrationBuilder.DropTable(
                name: "OrdensCompraItens");

            migrationBuilder.DropTable(
                name: "CustodiasFilhote");

            migrationBuilder.DropTable(
                name: "CustodiasMaster");

            migrationBuilder.DropTable(
                name: "Distribuicoes");

            migrationBuilder.DropTable(
                name: "ContasFilhote");

            migrationBuilder.DropTable(
                name: "OrdensCompra");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "CestasTopFive");

            migrationBuilder.DropTable(
                name: "ContasMaster");
        }
    }
}
