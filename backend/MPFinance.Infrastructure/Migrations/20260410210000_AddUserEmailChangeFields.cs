using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MPFinance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserEmailChangeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EmailChangedAt",
                table: "users",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PendingEmail",
                table: "users",
                type: "varchar(150)",
                maxLength: 150,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "EmailChangedAt", table: "users");
            migrationBuilder.DropColumn(name: "PendingEmail",   table: "users");
        }
    }
}
