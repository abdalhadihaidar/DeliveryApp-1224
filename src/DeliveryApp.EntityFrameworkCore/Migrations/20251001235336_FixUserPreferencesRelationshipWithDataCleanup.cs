using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryApp.Migrations
{
    /// <inheritdoc />
    public partial class FixUserPreferencesRelationshipWithDataCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPreferences_AbpUsers_Id",
                table: "UserPreferences");

            // Add UserId column as nullable first
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "UserPreferences",
                type: "uniqueidentifier",
                nullable: true);

            // Update existing UserPreferences records to link them to their corresponding users
            // Since UserPreferences.Id was previously used as the foreign key to AbpUsers.Id,
            // we can use that to populate the new UserId column
            migrationBuilder.Sql(@"
                UPDATE UserPreferences 
                SET UserId = Id 
                WHERE UserId IS NULL AND Id IN (SELECT Id FROM AbpUsers)
            ");

            // Delete any UserPreferences that don't have a corresponding user
            migrationBuilder.Sql(@"
                DELETE FROM UserPreferences 
                WHERE UserId IS NULL OR Id NOT IN (SELECT Id FROM AbpUsers)
            ");

            // Make UserId non-nullable
            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "UserPreferences",
                type: "uniqueidentifier",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_UserId",
                table: "UserPreferences",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPreferences_AbpUsers_UserId",
                table: "UserPreferences",
                column: "UserId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPreferences_AbpUsers_UserId",
                table: "UserPreferences");

            migrationBuilder.DropIndex(
                name: "IX_UserPreferences_UserId",
                table: "UserPreferences");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserPreferences");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPreferences_AbpUsers_Id",
                table: "UserPreferences",
                column: "Id",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
