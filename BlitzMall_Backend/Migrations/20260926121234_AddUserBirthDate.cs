using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlitzMall_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddUserBirthDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BirthDate",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "Users");
        }
    }
}
