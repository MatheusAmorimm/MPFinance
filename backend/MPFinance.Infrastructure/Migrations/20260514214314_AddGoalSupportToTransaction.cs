using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MPFinance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGoalSupportToTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GoalId",
                table: "transactions",
                type: "char(36)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsGoalDeposit",
                table: "transactions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoalId",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "IsGoalDeposit",
                table: "transactions");
        }
    }
}
