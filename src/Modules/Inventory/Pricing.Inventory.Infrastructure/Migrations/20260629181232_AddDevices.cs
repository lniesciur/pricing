using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pricing.Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDevices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Devices",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EanCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TypeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SubtypeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ManufacturerCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Devices_EanCode",
                schema: "inventory",
                table: "Devices",
                column: "EanCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Devices",
                schema: "inventory");
        }
    }
}
