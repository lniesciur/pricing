# ADR-001: Architektura Systemu — Modularny Monolit

**Data:** 2026-06-28  
**Status:** Zaakceptowane

---

## Kontekst

System Pricing ma za zadanie pobierać dane od partnerów, generować ceny algorytmicznie i serwować je przez REST API. Dotychczasowe rozwiązanie (legacy) oparte było na 3 mikroserwisach.

Warunki projektu:
- Jeden zespół deweloperski
- Ciągłe wdrożenia (praktycznie bezprzerywowe)
- Algorytm cenowy jest ściśle powiązany z logiką cennika
- Istniejąca instancja PostgreSQL

---

## Decyzje

### 1. Modularny monolit zamiast mikroserwisów

Jeden deployment, twarde granice między modułami wymuszone strukturą projektów.

**Uzasadnienie:**
- Jeden zespół — brak potrzeby niezależnych wdrożeń między serwisami
- Jeden pipeline CI/CD zamiast trzech
- Współdzielone modele domenowe (np. `Device` w Catalog i Rating) — w mikroserwisach wymuszałyby duplikację lub złożone kontrakty synchronizacyjne
- Algorytm i pricing to ta sama domena — podział w legacy generował zbędną komunikację sieciową

**Droga do mikroserwisów pozostaje otwarta** — wzorzec fasady (pkt. 5) umożliwia ekstrakcję modułu bez przepisywania logiki biznesowej.

---

### 2. Trzy moduły domenowe

| Moduł | Odpowiedzialność | Schemat DB |
|-------|-----------------|-----------|
| **Import** | Upload plików od partnerów, parsowanie formatów (CSV/XLSX), statusy importu, pobieranie pliku, lokalizacja błędów w wierszu | `import` |
| **Inventory** | Kanoniczny katalog urządzeń (EAN + klasyfikacja `type/subtype/range`), magazyn wirtualny (stany pomijamy), mapa SKU→EAN per partner/kontekst, metody pobierania stanów od partnerów | `inventory` |
| **Rating** | Reguły algorytmu cenowego per `type/subtype/range`, generowanie cen końcowych per urządzenie × kontekst × okres wynajmu — zarówno pre-kalkulowany cennik jak i wycena na żądanie | `rating` |

#### Dlaczego Inventory, nie Catalog?

Moduł obejmuje więcej niż sam katalog urządzeń — odpowiada również za magazyn wirtualny i pobieranie stanów magazynowych od partnerów. Nazwa Inventory lepiej oddaje pełną odpowiedzialność modułu.

#### Dlaczego mapa SKU→EAN jest w Inventory, nie w osobnym module?

Mapa partnera (SKU→EAN) to wiedza o tym jak partner identyfikuje urządzenie, które my znamy po EAN. Inventory jest właścicielem kanonicznego katalogu urządzeń — zna EAN, zna klasyfikację. Naturalne jest, że Inventory wie też jakie aliasy SKU używa dany partner dla danego urządzenia w danym kontekście.

Cykl życia mapy jest powiązany z cyklem życia katalogu: dodanie nowego urządzenia często pociąga za sobą dodanie mapowania SKU. Utrzymywanie osobnego modułu Partners oznaczałoby rozdzielenie tej wiedzy bez wyraźnej korzyści domenowej.

#### Dlaczego Rating, nie Engine?

"Engine" to nazwa mechanizmu, nie domeny. Moduł ten wyznacza **stawki** po których urządzenia są wynajmowane lub sprzedawane — co w branży leasingu i wynajmu odpowiada pojęciu *ratingu* (wyznaczanie stawki dla produktu).

Moduł obsługuje dwa przypadki jednocześnie:
- **Pre-kalkulowany cennik** — batch generowanie cen dla wszystkich urządzeń × kontekstów × okresów wynajmu, wynik przechowywany w DB
- **Wycena na żądanie** — obliczenie ceny dla konkretnego zapytania w czasie rzeczywistym

Oba przypadki opierają się na tych samych regułach biznesowych (algorytm per `type/subtype/range`), dlatego należą do jednego modułu. Nazwa "Rating" pokrywa oba scenariusze. Nazwa "Pricing" była najdokładniejsza domenowo, ale koliduje z nazwą solution.

---

### 3. Struktura projektów — separacja per moduł per warstwa

