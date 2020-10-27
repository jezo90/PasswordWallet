using Microsoft.EntityFrameworkCore.Migrations;

namespace PasswordWallet.Migrations
{
    public partial class webaddress : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WebAddress",
                table: "Passwds",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WebAddress",
                table: "Passwds");
        }
    }
}
