# Pricing

Modularny monolit do ingestii danych partnerskich, zarządzania katalogiem urządzeń i generowania cen. .NET 10, FastEndpoints, SQL Server.

[![CI](https://github.com/lniesciur/pricing/actions/workflows/ci.yml/badge.svg)](https://github.com/lniesciur/pricing/actions/workflows/ci.yml)

## Moduły

| Moduł | Odpowiedzialność |
|-------|-----------------|
| **Import** | Ingestion plików CSV/XLSX od partnerów, parsowanie, statusy importu |
| **Inventory** | Kanoniczny katalog urządzeń (EAN), magazyn wirtualny, mapa SKU→EAN per partner |
| **Rating** | Algorytm cenowy per `type/subtype/range`, pre-kalkulowany cennik i wycena na żądanie |

## Stack

- **.NET 10** — runtime
- **FastEndpoints** — HTTP API + walidacja (FluentValidation)
- **SQL Server** — schematy per moduł (`import`, `inventory`, `rating`)
- **EF Core** — ORM, migracje per moduł
- **Hangfire** — background jobs i komunikacja async między modułami
- **xUnit + NSubstitute** — testy jednostkowe
- **Testcontainers** — testy integracyjne (SQL Server w kontenerze)

## Uruchomienie

**Wymagania:** .NET 10 SDK, SQL Server (lub Docker)

```bash
# Ustaw connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Server=localhost;Database=Pricing;User Id=sa;Password=...;TrustServerCertificate=True" \
  --project src/Pricing.Api

# Migracje (per moduł)
dotnet ef database update --project src/Modules/Inventory/Pricing.Inventory.Infrastructure --startup-project src/Pricing.Api
dotnet ef database update --project src/Modules/Import/Pricing.Import.Infrastructure --startup-project src/Pricing.Api
dotnet ef database update --project src/Modules/Rating/Pricing.Rating.Infrastructure --startup-project src/Pricing.Api

# Start
dotnet run --project src/Pricing.Api
```

API dostępne pod `http://localhost:5000/api`, Swagger UI pod `http://localhost:5000/swagger`.

## Testy

```bash
# Wszystkie
dotnet test Pricing.slnx

# Konkretny test
dotnet test --filter "FullyQualifiedName~NazwaTestu"
```

Testy integracyjne (`Pricing.IntegrationTests`) wymagają Dockera — Testcontainers uruchamia SQL Server automatycznie.

## Struktura projektu

```
src/
  Shared/
    Pricing.Shared.Domain       ← Result, Entity, AggregateRoot, IDomainEvent
    Pricing.Shared.Application  ← IUnitOfWork, IDomainEventDispatcher
    Pricing.Shared.Contracts    ← typy cross-module (paginacja, wspólne enumy)
  Modules/
    Import/                     ← Facade | Domain | Contracts | Application | Infrastructure | Api
    Inventory/
    Rating/
  Pricing.Api/                  ← host: AddXxxModule() + UseFastEndpoints()
tests/
  Pricing.{Import,Inventory,Rating}.Domain.UnitTests
  Pricing.{Import,Inventory,Rating}.Application.UnitTests
  Pricing.IntegrationTests
  Pricing.ArchitectureTests
```

Moduły komunikują się wyłącznie przez `*.Facade` — granice wymuszone przez referencje projektów.

## Dodanie migracji

```bash
dotnet ef migrations add <NazwaMigracji> \
  --project src/Modules/<Modul>/Pricing.<Modul>.Infrastructure \
  --startup-project src/Pricing.Api
```

## Dokumentacja

- [`_docs/adr/ADR-001-modular-monolith.md`](_docs/adr/ADR-001-modular-monolith.md) — decyzje architektoniczne
- [`CLAUDE.md`](CLAUDE.md) — wzorce implementacji dla Claude Code
