using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryApp.Migrations
{
    /// <inheritdoc />
    public partial class user : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_AppUsers_UserId",
                table: "Addresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Advertisements_AppUsers_CreatedById",
                table: "Advertisements");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryStatuses_AppUsers_Id",
                table: "DeliveryStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteRestaurants_AppUsers_UserId",
                table: "FavoriteRestaurants");

            migrationBuilder.DropForeignKey(
                name: "FK_Locations_AppUsers_Id",
                table: "Locations");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AppUsers_DeliveryPersonId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AppUsers_UserId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentMethods_AppUsers_UserId",
                table: "PaymentMethods");

            migrationBuilder.DropForeignKey(
                name: "FK_Restaurants_AppUsers_OwnerId",
                table: "Restaurants");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_AppUsers_UserId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_StripeCustomers_AppUsers_UserId",
                table: "StripeCustomers");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPreferences_AppUsers_Id",
                table: "UserPreferences");

            migrationBuilder.DropTable(
                name: "AppUsers");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AbpUsers",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProfileImageUrl",
                table: "AbpUsers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewReason",
                table: "AbpUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewStatus",
                table: "AbpUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_AbpUsers_UserId",
                table: "Addresses",
                column: "UserId",
                principalTable: "AbpUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Advertisements_AbpUsers_CreatedById",
                table: "Advertisements",
                column: "CreatedById",
                principalTable: "AbpUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryStatuses_AbpUsers_Id",
                table: "DeliveryStatuses",
                column: "Id",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteRestaurants_AbpUsers_UserId",
                table: "FavoriteRestaurants",
                column: "UserId",
                principalTable: "AbpUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_AbpUsers_Id",
                table: "Locations",
                column: "Id",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AbpUsers_DeliveryPersonId",
                table: "Orders",
                column: "DeliveryPersonId",
                principalTable: "AbpUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AbpUsers_UserId",
                table: "Orders",
                column: "UserId",
                principalTable: "AbpUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentMethods_AbpUsers_UserId",
                table: "PaymentMethods",
                column: "UserId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Restaurants_AbpUsers_OwnerId",
                table: "Restaurants",
                column: "OwnerId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_AbpUsers_UserId",
                table: "Reviews",
                column: "UserId",
                principalTable: "AbpUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StripeCustomers_AbpUsers_UserId",
                table: "StripeCustomers",
                column: "UserId",
                principalTable: "AbpUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPreferences_AbpUsers_Id",
                table: "UserPreferences",
                column: "Id",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_AbpUsers_UserId",
                table: "Addresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Advertisements_AbpUsers_CreatedById",
                table: "Advertisements");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryStatuses_AbpUsers_Id",
                table: "DeliveryStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteRestaurants_AbpUsers_UserId",
                table: "FavoriteRestaurants");

            migrationBuilder.DropForeignKey(
                name: "FK_Locations_AbpUsers_Id",
                table: "Locations");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AbpUsers_DeliveryPersonId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AbpUsers_UserId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentMethods_AbpUsers_UserId",
                table: "PaymentMethods");

            migrationBuilder.DropForeignKey(
                name: "FK_Restaurants_AbpUsers_OwnerId",
                table: "Restaurants");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_AbpUsers_UserId",
                table: "Reviews");

            migrationBuilder.DropForeignKey(
                name: "FK_StripeCustomers_AbpUsers_UserId",
                table: "StripeCustomers");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPreferences_AbpUsers_Id",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "ProfileImageUrl",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "ReviewReason",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "ReviewStatus",
                table: "AbpUsers");

            migrationBuilder.CreateTable(
                name: "AppUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProfileImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ReviewReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewStatus = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppUsers_AbpUsers_Id",
                        column: x => x.Id,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_AppUsers_UserId",
                table: "Addresses",
                column: "UserId",
                principalTable: "AppUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Advertisements_AppUsers_CreatedById",
                table: "Advertisements",
                column: "CreatedById",
                principalTable: "AppUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryStatuses_AppUsers_Id",
                table: "DeliveryStatuses",
                column: "Id",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteRestaurants_AppUsers_UserId",
                table: "FavoriteRestaurants",
                column: "UserId",
                principalTable: "AppUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_AppUsers_Id",
                table: "Locations",
                column: "Id",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AppUsers_DeliveryPersonId",
                table: "Orders",
                column: "DeliveryPersonId",
                principalTable: "AppUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AppUsers_UserId",
                table: "Orders",
                column: "UserId",
                principalTable: "AppUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentMethods_AppUsers_UserId",
                table: "PaymentMethods",
                column: "UserId",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Restaurants_AppUsers_OwnerId",
                table: "Restaurants",
                column: "OwnerId",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_AppUsers_UserId",
                table: "Reviews",
                column: "UserId",
                principalTable: "AppUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StripeCustomers_AppUsers_UserId",
                table: "StripeCustomers",
                column: "UserId",
                principalTable: "AppUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPreferences_AppUsers_Id",
                table: "UserPreferences",
                column: "Id",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
