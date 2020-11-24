using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace PasswordWallet.Migrations
{
    public partial class zmiany : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Date",
                table: "AddressIPs");

            migrationBuilder.AddColumn<DateTime>(
                name: "AccountBlockDate",
                table: "Users",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "IpBlockDate",
                table: "AddressIPs",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountBlockDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IpBlockDate",
                table: "AddressIPs");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "AddressIPs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
