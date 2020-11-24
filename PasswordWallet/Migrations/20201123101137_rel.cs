using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace PasswordWallet.Migrations
{
    public partial class rel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoginAttempt_AddressIP_AddressIpId",
                table: "LoginAttempt");

            migrationBuilder.DropForeignKey(
                name: "FK_LoginAttempt_Users_UserId",
                table: "LoginAttempt");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LoginAttempt",
                table: "LoginAttempt");

            migrationBuilder.DropIndex(
                name: "IX_LoginAttempt_AddressIpId",
                table: "LoginAttempt");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AddressIP",
                table: "AddressIP");

            migrationBuilder.DropColumn(
                name: "AddressIpId",
                table: "LoginAttempt");

            migrationBuilder.DropColumn(
                name: "Successful",
                table: "AddressIP");

            migrationBuilder.RenameTable(
                name: "LoginAttempt",
                newName: "LoginAttempts");

            migrationBuilder.RenameTable(
                name: "AddressIP",
                newName: "AddressIPs");

            migrationBuilder.RenameIndex(
                name: "IX_LoginAttempt_UserId",
                table: "LoginAttempts",
                newName: "IX_LoginAttempts_UserId");

            migrationBuilder.AddColumn<string>(
                name: "AddressIp",
                table: "LoginAttempts",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "AddressIPs",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_LoginAttempts",
                table: "LoginAttempts",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AddressIPs",
                table: "AddressIPs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LoginAttempts_Users_UserId",
                table: "LoginAttempts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoginAttempts_Users_UserId",
                table: "LoginAttempts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LoginAttempts",
                table: "LoginAttempts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AddressIPs",
                table: "AddressIPs");

            migrationBuilder.DropColumn(
                name: "AddressIp",
                table: "LoginAttempts");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "AddressIPs");

            migrationBuilder.RenameTable(
                name: "LoginAttempts",
                newName: "LoginAttempt");

            migrationBuilder.RenameTable(
                name: "AddressIPs",
                newName: "AddressIP");

            migrationBuilder.RenameIndex(
                name: "IX_LoginAttempts_UserId",
                table: "LoginAttempt",
                newName: "IX_LoginAttempt_UserId");

            migrationBuilder.AddColumn<int>(
                name: "AddressIpId",
                table: "LoginAttempt",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Successful",
                table: "AddressIP",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_LoginAttempt",
                table: "LoginAttempt",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AddressIP",
                table: "AddressIP",
                column: "Id");

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

            migrationBuilder.AddForeignKey(
                name: "FK_LoginAttempt_Users_UserId",
                table: "LoginAttempt",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
