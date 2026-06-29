---
id: SPEC-004
title: Device Type and Subtype Dictionaries
module: Inventory
status: implemented
created: 2026-06-29
updated: 2026-06-29
branch: feature/SPEC-004-device-type-subtype-dict
related: []
---

# Spec: Device Type and Subtype Dictionaries

## Motivation
Urządzenia w katalogu Inventory mają Type i Subtype (np. "Telefon > 5G").
Słowniki tych wartości muszą być zarządzalne przez API — kody będą używane
w importach (SPEC-005) i w modelu Device. Subtype jest zawsze powiązany
z konkretnym Type.

## In Scope
- CRUD (Create, Update, Get) dla DeviceType
- CRUD (Create, Update, Get) dla DeviceSubtype w ramach DeviceType
- Code jest immutable po utworzeniu
- Walidacja unikalności Code

## Out of Scope
- Delete Type/Subtype
- Paginacja i filtrowanie
- Eksponowanie słowników przez IInventoryFacade (SPEC-005)
- Encja Device (SPEC-005)

## Domain Changes

### New Aggregates / Entities
- `DeviceType` (AggregateRoot) — `DeviceTypeId`, `Code` (immutable, unikalny globalnie), `Name`
- `DeviceSubtype` (child entity wewnątrz `DeviceType`) — `DeviceSubtypeId`, `Code` (immutable, unikalny w obrębie Type), `Name`

### Modified Aggregates / Entities
- None

### New Value Objects
- None

### Domain Events
- `DeviceTypeCreated(DeviceTypeId, Code, Name)`
- `DeviceSubtypeAdded(DeviceTypeId, DeviceSubtypeId, Code, Name)`

### Domain Services
- None

### Business Rules
- `DeviceType.Code` musi być unikalny globalnie — sprawdzenie przed `Create` w use case
- `DeviceSubtype.Code` musi być unikalny w obrębie swojego `DeviceType` — agregat sprawdza przy `AddSubtype(...)`
- `Code` jest immutable — brak metody zmiany kodu na agregacie

## Application Layer

### Use Cases
- `CreateDeviceTypeUseCase` — tworzy DeviceType, sprawdza unikalność Code przez repo, zwraca `Result<CreateDeviceTypeResponse>`
- `UpdateDeviceTypeUseCase` — zmienia Name na istniejącym Type (lookup by Code), zwraca `Result<UpdateDeviceTypeResponse>`
- `GetDeviceTypesUseCase` — zwraca wszystkie Types z zagnieżdżonymi Subtypes, zwraca `Result<GetDeviceTypesResponse>`
- `AddDeviceSubtypeUseCase` — ładuje Type by Code, wywołuje `deviceType.AddSubtype(code, name)`, zwraca `Result<AddDeviceSubtypeResponse>`
- `UpdateDeviceSubtypeUseCase` — ładuje Type by Code, wywołuje `deviceType.UpdateSubtype(subtypeCode, name)`, zwraca `Result<UpdateDeviceSubtypeResponse>`

### Port Interfaces (Abstractions)
- `IDeviceTypeRepository` — `FindByCodeAsync(string code)`, `FindAllAsync()`, `AddAsync(DeviceType)`, `ExistsByCodeAsync(string code)`

## Infrastructure Layer

### Repositories
- `DeviceTypeRepository` — implementuje `IDeviceTypeRepository`, używa `InventoryDbContext`, eager load Subtypes

### EF Configurations
- `DeviceTypeConfiguration` — tabela `device_types`, schema `inventory`, konwersja `DeviceTypeId`
- `DeviceSubtypeConfiguration` — tabela `device_subtypes`, schema `inventory`, FK → `device_types`, konwersja `DeviceSubtypeId`

### Migrations
- `AddDeviceTypesAndSubtypes`

### External Services
- None

## Contracts Layer

### Requests
- `CreateDeviceTypeRequest` — `{ string Code, string Name }`
- `UpdateDeviceTypeRequest` — `{ string Name }`
- `AddDeviceSubtypeRequest` — `{ string Code, string Name }`
- `UpdateDeviceSubtypeRequest` — `{ string Name }`

### Responses
- `CreateDeviceTypeResponse` — `{ string Code, string Name }`
- `UpdateDeviceTypeResponse` — `{ string Code, string Name }`
- `GetDeviceTypesResponse` — `{ IReadOnlyList<DeviceTypeDto> Types }`
- `AddDeviceSubtypeResponse` — `{ string Code, string Name }`
- `UpdateDeviceSubtypeResponse` — `{ string Code, string Name }`

