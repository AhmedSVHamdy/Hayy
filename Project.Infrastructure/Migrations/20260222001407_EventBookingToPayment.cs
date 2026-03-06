using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EventBookingToPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. إضافة عمود الحجز لجدول المدفوعات (زي ما هو)
            migrationBuilder.AddColumn<Guid>(
                name: "EventBookingId",
                table: "Payments",
                type: "uniqueidentifier",
                nullable: true);

            // 2. حل مشكلة الـ RowVersion: نمسح العمود القديم الأول
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Events");

            // 3. ننشئ العمود الجديد كـ rowversion نضيف من الصفر
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Events",
                type: "rowversion",
                rowVersion: true,
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventBookingId",
                table: "Payments");

            // نمسح الـ rowversion
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Events");

            // نرجعه زي ما كان varbinary(max)
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Events",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
