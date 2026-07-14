using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pricing.Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Attributes",
                schema: "inventory",
                table: "Devices",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Attributes",
                schema: "inventory",
                table: "Devices");
        }
    }
}
