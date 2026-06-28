# Components Diagram

```mermaid
graph TD
    subgraph Host["Pricing.Api (host)"]
        API[Pricing.Api]
    end

    subgraph Shared["Shared"]
        SD[Shared.Domain\nResult · Entity · AggregateRoot\nIDomainEvent · IHasDomainEvents]
        SA[Shared.Application\nIUnitOfWork · IDomainEventDispatcher]
        SC[Shared.Contracts\npaginacja · wspólne enumy]
    end

    subgraph ImportModule["Module: Import"]
        IF[Import.Facade\nIImportFacade]
        ID[Import.Domain]
        IA[Import.Application\nimpl IImportFacade]
        II[Import.Infrastructure\nDbContext · schema=import]
        IAPI[Import.Api\nendpoints · AddImportModule]
        IC[Import.Contracts\nrequest/response DTOs]
    end

    subgraph InventoryModule["Module: Inventory"]
        InvF[Inventory.Facade\nIInventoryFacade]
        InvD[Inventory.Domain]
        InvA[Inventory.Application\nimpl IInventoryFacade]
        InvI[Inventory.Infrastructure\nDbContext · schema=inventory]
        InvAPI[Inventory.Api\nendpoints · AddInventoryModule]
        InvC[Inventory.Contracts\nrequest/response DTOs]
    end

    subgraph RatingModule["Module: Rating"]
        RF[Rating.Facade\nIRatingFacade]
        RD[Rating.Domain]
        RA[Rating.Application\nimpl IRatingFacade]
        RI[Rating.Infrastructure\nDbContext · schema=rating]
        RAPI[Rating.Api\nendpoints · AddRatingModule]
        RC[Rating.Contracts\nrequest/response DTOs]
    end

    subgraph Infra["Infrastructure"]
        PG[(PostgreSQL\nschemas: import · inventory · rating · hangfire)]
        HF[Hangfire\nschema=hangfire]
    end

    %% Host wires modules
    API --> IAPI
    API --> InvAPI
    API --> RAPI

    %% Module internal layers
    IAPI --> II & IC
    II --> IA --> ID
    ID --> SD
    IA --> SA & SC

    InvAPI --> InvI & InvC
    InvI --> InvA --> InvD
    InvD --> SD
    InvA --> SA & SC

    RAPI --> RI & RC
    RI --> RA --> RD
    RD --> SD
    RA --> SA & SC

    %% Facade references (cross-module — consumer sees only Facade, not Application)
    IA -- "IInventoryFacade\n(cross-module)" --> InvF
    IA -- "IRatingFacade\n(cross-module)" --> RF
    RA -- "IInventoryFacade\n(cross-module)" --> InvF

    InvA -. "implements" .-> InvF
    RA -. "implements" .-> RF
    IA -. "implements" .-> IF

    %% Hangfire async triggers
    IA -- "Enqueue InventoryUpdateJob" --> HF
    InvA -- "Enqueue PriceRecalculationJob" --> HF

    %% DB
    II & InvI & RI --> PG
    HF --> PG
```

## Granice modułów

Kompilator wymusza granice — moduł-konsument referencuje wyłącznie `*.Facade`, nigdy `*.Application` ani `*.Domain` innego modułu.

| Komunikacja | Mechanizm |
|-------------|-----------|
| In-process sync (side effecty UoW) | `IDomainEventDispatcher` |
| Cross-module async (durable, retry) | Hangfire `IBackgroundJobClient.Enqueue` |
| Cross-module sync (query) | `IXxxFacade` — implementacja w `*.Application` |

## Główny przepływ ingestii

```
POST /import/{contextId}/run
  → Import.Application → UoW.SaveChanges
    → DomainEventDispatcher → IngestionCompletedHandler
      → IInventoryFacade.ProcessImportedDataAsync
        → Hangfire.Enqueue<InventoryUpdateJob>
          → Inventory: SKU→EAN · aktualizacja cen bazowych
            → IRatingFacade.TriggerPriceRecalculationAsync
              → Hangfire.Enqueue<PriceRecalculationJob>
                → Rating: pre-kalkulacja cennika
```
