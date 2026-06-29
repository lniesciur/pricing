using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pricing.Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedManufacturerDictionary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "inventory", table: "Manufacturers",
                columns: ["Id", "Code", "Name"],
                values: new object[,]
                {
                    { Guid.Parse("c0000000-0000-0000-0000-000000000001"), "APPLE",     "Apple"     },
                    { Guid.Parse("c0000000-0000-0000-0000-000000000002"), "SAMSUNG",   "Samsung"   },
                    { Guid.Parse("c0000000-0000-0000-0000-000000000003"), "HUAWEI",    "Huawei"    },
                    { Guid.Parse("c0000000-0000-0000-0000-000000000004"), "XIAOMI",    "Xiaomi"    },
                    { Guid.Parse("c0000000-0000-0000-0000-000000000005"), "OPPO",      "OPPO"      },
                    { Guid.Parse("c0000000-0000-0000-0000-000000000006"), "VIVO",      "Vivo"      },
                    { Guid.Parse("c0000000-0000-0000-0000-000000000007"), "MOTOROLA",  "Motorola"  },
                    { Guid.Parse("c0000000-0000-0000-0000-000000000008"), "SONY",      "Sony"      },
                    { Guid.Parse("c0000000-0000-0000-0000-000000000009"), "LG",        "LG"        },
                    { Guid.Parse("c0000000-0000-0000-0000-000000000010"), "NOKIA",     "Nokia"     },
                    { Guid.Parse("c0000000-0000-0000-0000-000000000011"), "ONEPLUS",   "OnePlus"   },
                    { Guid.Parse("c0000000-0000-0000-0000-000000000012"), "GOOGLE",    "Google"    },
                    { Guid.Parse("c0000000-0000-0000-0000-000000000013"), "REALME",    "Realme"    },
                    { Guid.Parse("c0000000-0000-0000-0000-000000000014"), "HONOR",     "Honor"     },
                    { Guid.Parse("c0000000-0000-0000-0000-000000000015"), "ASUS",      "Asus"      },
                    { Guid.Parse("c0000000-0000-0000-0000-000000000016"), "LENOVO",    "Lenovo"    },
                    { Guid.Parse("c0000000-0000-0000-0000-000000000017"), "HP",        "HP"        },
                    { Guid.Parse("c0000000-0000-0000-0000-000000000018"), "DELL",      "Dell"      },
                    { Guid.Parse("c0000000-0000-0000-0000-000000000019"), "MICROSOFT", "Microsoft" },
                    { Guid.Parse("c0000000-0000-0000-0000-000000000020"), "PHILIPS",   "Philips"   },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "inventory", table: "Manufacturers",
                keyColumn: "Id",
                keyValues: Enumerable.Range(1, 20)
                    .Select(i => (object)Guid.Parse($"c0000000-0000-0000-0000-{i:D12}"))
                    .ToArray());
        }
    }
}