```
src/
  Shared/
    Pricing.Shared.Domain           ← Result, Entity, AggregateRoot, IDomainEvent, IHasDomainEvents
    Pricing.Shared.Application      ← IUnitOfWork, IDomainEventDispatcher
    Pricing.Shared.Contracts        ← typy naprawdę cross-module (paginacja, wspólne enumy)

  Modules/
    Import/
      Pricing.Import.Facade         ← IImportFacade + cross-module DTOs
      Pricing.Import.Domain
      Pricing.Import.Contracts      ← request + response DTOs modułu (zero zależności)
      Pricing.Import.Application    ← implementuje IImportFacade
      Pricing.Import.Infrastructure
      Pricing.Import.Api            ← endpointy + validators + AddImportModule()

    Inventory/
      Pricing.Inventory.Facade      ← IInventoryFacade + cross-module DTOs
      Pricing.Inventory.Domain
      Pricing.Inventory.Contracts   ← request + response DTOs modułu (zero zależności)
      Pricing.Inventory.Application ← implementuje IInventoryFacade
      Pricing.Inventory.Infrastructure
      Pricing.Inventory.Api         ← endpointy + validators + AddInventoryModule()

    Rating/
      Pricing.Rating.Facade         ← IRatingFacade + cross-module DTOs
      Pricing.Rating.Domain
      Pricing.Rating.Contracts      ← request + response DTOs modułu (zero zależności)
      Pricing.Rating.Application    ← implementuje IRatingFacade
      Pricing.Rating.Infrastructure
      Pricing.Rating.Api            ← endpointy + validators + AddRatingModule()

  Pricing.Api/                      ← cienki host: referencje do *.Api + UseFastEndpoints()

tests/
  Pricing.{Inventory,Import,Rating}.Domain.UnitTests
  Pricing.{Inventory,Import,Rating}.Application.UnitTests
  Pricing.IntegrationTests          ← end-to-end przez HTTP, Testcontainers PostgreSQL
```

#### Dlaczego Shared.Domain + Shared.Application, nie SharedKernel?

SharedKernel sugerował jeden projekt bez wyraźnych granic. Podział na `Shared.Domain` (typy domenowe) i `Shared.Application` (interfejsy aplikacyjne jak `IUnitOfWork`) odpowiada Clean Architecture i pozwala na precyzyjne referencje — Domain nie musi widzieć `IUnitOfWork`.

#### Dlaczego per-module Contracts, nie jeden Contracts?

Każdy moduł posiada własny `*.Contracts` z request i response DTOs. Dzięki temu moduł jest samodzielną jednostką — przy ekstrakcji do mikroserwisu `*.Contracts` staje się pakietem NuGet z kontraktem bez żadnych zmian wewnątrz modułu. `Pricing.Shared.Contracts` pozostaje wyłącznie dla typów naprawdę cross-module.

#### Dlaczego *.Api per moduł, nie endpointy w Pricing.Api?

Każdy moduł posiada własną warstwę `*.Api` z endpointami, validatorami i metodą `AddXxxModule()`. `Pricing.Api` (host) jest ślepy na szczegóły modułów — wywołuje tylko instalatory. FastEndpoints automatycznie skanuje załadowane assembly, więc endpointy są wykrywane bez dodatkowej konfiguracji. Ekstrakcja modułu do mikroserwisu = nowy `Program.cs` z jedną referencją do `Pricing.{Module}.Api`.

Kompilator wymusza granice — moduł bez jawnej referencji projektowej nie może użyć typów innego modułu.

---

### 4. Komunikacja między modułami

Dwie warstwy o różnych odpowiedzialnościach:

**a) Custom Domain Event Dispatcher (in-process, synchroniczny)**

Własna implementacja `IDomainEventDispatcher` (~15 linii). Odpowiada za side effecty wewnątrz modułu w ramach transakcji `UnitOfWork`. Interfejs `IDomainEventHandler<TEvent>` rejestrowany przez Scrutor.

Odrzucone: MediatR (płatny w nowej wersji).

**b) Hangfire (async, durable, retry)**

Cross-module integration i background processing. Storage: PostgreSQL (`hangfire` schema).

Pokrywa trzy przypadki użycia jednym mechanizmem:

| Przypadek | Mechanizm |
|-----------|-----------|
| Cross-module trigger (Import → Catalog → Rating) | `IBackgroundJobClient.Enqueue` |
| Scheduled ingestion per kontekst | `RecurringJob.AddOrUpdate($"ingestion-{contextId}", cron)` |
| Manualny trigger przez API | `IBackgroundJobClient.Enqueue` z endpointu |

Odrzucone: Azure Service Bus (zbędna złożoność i koszt).

**Przykładowy przepływ — zakończenie ingestii:**

