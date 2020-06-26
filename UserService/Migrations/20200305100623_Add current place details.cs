using Microsoft.EntityFrameworkCore.Migrations;

namespace UserService.Migrations
{
    public partial class Addcurrentplacedetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentCity",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentCountry",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentCity",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CurrentCountry",
                table: "AspNetUsers");
        }
    }
}
