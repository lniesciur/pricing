---
id: SPEC-005
title: Manufacturer Dictionary
module: Inventory
status: implemented
created: 2026-06-29
updated: 2026-06-29
branch: feature/SPEC-005-manufacturer-dictionary
related: [SPEC-004]
---

# Spec: Manufacturer Dictionary

## Motivation
Import urządzeń wymaga identyfikacji producenta po kodzie z pliku Excel.
Słownik producentów dostarcza mapowania Code → Name i będzie używany przy walidacji
i wzbogacaniu danych importu.

## In Scope
- CRUD (Create, UpdateName, Get) dla producentów
- Zasilenie danych: ~20 największych producentów konsumenckich
- REST API w module Inventory

## Out of Scope
- Powiązanie producenta z urządzeniem (SPEC-006+)
- Logo / URL producenta
- Paginacja i filtrowanie (mały słownik)

## Domain Changes

### New Aggregates / Entities
- `Manufacturer` — AggregateRoot<ManufacturerId>; pola: `Code` (immutable), `Name` (mutable)
- `ManufacturerId` — `record(Guid Value)` z `static New()`

### Modified Aggregates / Entities
- None

### New Value Objects
- None

### Domain Events
- None

### Domain Services
- None

### Business Rules
- `Code` jest immutable po utworzeniu
- `Code` musi być unikalny w ramach wszystkich producentów

## Application Layer

### Use Cases
- `CreateManufacturerUseCase` — sprawdza `ExistsByCodeAsync`, tworzy `Manufacturer`, `AddAsync`, `SaveChangesAsync`
- `UpdateManufacturerUseCase` — `FindByCodeAsync`, `UpdateName`, `SaveChangesAsync`
- `GetManufacturersUseCase` — `FindAllAsync`, mapuje do DTO

### Port Interfaces (Abstractions)
- `IManufacturerRepository` — `FindByCodeAsync`, `FindAllAsync`, `AddAsync`, `ExistsByCodeAsync`

## Infrastructure Layer

### Repositories
- `ManufacturerRepository` implementuje `IManufacturerRepository`

### EF Configurations
- `ManufacturerConfiguration` — `ToTable("Manufacturers")`, schema `inventory`, unikalny indeks na `Code`, max lengths

### Migrations
- `AddManufacturers` — tworzy tabelę `inventory.Manufacturers`
- `SeedManufacturerDictionary` — `migrationBuilder.InsertData()` z danymi (deterministyczne Guidy):
  Apple, Samsung, Huawei, Xiaomi, OPPO, Vivo, Motorola, Sony, LG, Nokia,
  OnePlus, Google, Realme, Honor, Asus, Lenovo, HP, Dell, Microsoft, Philips

### External Services
- None

## Contracts Layer

### Requests
- `CreateManufacturerRequest(string Code, string Name)`
- `UpdateManufacturerRequest(string Name)`

### Responses
- `CreateManufacturerResponse(string Code, string Name)`
- `UpdateManufacturerResponse(string Code, string Name)`
- `GetManufacturersResponse(IReadOnlyList<ManufacturerDto> Manufacturers)`

### Dtos
- `ManufacturerDto(string Code, string Name)`

## Api Layer

### Endpoints
- `POST   /inventory/manufacturers` — CreateManufacturer → 201 Created
- `PATCH  /inventory/manufacturers/{code}` — UpdateManufacturer → 200 OK
- `GET    /inventory/manufacturers` — GetManufacturers → 200 OK

### Validators
- `CreateManufacturerRequestValidator` — Code: required, max 50; Name: required, max 100
- `UpdateManufacturerRequestValidator` — Name: required, max 100

## Web Layer

### Pages / Components
- None

### API Calls
- None

## Dependencies
- Other specs: SPEC-004 (wzorzec słownika)
- External services: None
- Must be done first: None

## Acceptance Criteria
- [ ] `POST /inventory/manufacturers` tworzy producenta i zwraca 201
- [ ] `POST /inventory/manufacturers` z duplikatem Code zwraca 409
- [ ] `PATCH /inventory/manufacturers/{code}` aktualizuje nazwę i zwraca 200
- [ ] `PATCH /inventory/manufacturers/{code}` dla nieistniejącego kodu zwraca 404
- [ ] `GET /inventory/manufacturers` zwraca wszystkich producentów z danymi seed
- [ ] Code jest immutable (brak endpointu do zmiany)
- [ ] ~20 producentów dostępnych po pierwszym uruchomieniu (z migracji)

## Implementation Checklist
- [x] Domain: entities / value objects
- [x] Application: use case(s)
- [x] Application: port interfaces
- [x] Infrastructure: repository implementation(s)
- [x] Infrastructure: EF configuration(s)
- [x] Infrastructure: migration
- [x] Contracts: request / response / dto records
- [x] Api: endpoint(s)
- [x] Api: validator(s)
- [x] Unit tests — Domain
- [x] Unit tests — Application
- [x] Integration tests — Infrastructure

## Open Questions
- None

## Technical Notes
- Wzorzec identyczny z DeviceType (SPEC-004) — bez podtypów
- Seed data w migracji `InsertData`, nie seeder (per projekt-konwencja)
