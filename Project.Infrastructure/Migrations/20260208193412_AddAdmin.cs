using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdminActions_Admins_AdminId",
                table: "AdminActions");

            migrationBuilder.DropForeignKey(
                name: "FK_BusinessVerifications_Admins_AdminId",
                table: "BusinessVerifications");

            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropIndex(
                name: "IX_BusinessVerifications_BusinessId",
                table: "BusinessVerifications");

            migrationBuilder.AlterColumn<string>(
                name: "TargetId",
                table: "AdminActions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessVerifications_BusinessId",
                table: "BusinessVerifications",
                column: "BusinessId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdminActions_Users_AdminId",
                table: "AdminActions",
                column: "AdminId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessVerifications_Users_AdminId",
                table: "BusinessVerifications",
                column: "AdminId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdminActions_Users_AdminId",
                table: "AdminActions");

            migrationBuilder.DropForeignKey(
                name: "FK_BusinessVerifications_Users_AdminId",
                table: "BusinessVerifications");

            migrationBuilder.DropIndex(
                name: "IX_BusinessVerifications_BusinessId",
                table: "BusinessVerifications");

            migrationBuilder.AlterColumn<Guid>(
                name: "TargetId",
                table: "AdminActions",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfileImage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessVerifications_BusinessId",
                table: "BusinessVerifications",
                column: "BusinessId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Admins_Email",
                table: "Admins",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AdminActions_Admins_AdminId",
                table: "AdminActions",
                column: "AdminId",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessVerifications_Admins_AdminId",
                table: "BusinessVerifications",
                column: "AdminId",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
