using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pricing.Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameTablesToPascalCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_device_subtypes_device_types_TypeId",
                schema: "inventory",
                table: "device_subtypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_examples",
                schema: "inventory",
                table: "examples");

            migrationBuilder.DropPrimaryKey(
                name: "PK_device_types",
                schema: "inventory",
                table: "device_types");

            migrationBuilder.DropPrimaryKey(
                name: "PK_device_subtypes",
                schema: "inventory",
                table: "device_subtypes");

            migrationBuilder.RenameTable(
                name: "examples",
                schema: "inventory",
                newName: "Examples",
                newSchema: "inventory");

            migrationBuilder.RenameTable(
                name: "device_types",
                schema: "inventory",
                newName: "DeviceTypes",
                newSchema: "inventory");

            migrationBuilder.RenameTable(
                name: "device_subtypes",
                schema: "inventory",
                newName: "DeviceSubtypes",
                newSchema: "inventory");

            migrationBuilder.RenameIndex(
                name: "IX_device_types_Code",
                schema: "inventory",
                table: "DeviceTypes",
                newName: "IX_DeviceTypes_Code");

            migrationBuilder.RenameIndex(
                name: "IX_device_subtypes_TypeId",
                schema: "inventory",
                table: "DeviceSubtypes",
                newName: "IX_DeviceSubtypes_TypeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Examples",
                schema: "inventory",
                table: "Examples",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeviceTypes",
                schema: "inventory",
                table: "DeviceTypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DeviceSubtypes",
                schema: "inventory",
                table: "DeviceSubtypes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeviceSubtypes_DeviceTypes_TypeId",
                schema: "inventory",
                table: "DeviceSubtypes",
                column: "TypeId",
                principalSchema: "inventory",
                principalTable: "DeviceTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeviceSubtypes_DeviceTypes_TypeId",
                schema: "inventory",
                table: "DeviceSubtypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Examples",
                schema: "inventory",
                table: "Examples");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeviceTypes",
                schema: "inventory",
                table: "DeviceTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DeviceSubtypes",
                schema: "inventory",
                table: "DeviceSubtypes");

            migrationBuilder.RenameTable(
                name: "Examples",
                schema: "inventory",
                newName: "examples",
                newSchema: "inventory");

            migrationBuilder.RenameTable(
                name: "DeviceTypes",
                schema: "inventory",
                newName: "device_types",
                newSchema: "inventory");

            migrationBuilder.RenameTable(
                name: "DeviceSubtypes",
                schema: "inventory",
                newName: "device_subtypes",
                newSchema: "inventory");

            migrationBuilder.RenameIndex(
                name: "IX_DeviceTypes_Code",
                schema: "inventory",
                table: "device_types",
                newName: "IX_device_types_Code");

            migrationBuilder.RenameIndex(
                name: "IX_DeviceSubtypes_TypeId",
                schema: "inventory",
                table: "device_subtypes",
                newName: "IX_device_subtypes_TypeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_examples",
                schema: "inventory",
                table: "examples",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_device_types",
                schema: "inventory",
                table: "device_types",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_device_subtypes",
                schema: "inventory",
                table: "device_subtypes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_device_subtypes_device_types_TypeId",
                schema: "inventory",
                table: "device_subtypes",
                column: "TypeId",
                principalSchema: "inventory",
                principalTable: "device_types",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
