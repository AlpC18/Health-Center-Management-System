using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WellnessAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddFotoPathToKlient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FotoPath",
                table: "Klientet",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FotoPath",
                table: "Klientet");
        }
    }
}
