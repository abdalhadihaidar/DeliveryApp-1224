using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryApp.Migrations
{
    /// <inheritdoc />
    public partial class reviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewReason",
                table: "AppUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewStatus",
                table: "AppUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewReason",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "ReviewStatus",
                table: "AppUsers");
        }
    }
}
