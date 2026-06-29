using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pricing.Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDeviceTypeDictionaries : Migration
    {
        private static readonly Guid PhoneId        = Guid.Parse("d0000000-0000-0000-0000-000000000001");
        private static readonly Guid TabletId       = Guid.Parse("d0000000-0000-0000-0000-000000000002");
        private static readonly Guid LaptopId       = Guid.Parse("d0000000-0000-0000-0000-000000000003");
        private static readonly Guid SmartwatchId   = Guid.Parse("d0000000-0000-0000-0000-000000000004");
        private static readonly Guid AccessoryId    = Guid.Parse("d0000000-0000-0000-0000-000000000005");
        private static readonly Guid MonitorId      = Guid.Parse("d0000000-0000-0000-0000-000000000006");
        private static readonly Guid TvId           = Guid.Parse("d0000000-0000-0000-0000-000000000007");
        private static readonly Guid ConsoleId      = Guid.Parse("d0000000-0000-0000-0000-000000000008");
        private static readonly Guid RouterId       = Guid.Parse("d0000000-0000-0000-0000-000000000009");
        private static readonly Guid CameraId       = Guid.Parse("d0000000-0000-0000-0000-000000000010");
        private static readonly Guid EReaderId      = Guid.Parse("d0000000-0000-0000-0000-000000000011");
        private static readonly Guid SmartSpeakerId = Guid.Parse("d0000000-0000-0000-0000-000000000012");

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "inventory", table: "DeviceTypes",
                columns: ["Id", "Code", "Name"],
                values: new object[,]
                {
                    { PhoneId,        "PHONE",         "Smartfon"             },
                    { TabletId,       "TABLET",        "Tablet"               },
                    { LaptopId,       "LAPTOP",        "Laptop"               },
                    { SmartwatchId,   "SMARTWATCH",    "Smartwatch"           },
                    { AccessoryId,    "ACCESSORY",     "Akcesorium"           },
                    { MonitorId,      "MONITOR",       "Monitor"              },
                    { TvId,           "TV",            "Telewizor"            },
                    { ConsoleId,      "CONSOLE",       "Konsola"              },
                    { RouterId,       "ROUTER",        "Router / Modem"       },
                    { CameraId,       "CAMERA",        "Aparat / Kamera"      },
                    { EReaderId,      "E_READER",      "Czytnik e-booków"     },
                    { SmartSpeakerId, "SMART_SPEAKER", "Głośnik inteligentny" },
                });

            migrationBuilder.InsertData(
                schema: "inventory", table: "DeviceSubtypes",
                columns: ["Id", "Code", "Name", "TypeId"],
                values: new object[,]
                {
                    // PHONE
                    { Guid.Parse("e0000000-0000-0000-0001-000000000001"), "BASIC",    "Podstawowy",    PhoneId },
                    { Guid.Parse("e0000000-0000-0000-0001-000000000002"), "MID",      "Średnia półka", PhoneId },
                    { Guid.Parse("e0000000-0000-0000-0001-000000000003"), "PREMIUM",  "Premium",       PhoneId },
                    { Guid.Parse("e0000000-0000-0000-0001-000000000004"), "FOLDABLE", "Składany",      PhoneId },
                    { Guid.Parse("e0000000-0000-0000-0001-000000000005"), "RUGGED",   "Wzmocniony",    PhoneId },
                    { Guid.Parse("e0000000-0000-0000-0001-000000000006"), "5G",       "5G",            PhoneId },
                    // TABLET
                    { Guid.Parse("e0000000-0000-0000-0002-000000000001"), "WIFI",    "Wi-Fi",      TabletId },
                    { Guid.Parse("e0000000-0000-0000-0002-000000000002"), "LTE",     "LTE",        TabletId },
                    { Guid.Parse("e0000000-0000-0000-0002-000000000003"), "5G",      "5G",         TabletId },
                    { Guid.Parse("e0000000-0000-0000-0002-000000000004"), "KIDS",    "Dla dzieci", TabletId },
                    { Guid.Parse("e0000000-0000-0000-0002-000000000005"), "DRAWING", "Graficzny",  TabletId },
                    // LAPTOP
                    { Guid.Parse("e0000000-0000-0000-0003-000000000001"), "ULTRABOOK",  "Ultrabook",  LaptopId },
                    { Guid.Parse("e0000000-0000-0000-0003-000000000002"), "GAMING",     "Gamingowy",  LaptopId },
                    { Guid.Parse("e0000000-0000-0000-0003-000000000003"), "BUSINESS",   "Biznesowy",  LaptopId },
                    { Guid.Parse("e0000000-0000-0000-0003-000000000004"), "2IN1",       "2 w 1",      LaptopId },
                    { Guid.Parse("e0000000-0000-0000-0003-000000000005"), "CHROMEBOOK", "Chromebook", LaptopId },
                    // SMARTWATCH
                    { Guid.Parse("e0000000-0000-0000-0004-000000000001"), "SPORT",   "Sportowy",   SmartwatchId },
                    { Guid.Parse("e0000000-0000-0000-0004-000000000002"), "CLASSIC", "Klasyczny",  SmartwatchId },
                    { Guid.Parse("e0000000-0000-0000-0004-000000000003"), "KIDS",    "Dla dzieci", SmartwatchId },
                    { Guid.Parse("e0000000-0000-0000-0004-000000000004"), "MEDICAL", "Medyczny",   SmartwatchId },
                    // ACCESSORY
                    { Guid.Parse("e0000000-0000-0000-0005-000000000001"), "CASE",             "Etui",           AccessoryId },
                    { Guid.Parse("e0000000-0000-0000-0005-000000000002"), "CHARGER",          "Ładowarka",      AccessoryId },
                    { Guid.Parse("e0000000-0000-0000-0005-000000000003"), "HEADPHONES",       "Słuchawki",      AccessoryId },
                    { Guid.Parse("e0000000-0000-0000-0005-000000000004"), "POWERBANK",        "Powerbank",      AccessoryId },
                    { Guid.Parse("e0000000-0000-0000-0005-000000000005"), "CABLE",            "Kabel",          AccessoryId },
                    { Guid.Parse("e0000000-0000-0000-0005-000000000006"), "SCREEN_PROTECTOR", "Szkło ochronne", AccessoryId },
                    { Guid.Parse("e0000000-0000-0000-0005-000000000007"), "KEYBOARD",         "Klawiatura",     AccessoryId },
                    { Guid.Parse("e0000000-0000-0000-0005-000000000008"), "MOUSE",            "Mysz",           AccessoryId },
                    // MONITOR
                    { Guid.Parse("e0000000-0000-0000-0006-000000000001"), "OFFICE",   "Biurowy",   MonitorId },
                    { Guid.Parse("e0000000-0000-0000-0006-000000000002"), "GAMING",   "Gamingowy", MonitorId },
                    { Guid.Parse("e0000000-0000-0000-0006-000000000003"), "GRAPHIC",  "Graficzny", MonitorId },
                    { Guid.Parse("e0000000-0000-0000-0006-000000000004"), "PORTABLE", "Przenośny", MonitorId },
                    // TV
                    { Guid.Parse("e0000000-0000-0000-0007-000000000001"), "LED",   "LED",      TvId },
                    { Guid.Parse("e0000000-0000-0000-0007-000000000002"), "OLED",  "OLED",     TvId },
                    { Guid.Parse("e0000000-0000-0000-0007-000000000003"), "QLED",  "QLED",     TvId },
                    { Guid.Parse("e0000000-0000-0000-0007-000000000004"), "SMART", "Smart TV", TvId },
                    // CONSOLE
                    { Guid.Parse("e0000000-0000-0000-0008-000000000001"), "STATIONARY", "Stacjonarna", ConsoleId },
                    { Guid.Parse("e0000000-0000-0000-0008-000000000002"), "HANDHELD",   "Przenośna",   ConsoleId },
                    { Guid.Parse("e0000000-0000-0000-0008-000000000003"), "RETRO",      "Retro",       ConsoleId },
                    // ROUTER
                    { Guid.Parse("e0000000-0000-0000-0009-000000000001"), "HOME",     "Domowy",    RouterId },
                    { Guid.Parse("e0000000-0000-0000-0009-000000000002"), "MESH",     "Mesh",      RouterId },
                    { Guid.Parse("e0000000-0000-0000-0009-000000000003"), "5G",       "5G",        RouterId },
                    { Guid.Parse("e0000000-0000-0000-0009-000000000004"), "BUSINESS", "Biznesowy", RouterId },
                    // CAMERA
                    { Guid.Parse("e0000000-0000-0000-0010-000000000001"), "COMPACT",    "Kompaktowy",    CameraId },
                    { Guid.Parse("e0000000-0000-0000-0010-000000000002"), "MIRRORLESS", "Bezlusterkowy", CameraId },
                    { Guid.Parse("e0000000-0000-0000-0010-000000000003"), "DSLR",       "DSLR",          CameraId },
                    { Guid.Parse("e0000000-0000-0000-0010-000000000004"), "ACTION",     "Sportowa",      CameraId },
                    { Guid.Parse("e0000000-0000-0000-0010-000000000005"), "DRONE",      "Dron",          CameraId },
                    // E_READER
                    { Guid.Parse("e0000000-0000-0000-0011-000000000001"), "BASIC",      "Podstawowy",       EReaderId },
                    { Guid.Parse("e0000000-0000-0000-0011-000000000002"), "BACKLIT",    "Z podświetleniem", EReaderId },
                    { Guid.Parse("e0000000-0000-0000-0011-000000000003"), "WATERPROOF", "Wodoodporny",      EReaderId },
                    // SMART_SPEAKER
                    { Guid.Parse("e0000000-0000-0000-0012-000000000001"), "MINI",     "Mini",        SmartSpeakerId },
                    { Guid.Parse("e0000000-0000-0000-0012-000000000002"), "STANDARD", "Standardowy", SmartSpeakerId },
                    { Guid.Parse("e0000000-0000-0000-0012-000000000003"), "DISPLAY",  "Z ekranem",   SmartSpeakerId },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(schema: "inventory", table: "DeviceSubtypes",
                keyColumn: "TypeId",
                keyValues: [PhoneId, TabletId, LaptopId, SmartwatchId, AccessoryId,
                            MonitorId, TvId, ConsoleId, RouterId, CameraId, EReaderId, SmartSpeakerId]);

            migrationBuilder.DeleteData(schema: "inventory", table: "DeviceTypes",
                keyColumn: "Id",
                keyValues: [PhoneId, TabletId, LaptopId, SmartwatchId, AccessoryId,
                            MonitorId, TvId, ConsoleId, RouterId, CameraId, EReaderId, SmartSpeakerId]);
        }
    }
}
