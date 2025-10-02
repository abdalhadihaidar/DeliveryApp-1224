using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryApp.Migrations
{
    /// <inheritdoc />
    public partial class flow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Restaurants");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "Restaurants",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Orders",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Discriminator",
                table: "AbpUsers",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "IdentityUser",
                oldClrType: typeof(string),
                oldType: "nvarchar(13)",
                oldMaxLength: 13);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedById",
                table: "AbpUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedTime",
                table: "AbpUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAdminApproved",
                table: "AbpUsers",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailConfirmed",
                table: "AbpUsers",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPhoneConfirmed",
                table: "AbpUsers",
                type: "bit",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RestaurantCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_RestaurantCategories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_CategoryId",
                table: "Restaurants",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantCategories_Name",
                table: "RestaurantCategories",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Restaurants_RestaurantCategories_CategoryId",
                table: "Restaurants",
                column: "CategoryId",
                principalTable: "RestaurantCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Restaurants_RestaurantCategories_CategoryId",
                table: "Restaurants");

            migrationBuilder.DropTable(
                name: "RestaurantCategories");

            migrationBuilder.DropIndex(
                name: "IX_Restaurants_CategoryId",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "ApprovedById",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "ApprovedTime",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "IsAdminApproved",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "IsEmailConfirmed",
                table: "AbpUsers");

            migrationBuilder.DropColumn(
                name: "IsPhoneConfirmed",
                table: "AbpUsers");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Restaurants",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Discriminator",
                table: "AbpUsers",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(13)",
                oldMaxLength: 13,
                oldDefaultValue: "IdentityUser");
        }
    }
}
