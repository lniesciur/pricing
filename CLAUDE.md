# Pricing

Modularny monolit — .NET 10, FastEndpoints, SQL Server.

## Commands

```bash
dotnet build Pricing.slnx
dotnet run --project src/Pricing.Api
dotnet test Pricing.slnx
dotnet test --filter "FullyQualifiedName~TestMethodName"

# Migracje — per moduł, zawsze z --startup-project Pricing.Api
dotnet ef migrations add <MigrationName> --project src/Modules/Inventory/Pricing.Inventory.Infrastructure --startup-project src/Pricing.Api
dotnet ef database update --project src/Modules/Inventory/Pricing.Inventory.Infrastructure --startup-project src/Pricing.Api
dotnet ef migrations remove --project src/Modules/Inventory/Pricing.Inventory.Infrastructure --startup-project src/Pricing.Api
```

## Architektura

Modularny monolit. 3 moduły domenowe, twarde granice wymuszone przez kompilator (referencje projektów).

```
Shared.Domain ← Domain ← Application ← Infrastructure ← *.Api (module)
Shared.Application ↗        ↑    ↑         ↑                  ↑
Shared.Contracts ↗    Facade  Contracts  Facade (impl)   AddXxxModule()
                               ↑                              ↑
                          *.Api (module)              Pricing.Api (host)
```

### Struktura katalogów

```
src/
  Shared/
    Pricing.Shared.Domain       ← Result, Entity, AggregateRoot, IDomainEvent, IHasDomainEvents
    Pricing.Shared.Application  ← IUnitOfWork, IDomainEventDispatcher
    Pricing.Shared.Contracts    ← typy domenowe współdzielone (Response, Dto) — brak HTTP-specifics
  Modules/
    Inventory/                  ← katalog urządzeń, magazyn wirtualny, mapa SKU→EAN per partner
      Pricing.Inventory.Facade
      Pricing.Inventory.Domain
      Pricing.Inventory.Contracts ← request + response DTOs (brak zależności)
      Pricing.Inventory.Application
      Pricing.Inventory.Infrastructure
      Pricing.Inventory.Api     ← endpointy + validators + AddInventoryModule()
    Import/                     ← pipeline ingestii plików CSV/XLSX
      ...
      Pricing.Import.Contracts
      Pricing.Import.Api
    Rating/                     ← algorytm cenowy, pre-kalkulowany cennik + wycena na żądanie
      ...
      Pricing.Rating.Contracts
      Pricing.Rating.Api
  Pricing.Api/                  ← cienki host: AddXxxModule() + UseFastEndpoints()
tests/
  Pricing.{Inventory,Import,Rating}.Domain.UnitTests
  Pricing.{Inventory,Import,Rating}.Application.UnitTests
  Pricing.IntegrationTests
```

### Projekty per moduł

Każdy moduł ma 5 warstw: `*.Facade`, `*.Domain`, `*.Application`, `*.Infrastructure`, `*.Api`

| Warstwa | Rola |
|---------|------|
| `*.Facade` | Interfejs + DTOs do komunikacji cross-modułowej. Zależy od Shared.Domain. |
| `*.Domain` | Agregaty, value objects, domain events, interfejsy repozytoriów. Zależy od Shared.Domain. |
| `*.Contracts` | Request + response DTOs modułu. Zero zależności (czyste rekordy). |
| `*.Application` | Use casy, DI registration. Zależy od Domain + Facade + Contracts + Shared.{Domain,Application,Contracts}. |
| `*.Infrastructure` | EF Core DbContext per moduł (schema = lowercase moduł, tabele = PascalCase), repozytoria, UnitOfWork. Rejestruje Application. |
| `*.Api` | FastEndpoints endpoints + validators + `AddXxxModule()` installer. Zależy od Contracts + Infrastructure. |

### Komunikacja między modułami

- Moduły referencują się wyłącznie przez `*.Facade` — nigdy przez `*.Application` ani `*.Domain`
- Cross-module async: Hangfire (`hangfire` schema w DB)
- In-process domain events: `IDomainEventDispatcher` (domyślnie `NullDomainEventDispatcher`)

## Implementacja nowej funkcji w module

Wzorzec na przykładzie Inventory: `src/Modules/Inventory/Pricing.Inventory.*/DeviceTypes/`

**1. Domain** (`Pricing.{Module}.Domain/<Feature>/`)
- `<Entity>.cs` — dziedziczy `AggregateRoot<TId>`, fabryka `Create(...)`, bez publicznych setterów
- `<EntityId>.cs` — `public record <EntityId>(Guid Value) { public static <EntityId> New() => new(Guid.NewGuid()); }`
- `<Entity>Created.cs` — implementuje `IDomainEvent`; wywoływane przez `RaiseDomainEvent(...)` w agregacie
- `I<Entity>Repository.cs` — tylko metody potrzebne use case'owi

**2. Application** (`Pricing.{Module}.Application/UseCases/<UseCase>/`)
- `<UseCase>UseCase.cs` — zwraca `Result<XxxResponse>` z Shared.Contracts, wstrzykuje `I{Module}UnitOfWork`, wywołuje `SaveChangesAsync` na końcu
- Factory methods: `Result<T>.Ok(value)` / `Result<T>.Fail("error")`, `Result.Ok()` / `Result.Fail(...)` dla void

