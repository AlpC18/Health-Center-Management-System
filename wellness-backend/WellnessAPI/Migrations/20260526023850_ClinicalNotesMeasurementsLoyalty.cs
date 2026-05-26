using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WellnessAPI.Migrations
{
    /// <inheritdoc />
    public partial class ClinicalNotesMeasurementsLoyalty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TerapistId",
                table: "AspNetUsers",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "KlientMatjet",
                columns: table => new
                {
                    MatjeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    KlientId = table.Column<int>(type: "int", nullable: false),
                    DataMatjes = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PeshaKg = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    GjatesiaCm = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    YndyraTrupore = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    BeliCm = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    KofshaCm = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    Shenim = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KlientMatjet", x => x.MatjeId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "KlientPikat",
                columns: table => new
                {
                    PikaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    KlientId = table.Column<int>(type: "int", nullable: false),
                    Pike = table.Column<int>(type: "int", nullable: false),
                    Tipi = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LidhjeId = table.Column<int>(type: "int", nullable: true),
                    Shenim = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataKrijimit = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KlientPikat", x => x.PikaId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "KlientShenime",
                columns: table => new
                {
                    ShenimId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    KlientId = table.Column<int>(type: "int", nullable: false),
                    TerminId = table.Column<int>(type: "int", nullable: true),
                    TerapistId = table.Column<int>(type: "int", nullable: true),
                    Tipi = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Permbajtja = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Privat = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DataKrijimit = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KlientShenime", x => x.ShenimId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_KlientMatjet_KlientId_DataMatjes",
                table: "KlientMatjet",
                columns: new[] { "KlientId", "DataMatjes" });

            migrationBuilder.CreateIndex(
                name: "IX_KlientPikat_DataKrijimit",
                table: "KlientPikat",
                column: "DataKrijimit");

            migrationBuilder.CreateIndex(
                name: "IX_KlientPikat_KlientId",
                table: "KlientPikat",
                column: "KlientId");

            migrationBuilder.CreateIndex(
                name: "IX_KlientShenime_DataKrijimit",
                table: "KlientShenime",
                column: "DataKrijimit");

            migrationBuilder.CreateIndex(
                name: "IX_KlientShenime_KlientId",
                table: "KlientShenime",
                column: "KlientId");

            migrationBuilder.CreateIndex(
                name: "IX_KlientShenime_TerminId",
                table: "KlientShenime",
                column: "TerminId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KlientMatjet");

            migrationBuilder.DropTable(
                name: "KlientPikat");

            migrationBuilder.DropTable(
                name: "KlientShenime");

            migrationBuilder.DropColumn(
                name: "TerapistId",
                table: "AspNetUsers");
        }
    }
}
