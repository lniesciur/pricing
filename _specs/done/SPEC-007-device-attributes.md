---
id: SPEC-007
title: Device Attributes
module: Inventory
status: implemented
created: 2026-07-02
updated: 2026-07-02
branch: feature/SPEC-007-device-attributes
related: [SPEC-006]
---

# Spec: Device Attributes

## Motivation
Urządzenia importowane do systemu nie mają możliwości przechowania dodatkowych danych
specyfikacji (np. kolor, pamięć). Rozszerzenie modelu urządzenia o generyczny mechanizm
atrybutów (klucz-wartość) pozwala elastycznie importować i przechowywać dowolne cechy
urządzenia bez migracji przy każdym nowym polu.

## In Scope
- Nowy value object `DeviceAttribute(Name, Value)` w domenie Inventory
- Rozszerzenie agregatu `Device` o kolekcję atrybutów (z regułą unikalności nazw)
- Rozszerzenie `RegisterDeviceRequest` o opcjonalną listę atrybutów
- Rozszerzenie importu CSV/XLSX o opcjonalne kolumny `Color` i `Memory`
- Przechowywanie atrybutów jako JSON column w tabeli `Devices`
- Rozszerzenie `tools/generate_import_files.py` o kolumny `Color` i `Memory`

## Out of Scope
- Endpoint do aktualizacji atrybutów po imporcie
- Endpoint do pobierania urządzenia z atrybutami (GET device)
- Filtrowanie urządzeń po wartości atrybutu
- Generyczne parsowanie dowolnych kolumn CSV poza `Color` i `Memory`

## Domain Changes

### New Aggregates / Entities
- None

### Modified Aggregates / Entities
- `Device` — dodanie `IReadOnlyList<DeviceAttribute> Attributes` oraz rozszerzenie
  `Create(...)` o parametr `IReadOnlyList<DeviceAttribute>? attributes`

### New Value Objects
- `DeviceAttribute(string Name, string Value)` — immutable record; Name nie może być pusty

### Domain Events
- None

### Domain Services
- None

### Business Rules
- Nazwy atrybutów muszą być unikalne per urządzenie (case-insensitive)
- Wartości atrybutów mogą być dowolnym stringiem (włącznie z pustym)
- Atrybuty są opcjonalne — urządzenie bez atrybutów jest poprawne

## Application Layer

### Use Cases
- `RegisterDevicesUseCase` (modyfikacja) — mapuje `request.Attributes`
  (`DeviceAttributeDto` → `DeviceAttribute`) i przekazuje do `Device.Create(...)`

#### Import module — ProcessDeviceImportUseCase (modyfikacja)
- `DeviceImportRawRow` — dodać `string? Color`, `string? Memory`
  (poza `ExpectedHeaders` — brak kolumny w pliku = `null`, brak błędu)
- `ParseFileAsync` — buduje listę `DeviceAttributeDto` z niezerowych wartości
  `Color` i `Memory` i przekazuje w `RegisterDeviceRequest.Attributes`

### Port Interfaces (Abstractions)
- None

## Infrastructure Layer

### Repositories
- `DeviceRepository` — bez zmian

### EF Configurations
- `DeviceConfiguration` — dodać `.OwnsMany(d => d.Attributes, b => b.ToJson())`

### Migrations
- Inventory: dodanie kolumny `Attributes NVARCHAR(MAX) NULL` do tabeli `Devices`

### External Services
- None

## Contracts Layer

### Requests
- `RegisterDeviceRequest` (modyfikacja) — dodać `IReadOnlyList<DeviceAttributeDto>? Attributes`

### Responses
- None

### Dtos
- `DeviceAttributeDto(string Name, string Value)` — nowy record
  w `Pricing.Inventory.Contracts/Devices/`

## Api Layer

### Endpoints
- None

### Validators
- None

## Web Layer

### Pages / Components
- None

### API Calls
- None

## Dependencies
- Other specs: SPEC-006 (device import pipeline — already implemented)
- External services: None
- Must be done first: None

## Acceptance Criteria
- [ ] Plik CSV z kolumnami `Color` i `Memory` importuje urządzenia z tymi atrybutami
- [ ] Plik CSV bez kolumn `Color` i `Memory` importuje urządzenia bez błędu i bez atrybutów
- [ ] Plik XLSX z kolumnami `Color` i `Memory` importuje urządzenia z tymi atrybutami
- [ ] Duplikat nazwy atrybutu (case-insensitive) jest odrzucany przez domenę
- [ ] Atrybuty są persystowane jako JSON w kolumnie `Attributes` tabeli `Devices`
- [ ] Urządzenie bez atrybutów ma `Attributes = null` lub `[]` w bazie (nie błąd)

## Implementation Checklist
- [x] Domain: entities / value objects
- [x] Domain: domain events
- [x] Domain: domain services
- [x] Domain: business rules
- [x] Application: use case(s)
- [x] Application: port interfaces
- [x] Infrastructure: repository implementation(s)
- [x] Infrastructure: EF configuration(s)
- [x] Infrastructure: migration
- [x] Contracts: request / response / dto records
- [x] Web: page(s) / component(s)
- [x] Api: endpoint(s)
- [x] Api: validator(s)
- [x] Tools: generate_import_files.py — kolumny Color i Memory
- [x] Unit tests — Domain
- [x] Unit tests — Application
- [ ] Integration tests — Infrastructure

## Open Questions
- None

## Technical Notes
- `ExpectedHeaders` nie zawiera `Color` ani `Memory` — `MissingFieldFound = null`
  w `CsvFileReader` sprawia że brakujące kolumny są po cichu ignorowane
- JSON column zamiast tabeli potomnej: brak przypadków filtrowania po atrybucie (YAGNI);
  zmiana na tabelę w przyszłości = tylko modyfikacja konfiguracji EF + nowa migracja
- `generate_import_files.py`: `COLUMNS` → dodać `Color`, `Memory`; sample dane np.
  `COLORS = ["Black", "White", "Silver", "Blue", "Red"]`,
  `MEMORY_SIZES = ["8GB", "16GB", "32GB", "64GB", "128GB"]`;
  wartości przypisane przez `i % len(...)`, analogicznie do istniejących pól
