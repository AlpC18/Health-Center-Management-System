using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WellnessAPI.Migrations
{
    public partial class AddAdressaToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Adresa",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Adresa",
                table: "AspNetUsers");
        }
    }
}
