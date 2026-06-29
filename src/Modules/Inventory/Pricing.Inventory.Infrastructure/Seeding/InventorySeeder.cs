using Microsoft.EntityFrameworkCore;
using Pricing.Inventory.Domain.DeviceTypes;
using Pricing.Inventory.Infrastructure.Persistence;

namespace Pricing.Inventory.Infrastructure.Seeding;

public class InventorySeeder(InventoryDbContext context)
{
    public async Task SeedAsync(CancellationToken ct = default)
    {
        await SeedDeviceTypesAsync(ct);
    }

    private async Task SeedDeviceTypesAsync(CancellationToken ct)
    {
        var seedData = GetSeedData();

        foreach (var (typeCode, typeName, subtypes) in seedData)
        {
            var exists = await context.DeviceTypes.AnyAsync(t => t.Code == typeCode, ct);
            if (exists)
                continue;

            var deviceType = DeviceType.Create(typeCode, typeName);
            foreach (var (subtypeCode, subtypeName) in subtypes)
                deviceType.AddSubtype(subtypeCode, subtypeName);

            await context.DeviceTypes.AddAsync(deviceType, ct);
        }

        await context.SaveChangesAsync(ct);
    }

    private static IEnumerable<(string Code, string Name, (string Code, string Name)[] Subtypes)> GetSeedData() =>
    [
        ("PHONE", "Smartfon",
        [
            ("BASIC", "Podstawowy"), ("MID", "Średnia półka"), ("PREMIUM", "Premium"),
            ("FOLDABLE", "Składany"), ("RUGGED", "Wzmocniony"), ("5G", "5G"),
        ]),
        ("TABLET", "Tablet",
        [
            ("WIFI", "Wi-Fi"), ("LTE", "LTE"), ("5G", "5G"),
            ("KIDS", "Dla dzieci"), ("DRAWING", "Graficzny"),
        ]),
        ("LAPTOP", "Laptop",
        [
            ("ULTRABOOK", "Ultrabook"), ("GAMING", "Gamingowy"), ("BUSINESS", "Biznesowy"),
            ("2IN1", "2 w 1"), ("CHROMEBOOK", "Chromebook"),
        ]),
        ("SMARTWATCH", "Smartwatch",
        [
            ("SPORT", "Sportowy"), ("CLASSIC", "Klasyczny"),
            ("KIDS", "Dla dzieci"), ("MEDICAL", "Medyczny"),
        ]),
        ("ACCESSORY", "Akcesorium",
        [
            ("CASE", "Etui"), ("CHARGER", "Ładowarka"), ("HEADPHONES", "Słuchawki"),
            ("POWERBANK", "Powerbank"), ("CABLE", "Kabel"), ("SCREEN_PROTECTOR", "Szkło ochronne"),
            ("KEYBOARD", "Klawiatura"), ("MOUSE", "Mysz"),
        ]),
        ("MONITOR", "Monitor",
        [
            ("OFFICE", "Biurowy"), ("GAMING", "Gamingowy"),
            ("GRAPHIC", "Graficzny"), ("PORTABLE", "Przenośny"),
        ]),
        ("TV", "Telewizor",
        [
            ("LED", "LED"), ("OLED", "OLED"), ("QLED", "QLED"), ("SMART", "Smart TV"),
        ]),
        ("CONSOLE", "Konsola",
        [
            ("STATIONARY", "Stacjonarna"), ("HANDHELD", "Przenośna"), ("RETRO", "Retro"),
        ]),
        ("ROUTER", "Router / Modem",
        [
            ("HOME", "Domowy"), ("MESH", "Mesh"), ("5G", "5G"), ("BUSINESS", "Biznesowy"),
        ]),
        ("CAMERA", "Aparat / Kamera",
        [
            ("COMPACT", "Kompaktowy"), ("MIRRORLESS", "Bezlusterkowy"),
            ("DSLR", "DSLR"), ("ACTION", "Sportowa"), ("DRONE", "Dron"),
        ]),
        ("E_READER", "Czytnik e-booków",
        [
            ("BASIC", "Podstawowy"), ("BACKLIT", "Z podświetleniem"), ("WATERPROOF", "Wodoodporny"),
        ]),
        ("SMART_SPEAKER", "Głośnik inteligentny",
        [
            ("MINI", "Mini"), ("STANDARD", "Standardowy"), ("DISPLAY", "Z ekranem"),
        ]),
    ];
}
