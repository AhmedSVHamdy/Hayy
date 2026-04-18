using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropShadowPropertiesClean : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BusinessPlans_Businesses_BusinessId1",
                table: "BusinessPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_BusinessPlans_SubscriptionPlans_SubscriptionPlanId",
                table: "BusinessPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_BusinessPlans_SubscriptionPlans_SubscriptionPlanId1",
                table: "BusinessPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_EventBookings_Users_UserId1",
                table: "EventBookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_Places_PlaceId1",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_UserId1",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Offers_Places_PlaceId1",
                table: "Offers");

            migrationBuilder.DropForeignKey(
                name: "FK_PlaceFollows_Users_UserId1",
                table: "PlaceFollows");

            migrationBuilder.DropForeignKey(
                name: "FK_Places_Businesses_BusinessId1",
                table: "Places");

            migrationBuilder.DropForeignKey(
                name: "FK_PostComments_Users_UserId1",
                table: "PostComments");

            migrationBuilder.DropForeignKey(
                name: "FK_PostLikes_Users_UserId1",
                table: "PostLikes");

            // 1. اعمل كومنت لده (لأنه اتمسح من السيكوال خلاص)
            // migrationBuilder.DropForeignKey(
            //    name: "FK_UserInterestProfiles_Users_UserId1",
            //    table: "UserInterestProfiles");
            
            // 2. اعمل كومنت لده كمان (الاندكس المرتبط بيه)
            // migrationBuilder.DropIndex(
            //    name: "IX_UserInterestProfiles_UserId1",
            //    table: "UserInterestProfiles");

            // 3. وللأمان اعمل كومنت لمسح العمود نفسه لو كنت مسحته بردو
            // migrationBuilder.DropColumn(
            //    name: "UserId1",
            //    table: "UserInterestProfiles");

            // migrationBuilder.DropIndex(
            //    name: "IX_Reviews_UserId1",
            //    table: "Reviews");

            // 2. انزل تحت شوية واعمل كومنت لده
            // migrationBuilder.DropIndex(
            //    name: "IX_RecommendedItems_UserId1",
            //    table: "RecommendedItems");

            migrationBuilder.DropIndex(
                name: "IX_PostLikes_UserId1",
                table: "PostLikes");

            migrationBuilder.DropIndex(
                name: "IX_PostComments_UserId1",
                table: "PostComments");

            migrationBuilder.DropIndex(
                name: "IX_Places_BusinessId1",
                table: "Places");

            migrationBuilder.DropIndex(
                name: "IX_PlaceFollows_UserId1",
                table: "PlaceFollows");

            migrationBuilder.DropIndex(
                name: "IX_Offers_PlaceId1",
                table: "Offers");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId1",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Events_PlaceId1",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_EventBookings_UserId1",
                table: "EventBookings");

            migrationBuilder.DropIndex(
                name: "IX_BusinessPlans_BusinessId1",
                table: "BusinessPlans");

            migrationBuilder.DropIndex(
                name: "IX_BusinessPlans_SubscriptionPlanId",
                table: "BusinessPlans");

            migrationBuilder.DropIndex(
                name: "IX_BusinessPlans_SubscriptionPlanId1",
                table: "BusinessPlans");

            // 3. وانزل تحت كمان واعمل كومنت لده
            // migrationBuilder.DropColumn(
            //    name: "UserId1",
            //    table: "RecommendedItems");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "PostLikes");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "PostComments");

            migrationBuilder.DropColumn(
                name: "BusinessId1",
                table: "Places");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "PlaceFollows");

            migrationBuilder.DropColumn(
                name: "PlaceId1",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "PlaceId1",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "EventBookings");

            migrationBuilder.DropColumn(
                name: "BusinessId1",
                table: "BusinessPlans");

            migrationBuilder.DropColumn(
                name: "SubscriptionPlanId",
                table: "BusinessPlans");

            migrationBuilder.DropColumn(
                name: "SubscriptionPlanId1",
                table: "BusinessPlans");

            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessPlans_PlanId",
                table: "BusinessPlans",
                column: "PlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessPlans_SubscriptionPlans_PlanId",
                table: "BusinessPlans",
                column: "PlanId",
                principalTable: "SubscriptionPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BusinessPlans_SubscriptionPlans_PlanId",
                table: "BusinessPlans");

            migrationBuilder.DropIndex(
                name: "IX_BusinessPlans_PlanId",
                table: "BusinessPlans");

            migrationBuilder.AlterColumn<string>(
                name: "RefreshToken",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "Reviews",
                type: "uniqueidentifier",
                nullable: true);

            // 1. اعمل كومنت لده
            // migrationBuilder.AddColumn<Guid>(
            //    name: "UserId1",
            //    table: "RecommendedItems",
            //    type: "uniqueidentifier",
            //    nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "PostLikes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "PostComments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BusinessId1",
                table: "Places",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "PlaceFollows",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PlaceId1",
                table: "Offers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PlaceId1",
                table: "Events",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "EventBookings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BusinessId1",
                table: "BusinessPlans",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubscriptionPlanId",
                table: "BusinessPlans",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "SubscriptionPlanId1",
                table: "BusinessPlans",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId1",
                table: "Reviews",
                column: "UserId1");

            // 2. انزل تحت شوية واعمل كومنت لده
            // migrationBuilder.CreateIndex(
            //    name: "IX_RecommendedItems_UserId1",
            //    table: "RecommendedItems",
            //    column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_PostLikes_UserId1",
                table: "PostLikes",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_PostComments_UserId1",
                table: "PostComments",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Places_BusinessId1",
                table: "Places",
                column: "BusinessId1");

            migrationBuilder.CreateIndex(
                name: "IX_PlaceFollows_UserId1",
                table: "PlaceFollows",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Offers_PlaceId1",
                table: "Offers",
                column: "PlaceId1");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId1",
                table: "Notifications",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Events_PlaceId1",
                table: "Events",
                column: "PlaceId1");

            migrationBuilder.CreateIndex(
                name: "IX_EventBookings_UserId1",
                table: "EventBookings",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessPlans_BusinessId1",
                table: "BusinessPlans",
                column: "BusinessId1");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessPlans_SubscriptionPlanId",
                table: "BusinessPlans",
                column: "SubscriptionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessPlans_SubscriptionPlanId1",
                table: "BusinessPlans",
                column: "SubscriptionPlanId1");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessPlans_Businesses_BusinessId1",
                table: "BusinessPlans",
                column: "BusinessId1",
                principalTable: "Businesses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessPlans_SubscriptionPlans_SubscriptionPlanId",
                table: "BusinessPlans",
                column: "SubscriptionPlanId",
                principalTable: "SubscriptionPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessPlans_SubscriptionPlans_SubscriptionPlanId1",
                table: "BusinessPlans",
                column: "SubscriptionPlanId1",
                principalTable: "SubscriptionPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventBookings_Users_UserId1",
                table: "EventBookings",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Places_PlaceId1",
                table: "Events",
                column: "PlaceId1",
                principalTable: "Places",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_UserId1",
                table: "Notifications",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Offers_Places_PlaceId1",
                table: "Offers",
                column: "PlaceId1",
                principalTable: "Places",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlaceFollows_Users_UserId1",
                table: "PlaceFollows",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Places_Businesses_BusinessId1",
                table: "Places",
                column: "BusinessId1",
                principalTable: "Businesses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PostComments_Users_UserId1",
                table: "PostComments",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PostLikes_Users_UserId1",
                table: "PostLikes",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RecommendedItems_Users_UserId1",
                table: "RecommendedItems",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Users_UserId1",
                table: "Reviews",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserInterestProfiles_Users_UserId1",
                table: "UserInterestProfiles",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
