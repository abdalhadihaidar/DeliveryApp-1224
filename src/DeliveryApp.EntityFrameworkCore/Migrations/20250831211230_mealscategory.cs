using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryApp.Migrations
{
    /// <inheritdoc />
    public partial class mealscategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "MenuItems");

            migrationBuilder.AddColumn<string>(
                name: "ApplicableDays",
                table: "SpecialOffers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "SpecialOffers",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "SpecialOffers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "SpecialOffers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentOccurrences",
                table: "SpecialOffers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentUses",
                table: "SpecialOffers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "DeleterId",
                table: "SpecialOffers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "SpecialOffers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                table: "SpecialOffers",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtraProperties",
                table: "SpecialOffers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "SpecialOffers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SpecialOffers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRecurring",
                table: "SpecialOffers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "SpecialOffers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LastModifierId",
                table: "SpecialOffers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUsed",
                table: "SpecialOffers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxOccurrences",
                table: "SpecialOffers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxUses",
                table: "SpecialOffers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextOccurrence",
                table: "SpecialOffers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "SpecialOffers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RecurrencePattern",
                table: "SpecialOffers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "SpecialOffers",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "SpecialOffers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "RestaurantPayouts",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "PaymentTransactions",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<Guid>(
                name: "MealCategoryId",
                table: "MenuItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "FinancialTransactions",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "ReviewStatus",
                table: "AbpUsers",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceToken",
                table: "AbpUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AdRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RestaurantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LinkUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    TargetAudience = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReviewReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReviewedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdvertisementId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Budget = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdRequests_AbpUsers_ReviewedById",
                        column: x => x.ReviewedById,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AdRequests_Advertisements_AdvertisementId",
                        column: x => x.AdvertisementId,
                        principalTable: "Advertisements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AdRequests_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MealCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RestaurantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MealCategories_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_MealCategoryId",
                table: "MenuItems",
                column: "MealCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AdRequests_AdvertisementId",
                table: "AdRequests",
                column: "AdvertisementId");

            migrationBuilder.CreateIndex(
                name: "IX_AdRequests_RestaurantId",
                table: "AdRequests",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_AdRequests_ReviewedById",
                table: "AdRequests",
                column: "ReviewedById");

            migrationBuilder.CreateIndex(
                name: "IX_MealCategories_RestaurantId_Name",
                table: "MealCategories",
                columns: new[] { "RestaurantId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_MealCategories_MealCategoryId",
                table: "MenuItems",
                column: "MealCategoryId",
                principalTable: "MealCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_MealCategories_MealCategoryId",
                table: "MenuItems");

            migrationBuilder.DropTable(
                name: "AdRequests");

            migrationBuilder.DropTable(
                name: "MealCategories");

            migrationBuilder.DropIndex(
                name: "IX_MenuItems_MealCategoryId",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "ApplicableDays",
                table: "SpecialOffers");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "SpecialOffers");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "SpecialOffers");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "SpecialOffers");

            migrationBuilder.DropColumn(
                name: "CurrentOccurrences",
                table: "SpecialOffers");

            migrationBuilder.DropColumn(
                name: "CurrentUses",
                table: "SpecialOffers");

            migrationBuilder.DropColumn(
                name: "DeleterId",
                table: "SpecialOffers");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "SpecialOffers");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "SpecialOffers");

            migrationBuilder.DropColumn(
                name: "ExtraProperties",
                table: "SpecialOffers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "SpecialOffers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SpecialOffers");

            migrationBuilder.DropColumn(
                name: "IsRecurring",
                table: "SpecialOffers");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "SpecialOffers");

            migrationBuilder.DropColumn(
                name: "LastModifierId",
                table: "SpecialOffers");

            migrationBuilder.DropColumn(
                name: "LastUsed",
                table: "SpecialOffers");

            migrationBuilder.DropColumn(
                name: "MaxOccurrences",
                table: "SpecialOffers");

            migrationBuilder.DropColumn(
                name: "MaxUses",
                table: "SpecialOffers");

            migrationBuilder.DropColumn(
                name: "NextOccurrence",
                table: "SpecialOffers");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "SpecialOffers");

            migrationBuilder.DropColumn(
                name: "RecurrencePattern",
                table: "SpecialOffers");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "SpecialOffers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "SpecialOffers");

            migrationBuilder.DropColumn(
                name: "MealCategoryId",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "DeviceToken",
                table: "AbpUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "RestaurantPayouts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "PaymentTransactions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "MenuItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "FinancialTransactions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "ReviewStatus",
                table: "AbpUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
