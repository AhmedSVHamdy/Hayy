using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserLogs_Users_UserId",
                table: "UserLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_UserLogs_Users_UserId1",
                table: "UserLogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserLogs",
                table: "UserLogs");

            migrationBuilder.DropIndex(
                name: "IX_UserLogs_UserId1",
                table: "UserLogs");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "UserLogs");

            migrationBuilder.RenameTable(
                name: "UserLogs",
                newName: "UserLog");

            migrationBuilder.RenameIndex(
                name: "IX_UserLogs_UserId",
                table: "UserLog",
                newName: "IX_UserLog_UserId");

            migrationBuilder.AlterColumn<int>(
                name: "TargetType",
                table: "UserLog",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "SearchQuery",
                table: "UserLog",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Duration",
                table: "UserLog",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "ActionType",
                table: "UserLog",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserLog",
                table: "UserLog",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserLog_Users_UserId",
                table: "UserLog",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserLog_Users_UserId",
                table: "UserLog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserLog",
                table: "UserLog");

            migrationBuilder.RenameTable(
                name: "UserLog",
                newName: "UserLogs");

            migrationBuilder.RenameIndex(
                name: "IX_UserLog_UserId",
                table: "UserLogs",
                newName: "IX_UserLogs_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "TargetType",
                table: "UserLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "SearchQuery",
                table: "UserLogs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Duration",
                table: "UserLogs",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "ActionType",
                table: "UserLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "UserLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserLogs",
                table: "UserLogs",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogs_UserId1",
                table: "UserLogs",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_UserLogs_Users_UserId",
                table: "UserLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserLogs_Users_UserId1",
                table: "UserLogs",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