### Dtos
- `DeviceTypeDto` — `{ string Code, string Name, IReadOnlyList<DeviceSubtypeDto> Subtypes }`
- `DeviceSubtypeDto` — `{ string Code, string Name }`

## Api Layer

### Endpoints
| Method | Route | Use Case | Success |
|--------|-------|----------|---------|
| `POST` | `/inventory/device-types` | CreateDeviceTypeUseCase | `201 Created` |
| `PATCH` | `/inventory/device-types/{code}` | UpdateDeviceTypeUseCase | `200 OK` |
| `GET` | `/inventory/device-types` | GetDeviceTypesUseCase | `200 OK` |
| `POST` | `/inventory/device-types/{code}/subtypes` | AddDeviceSubtypeUseCase | `201 Created` |
| `PATCH` | `/inventory/device-types/{code}/subtypes/{subtypeCode}` | UpdateDeviceSubtypeUseCase | `200 OK` |

Błędy domenowe (duplikat Code, nieistniejący Type/Subtype) → `409 Conflict`.

### Validators
- `CreateDeviceTypeValidator` — `Code` not empty, max 50 chars; `Name` not empty, max 200 chars
- `UpdateDeviceTypeValidator` — `Name` not empty, max 200 chars
- `AddDeviceSubtypeValidator` — `Code` not empty, max 50 chars; `Name` not empty, max 200 chars
- `UpdateDeviceSubtypeValidator` — `Name` not empty, max 200 chars

## Web Layer
- None

## Dependencies
- Other specs: None
- External services: None
- Must be done first: None

## Acceptance Criteria
- [ ] `POST /inventory/device-types` tworzy Type, zwraca `201` z `{ Code, Name }`
- [ ] `POST /inventory/device-types` zwraca `409` gdy Code już istnieje
- [ ] `PATCH /inventory/device-types/{code}` zmienia Name, zwraca `200` z `{ Code, Name }`
- [ ] `PATCH /inventory/device-types/{code}` zwraca `409` gdy Type nie istnieje
- [ ] `GET /inventory/device-types` zwraca wszystkie Types z zagnieżdżonymi Subtypes
- [ ] `POST /inventory/device-types/{code}/subtypes` dodaje Subtype, zwraca `201`
- [ ] `POST /inventory/device-types/{code}/subtypes` zwraca `409` gdy subtypeCode już istnieje w obrębie Type
- [ ] `PATCH /inventory/device-types/{code}/subtypes/{subtypeCode}` zmienia Name, zwraca `200`
- [ ] Code jest immutable — brak możliwości zmiany przez API
- [ ] Testy jednostkowe — Domain (AddSubtype, unikalność kodu)
- [ ] Testy jednostkowe — Application (każdy use case)
- [ ] Testy integracyjne — Infrastructure (repo + EF)

## Implementation Checklist
- [x] Domain: DeviceType aggregate + DeviceSubtype child entity
- [x] Domain: domain events (DeviceTypeCreated, DeviceSubtypeAdded)
- [x] Domain: business rules (Code uniqueness)
- [x] Application: use case(s)
- [x] Application: IDeviceTypeRepository interface
- [x] Infrastructure: DeviceTypeRepository
- [x] Infrastructure: EF configurations (DeviceType + DeviceSubtype)
- [x] Infrastructure: migration AddDeviceTypesAndSubtypes
- [x] Contracts: request / response / dto records
- [x] Api: endpoints (5 endpointów)
- [x] Api: validators (4 validatory)
- [x] Unit tests — Domain
- [x] Unit tests — Application
- [x] Integration tests — Infrastructure

## Open Questions
- None

## Seed Data

Dane do wygenerowania przy pierwszym uruchomieniu (migracja lub seeder):

