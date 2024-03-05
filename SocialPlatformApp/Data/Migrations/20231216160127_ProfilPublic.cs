using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialPlatformApp.Data.Migrations
{
    public partial class ProfilPublic : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PublicProfile",
                table: "Profiles",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublicProfile",
                table: "Profiles");
        }
    }
}
