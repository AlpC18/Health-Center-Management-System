using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WellnessAPI.Migrations
{
    public partial class AddPaymentFieldsAndReviewConstraint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Task 3: Add payment type/status columns to ShitjetProduktet
            migrationBuilder.AddColumn<string>(
                name: "TipiPageses",
                table: "ShitjetProduktet",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "Kesh");

            migrationBuilder.AddColumn<string>(
                name: "StatusiPageses",
                table: "ShitjetProduktet",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "Paguar");

            // Task 9: Unique constraint on Vlereisimet (KlientId, SherbimId)
            migrationBuilder.CreateIndex(
                name: "IX_Vlereisimet_KlientId_SherbimId",
                table: "Vlereisimet",
                columns: new[] { "KlientId", "SherbimId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vlereisimet_KlientId_SherbimId",
                table: "Vlereisimet");

            migrationBuilder.DropColumn(
                name: "TipiPageses",
                table: "ShitjetProduktet");

            migrationBuilder.DropColumn(
                name: "StatusiPageses",
                table: "ShitjetProduktet");
        }
    }
}