**3. Infrastructure** (`Pricing.{Module}.Infrastructure/Persistence/`)
- `Repositories/<Entity>Repository.cs` — implementuje interfejs domenowy, używa `{Module}DbContext`, nie wywołuje `SaveChanges`
- `Configurations/<Entity>Configuration.cs` — EF Fluent API, konwersje typów dla strongly-typed ids; tabele = PascalCase (`builder.ToTable("EntityName")`), schema = lowercase moduł (`inventory`, `import`, `rating`)
- Po dodaniu encji: uruchom migrację (patrz Commands)
- **Dane słownikowe (reference/seed data)** → `migrationBuilder.InsertData()` w dedykowanej migracji, NIE seeder na starcie. Powód: dane wersjonowane ze schematem, brak overhead przy każdym uruchomieniu. `HasData()` nie działa z agregatami mającymi prywatny konstruktor.

**4. Contracts** (`src/Modules/{Module}/Pricing.{Module}.Contracts/<Feature>/`)
- `<UseCase>Request.cs` — namespace `Pricing.{Module}.Contracts.<Feature>`
- `<UseCase>Response.cs` — namespace `Pricing.{Module}.Contracts.<Feature>`
- Brak zależności projektowych — czyste rekordy
- `src/Shared/Pricing.Shared.Contracts/` — tylko typy naprawdę cross-module (paginacja, wspólne enumy)

**5. Api** (`src/Modules/{Module}/Pricing.{Module}.Api/Endpoints/<Feature>/`)
- `<UseCase>Endpoint.cs` — dziedziczy `Endpoint<TRequest, TResponse>`, deleguje do use case
- `<UseCase>RequestValidator.cs` — FluentValidation, co-located z endpointem
- Błędy domenowe → `409 Conflict` przez `AddError` + `Send.ErrorsAsync`
- Sukces → `201 Created` przez `Send.CreatedAtAsync`

## IUnitOfWork — wzorzec per moduł

Każdy moduł definiuje własny interfejs aby uniknąć konfliktu w DI:

```csharp
// Pricing.{Module}.Application
public interface I{Module}UnitOfWork : IUnitOfWork;

// Pricing.{Module}.Infrastructure
public class {Module}UnitOfWork(…) : I{Module}UnitOfWork { … }
```

Use case wstrzykuje `I{Module}UnitOfWork`, nie `IUnitOfWork` bezpośrednio.

## DI Registration

### Scrutor (auto by naming convention, w Application)
- `*UseCase` → scoped, registered as self
- `*Repository` → scoped, registered as implemented interfaces
- `*Facade` (implementacja) → scoped, registered as implemented interfaces

### Module installer (w *.Api)
```csharp
// Pricing.{Module}.Api/DependencyInjection.cs
public static IServiceCollection AddXxxModule(this IServiceCollection services, IConfiguration configuration)
{
    services.AddXxxInfrastructure(configuration);
    return services;
}
```

`Program.cs` wywołuje tylko `AddXxxModule()` — nie zna Infrastructure ani Application bezpośrednio.

## Domain Events

`AggregateRoot<TId>` implementuje `IHasDomainEvents`. `{Module}UnitOfWork.SaveChangesAsync` wyciąga eventy ze śledzonych agregatów i dispatcha przez `IDomainEventDispatcher` po `SaveChanges`.

Domyślnie: `NullDomainEventDispatcher` (no-op). Podmień na prawdziwą implementację w `{Module}.Infrastructure/DependencyInjection.cs`.

## Testing

- Per-warstwo unit testy per moduł: `tests/Pricing.{Module}.{Domain,Application}.UnitTests`
  - `*.Domain.UnitTests` referuje tylko `*.Domain`
  - `*.Application.UnitTests` referuje `*.Application` (i transytywnie `*.Domain`)
- Integracyjne: `tests/Pricing.IntegrationTests` — Testcontainers SQL Server (`mcr.microsoft.com/mssql/server:2022-latest`), `WebApplicationFactory<Program>`
- Mocking: **NSubstitute**
- Nazewnictwo testów: `MethodName_WhenCondition_ExpectedOutcome`
- Failure: `Assert.False(result.IsSuccess)` + `DidNotReceive`
- Success: `Assert.True(result.IsSuccess)` + verify output + `Received(1)`

## Specs

Specyfikacje funkcji w `_docs/adr/` (decyzje architektoniczne) i `_specs/active/` (feature specs). Przed implementacją przeczytaj odpowiedni spec.

## Known Gotchas

- Connection string musi zawierać `TrustServerCertificate=True` w środowiskach dev/CI (self-signed cert SQL Server)
- `ConnectionStrings__DefaultConnection` env var musi być ustawiony przed `dotnet ef database update` w CI
- Never commit credentials — używaj `dotnet user-secrets` lokalnie
- FastEndpoints skanuje wszystkie załadowane assembly automatycznie — wystarczy że `*.Api` jest referencją `Pricing.Api`
