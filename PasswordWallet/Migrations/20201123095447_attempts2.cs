using Microsoft.EntityFrameworkCore.Migrations;

namespace PasswordWallet.Migrations
{
    public partial class attempts2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressIP",
                table: "LoginAttempt");

            migrationBuilder.AddColumn<int>(
                name: "AddressIpId",
                table: "LoginAttempt",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AddressIP",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Address = table.Column<string>(nullable: false),
                    Successful = table.Column<bool>(nullable: false),
                    Correct = table.Column<int>(nullable: false),
                    Incorrect = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressIP", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempt_AddressIpId",
                table: "LoginAttempt",
                column: "AddressIpId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoginAttempt_AddressIP_AddressIpId",
                table: "LoginAttempt",
                column: "AddressIpId",
                principalTable: "AddressIP",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoginAttempt_AddressIP_AddressIpId",
                table: "LoginAttempt");

            migrationBuilder.DropTable(
                name: "AddressIP");

            migrationBuilder.DropIndex(
                name: "IX_LoginAttempt_AddressIpId",
                table: "LoginAttempt");

            migrationBuilder.DropColumn(
                name: "AddressIpId",
                table: "LoginAttempt");

            migrationBuilder.AddColumn<string>(
                name: "AddressIP",
                table: "LoginAttempt",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
