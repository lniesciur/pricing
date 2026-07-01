using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pricing.Import.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitImportSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "import");

            migrationBuilder.CreateTable(
                name: "ImportJobs",
                schema: "import",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ImportType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FileContent = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    Added = table.Column<int>(type: "int", nullable: false),
                    Skipped = table.Column<int>(type: "int", nullable: false),
                    Updated = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImportJobErrors",
                schema: "import",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RowNumber = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ErrorType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ImportJobId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportJobErrors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportJobErrors_ImportJobs_ImportJobId",
                        column: x => x.ImportJobId,
                        principalSchema: "import",
                        principalTable: "ImportJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImportJobErrors_ImportJobId",
                schema: "import",
                table: "ImportJobErrors",
                column: "ImportJobId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportJobErrors",
                schema: "import");

            migrationBuilder.DropTable(
                name: "ImportJobs",
                schema: "import");
        }
    }
}
