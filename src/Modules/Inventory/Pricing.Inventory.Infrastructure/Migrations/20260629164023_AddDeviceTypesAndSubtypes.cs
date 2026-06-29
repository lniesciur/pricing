using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pricing.Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceTypesAndSubtypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "inventory");

            migrationBuilder.CreateTable(
                name: "device_types",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_device_types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "examples",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_examples", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "device_subtypes",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_device_subtypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_device_subtypes_device_types_TypeId",
                        column: x => x.TypeId,
                        principalSchema: "inventory",
                        principalTable: "device_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_device_subtypes_TypeId",
                schema: "inventory",
                table: "device_subtypes",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_device_types_Code",
                schema: "inventory",
                table: "device_types",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "device_subtypes",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "examples",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "device_types",
                schema: "inventory");
        }
    }
}
