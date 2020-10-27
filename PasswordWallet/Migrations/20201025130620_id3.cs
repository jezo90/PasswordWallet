using Microsoft.EntityFrameworkCore.Migrations;

namespace PasswordWallet.Migrations
{
    public partial class id3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Passwds",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Passwds_UserId",
                table: "Passwds",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Passwds_Users_UserId",
                table: "Passwds",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Passwds_Users_UserId",
                table: "Passwds");

            migrationBuilder.DropIndex(
                name: "IX_Passwds_UserId",
                table: "Passwds");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Passwds");
        }
    }
}
