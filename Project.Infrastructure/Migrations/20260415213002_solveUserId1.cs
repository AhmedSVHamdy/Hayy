using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class solveUserId1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "Businesses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Businesses_UserId1",
                table: "Businesses",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Businesses_Users_UserId1",
                table: "Businesses",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
