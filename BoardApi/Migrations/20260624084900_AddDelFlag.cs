using Microsoft.EntityFrameworkCore.Migrations;

namespace BoardApi.Migrations
{
    public partial class AddDelFlag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DelFlag",
                table: "Posts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DelFlag",
                table: "Comments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DelFlag",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "DelFlag",
                table: "Comments");
        }
    }
}
