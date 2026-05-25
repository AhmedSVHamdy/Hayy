using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Infrastructure.Migrations // اتأكد إن الـ namespace نفس اللي عندك
{
    public partial class AddEventPaymentTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // إنشاء جدول الدفع فقط بدون اللعب في الجداول التانية
            migrationBuilder.CreateTable(
                name: "EventPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PaymobOrderId = table.Column<long>(type: "bigint", nullable: true),
                    PaymobTransactionId = table.Column<long>(type: "bigint", nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventPayments", x => x.Id);

                    // علاقة الدفع بالحدث
                    table.ForeignKey(
                        name: "FK_EventPayments_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade); // أو NoAction لو كنت مغيرها في الكونفيجراشن

                    // علاقة الدفع باليوزر
                    table.ForeignKey(
                        name: "FK_EventPayments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade); // أو NoAction
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventPayments_EventId",
                table: "EventPayments",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventPayments_PaymobOrderId",
                table: "EventPayments",
                column: "PaymobOrderId",
                unique: true,
                filter: "[PaymobOrderId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EventPayments_UserId",
                table: "EventPayments",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventPayments");
        }
    }
}