```
POST /import/{contextId}/run
  → ImportUseCase → UoW.SaveChanges()
  → DomainEventDispatcher → IngestionCompletedHandler
    → IInventoryFacade.ProcessImportedDataAsync(contextId)
      → Hangfire.Enqueue<InventoryUpdateJob>(contextId)
        → Inventory: tłumaczy SKU→EAN, aktualizuje ceny bazowe
          → IRatingFacade.TriggerPriceRecalculationAsync(contextId)
            → Hangfire.Enqueue<PriceRecalculationJob>(contextId)
```

---

### 5. Wzorzec Fasady dla komunikacji między modułami

Moduły nie referencują się bezpośrednio. Każdy moduł eksponuje fasadę:

- `*.Facade` projekt definiuje **wyłącznie interfejs + DTOs** — zero zależności do `*.Application`
- `*.Application` implementuje interfejs fasady
- Moduł-konsument referencuje tylko `*.Facade`, nie widzi implementacji

```csharp
// Pricing.Inventory.Facade
public interface IInventoryFacade
{
    Task<IReadOnlyList<DeviceWithPartnerPriceDto>> GetDevicesWithPartnerPricesAsync(
        Guid contextId, CancellationToken ct);
}

// Pricing.Inventory.Application
public class InventoryFacade(IDeviceRepository repo) : IInventoryFacade { ... }

// Pricing.Rating.Application — nie wie o Inventory.Application
public class PriceRecalculationJob(IInventoryFacade inventory) { ... }
```

**Korzyść długoterminowa:** ekstrakcja modułu do mikroserwisu = nowa implementacja `IInventoryFacade` jako HTTP client. `Rating.Application` bez zmian.

---

### 6. Baza danych

- **PostgreSQL** — instancja przez Supabase connection string
- EF Core z pakietem `Npgsql.EntityFrameworkCore.PostgreSQL`
- Separacja przez schematy: `import`, `inventory`, `rating`, `hangfire`
- Każdy moduł ma własny `DbContext` mapujący tylko swój schemat

**Pliki partnerów (CSV/XLSX):** `BYTEA` w tabeli `import.partner_files` — eliminuje zależność od zewnętrznego object storage.

Odrzucone: Azure Blob Storage (nie darmowe po 12 mies.), Cloudflare R2 (dodatkowa zależność infrastrukturalna nieproporcjonalna do rozmiaru plików).

---

### 7. Algorytm cenowy — podejście do zmian

**MVP:** `cena_końcowa = cena_bazowa × mnożnik + marża`

Parametry (`mnożnik`, `marża`) per `type/subtype/range` przechowywane w DB — zmiany parametrów nie wymagają deploymentu.

Implementacja: Strategy pattern (`IPricingStrategy`). Dodanie nowego wariantu algorytmu = nowa klasa + deployment (jeden pipeline, koszt pomijalny).

Odrzucone: Plugin assemblies (overkill przy jednym zespole), Expression engine w DB (złożoność nieproporcjonalna do MVP).

---

### 8. Mapa partnera (SKU → EAN)

- Jeden EAN może mapować się na wiele SKU (relacja 1:N)
- Mapa jest **per kontekst** (MediaMarktES i MediaMarktDE mają oddzielne mapy)
- Dwa tryby budowania mapy: plik (ręczny proces biznesowy) oraz auto-build z API partnera
- Auto-build **uzupełnia** istniejącą mapę (merge), nie zastępuje
- SKU bez mapowania → kolejka `inventory.unmapped_skus` + raport + email

---

## Konsekwencje

**Plusy:**
- Jeden pipeline CI/CD, prosty deployment
- Kompilator wymusza granice modułów — niemożliwe przypadkowe zależności
- Fasady umożliwiają przyszłą ekstrakcję do mikroserwisów bez zmiany logiki
- Hangfire pokrywa retry, scheduled ingestion i manualne triggery jednym mechanizmem
- Brak zewnętrznych serwisów na MVP (storage w PostgreSQL, brak message brokera)
- Mapa SKU→EAN w Inventory — spójna z wiedzą o urządzeniach, bez zbędnego modułu

**Ryzyka:**
- ~27 projektów w solution — wyższy koszt nawigacji i utrzymania struktury
- Tranzytywne referencje projektów w .NET wymagają dyscypliny — moduł może "zobaczyć" typy których nie powinien używać; pilnowane przez code review
- Hangfire wymaga monitoringu nieudanych jobów — bez alertów błędy ingestii mogą pozostać niezauważone
- Przepływ Import → Inventory → Rating to łańcuch async jobów — debugowanie wymaga śledzenia przez Hangfire dashboard
