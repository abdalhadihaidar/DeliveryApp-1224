using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryApp.Migrations
{
    /// <inheritdoc />
    public partial class test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First, update existing data to use integer values
            migrationBuilder.Sql(@"
                UPDATE PaymentMethods 
                SET Type = CASE 
                    WHEN Type = 'CreditCard' OR Type = 'بطاقة ائتمان' OR Type = 'فيزا' OR Type = 'ماستركارد' THEN '1'
                    WHEN Type = 'DebitCard' OR Type = 'بطاقة خصم' THEN '2'
                    WHEN Type = 'CashOnDelivery' OR Type = 'نقدي' OR Type = 'الدفع عند الاستلام' THEN '3'
                    WHEN Type = 'BankTransfer' OR Type = 'تحويل بنكي' THEN '4'
                    WHEN Type = 'DigitalWallet' OR Type = 'محفظة رقمية' THEN '5'
                    ELSE '1' -- Default to CreditCard
                END
            ");

            // Then alter the column type
            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "PaymentMethods",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<bool>(
                name: "AcceptsCOD",
                table: "DeliveryStatuses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "CashBalance",
                table: "DeliveryStatuses",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxCashLimit",
                table: "DeliveryStatuses",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "CODTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeliveryPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RestaurantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DriverPaidToRestaurant = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DriverCollectedFromCustomer = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DriverProfit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_CODTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CODTransactions_AbpUsers_DeliveryPersonId",
                        column: x => x.DeliveryPersonId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CODTransactions_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CODTransactions_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CODTransactions_DeliveryPersonId",
                table: "CODTransactions",
                column: "DeliveryPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_CODTransactions_OrderId",
                table: "CODTransactions",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_CODTransactions_RestaurantId",
                table: "CODTransactions",
                column: "RestaurantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CODTransactions");

            migrationBuilder.DropColumn(
                name: "AcceptsCOD",
                table: "DeliveryStatuses");

            migrationBuilder.DropColumn(
                name: "CashBalance",
                table: "DeliveryStatuses");

            migrationBuilder.DropColumn(
                name: "MaxCashLimit",
                table: "DeliveryStatuses");

            // First alter the column back to string
            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "PaymentMethods",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            // Then convert integer values back to string values
            migrationBuilder.Sql(@"
                UPDATE PaymentMethods 
                SET Type = CASE 
                    WHEN Type = '1' THEN 'CreditCard'
                    WHEN Type = '2' THEN 'DebitCard'
                    WHEN Type = '3' THEN 'CashOnDelivery'
                    WHEN Type = '4' THEN 'BankTransfer'
                    WHEN Type = '5' THEN 'DigitalWallet'
                    ELSE 'CreditCard' -- Default
                END
            ");
        }
    }
}
