using Microsoft.EntityFrameworkCore.Migrations;

namespace PasswordWallet.Migrations
{
    public partial class sharedpassword3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SharedPasswds_PasswdId",
                table: "SharedPasswds",
                column: "PasswdId");

            migrationBuilder.AddForeignKey(
                name: "FK_SharedPasswds_Passwds_PasswdId",
                table: "SharedPasswds",
                column: "PasswdId",
                principalTable: "Passwds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SharedPasswds_Passwds_PasswdId",
                table: "SharedPasswds");

            migrationBuilder.DropIndex(
                name: "IX_SharedPasswds_PasswdId",
                table: "SharedPasswds");
        }
    }
}