| DeviceType.Code | DeviceType.Name | DeviceSubtype.Code | DeviceSubtype.Name |
|---|---|---|---|
| `PHONE` | Smartfon | `BASIC` | Podstawowy |
| `PHONE` | Smartfon | `MID` | Średnia półka |
| `PHONE` | Smartfon | `PREMIUM` | Premium |
| `PHONE` | Smartfon | `FOLDABLE` | Składany |
| `PHONE` | Smartfon | `RUGGED` | Wzmocniony |
| `PHONE` | Smartfon | `5G` | 5G |
| `TABLET` | Tablet | `WIFI` | Wi-Fi |
| `TABLET` | Tablet | `LTE` | LTE |
| `TABLET` | Tablet | `5G` | 5G |
| `TABLET` | Tablet | `KIDS` | Dla dzieci |
| `TABLET` | Tablet | `DRAWING` | Graficzny |
| `LAPTOP` | Laptop | `ULTRABOOK` | Ultrabook |
| `LAPTOP` | Laptop | `GAMING` | Gamingowy |
| `LAPTOP` | Laptop | `BUSINESS` | Biznesowy |
| `LAPTOP` | Laptop | `2IN1` | 2 w 1 |
| `LAPTOP` | Laptop | `CHROMEBOOK` | Chromebook |
| `SMARTWATCH` | Smartwatch | `SPORT` | Sportowy |
| `SMARTWATCH` | Smartwatch | `CLASSIC` | Klasyczny |
| `SMARTWATCH` | Smartwatch | `KIDS` | Dla dzieci |
| `SMARTWATCH` | Smartwatch | `MEDICAL` | Medyczny |
| `ACCESSORY` | Akcesorium | `CASE` | Etui |
| `ACCESSORY` | Akcesorium | `CHARGER` | Ładowarka |
| `ACCESSORY` | Akcesorium | `HEADPHONES` | Słuchawki |
| `ACCESSORY` | Akcesorium | `POWERBANK` | Powerbank |
| `ACCESSORY` | Akcesorium | `CABLE` | Kabel |
| `ACCESSORY` | Akcesorium | `SCREEN_PROTECTOR` | Szkło ochronne |
| `ACCESSORY` | Akcesorium | `KEYBOARD` | Klawiatura |
| `ACCESSORY` | Akcesorium | `MOUSE` | Mysz |
| `MONITOR` | Monitor | `OFFICE` | Biurowy |
| `MONITOR` | Monitor | `GAMING` | Gamingowy |
| `MONITOR` | Monitor | `GRAPHIC` | Graficzny |
| `MONITOR` | Monitor | `PORTABLE` | Przenośny |
| `TV` | Telewizor | `LED` | LED |
| `TV` | Telewizor | `OLED` | OLED |
| `TV` | Telewizor | `QLED` | QLED |
| `TV` | Telewizor | `SMART` | Smart TV |
| `CONSOLE` | Konsola | `STATIONARY` | Stacjonarna |
| `CONSOLE` | Konsola | `HANDHELD` | Przenośna |
| `CONSOLE` | Konsola | `RETRO` | Retro |
| `ROUTER` | Router / Modem | `HOME` | Domowy |
| `ROUTER` | Router / Modem | `MESH` | Mesh |
| `ROUTER` | Router / Modem | `5G` | 5G |
| `ROUTER` | Router / Modem | `BUSINESS` | Biznesowy |
| `CAMERA` | Aparat / Kamera | `COMPACT` | Kompaktowy |
| `CAMERA` | Aparat / Kamera | `MIRRORLESS` | Bezlusterkowy |
| `CAMERA` | Aparat / Kamera | `DSLR` | DSLR |
| `CAMERA` | Aparat / Kamera | `ACTION` | Sportowa |
| `CAMERA` | Aparat / Kamera | `DRONE` | Dron |
| `E_READER` | Czytnik e-booków | `BASIC` | Podstawowy |
| `E_READER` | Czytnik e-booków | `BACKLIT` | Z podświetleniem |
| `E_READER` | Czytnik e-booków | `WATERPROOF` | Wodoodporny |
| `SMART_SPEAKER` | Głośnik inteligentny | `MINI` | Mini |
| `SMART_SPEAKER` | Głośnik inteligentny | `STANDARD` | Standardowy |
| `SMART_SPEAKER` | Głośnik inteligentny | `DISPLAY` | Z ekranem |

Seed uruchamiany jednorazowo — sprawdzić przed insertem czy rekord już istnieje (idempotentny).

## Technical Notes
- DeviceSubtype jako child entity (nie aggregate root) w DeviceType — spójność atomowa,
  unikalność kodu subtype wymuszana przez agregat bez dodatkowego repo
- Route prefix `/inventory/` odróżnia moduł Inventory od przyszłych modułów
