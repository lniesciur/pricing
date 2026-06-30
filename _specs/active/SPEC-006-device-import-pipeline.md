---
id: SPEC-006
title: Device Import Pipeline
module: Import
status: in-progress
created: 2026-06-29
updated: 2026-06-29
branch: feature/SPEC-006-device-import-pipeline
related: [SPEC-004, SPEC-005]
---

# Spec: Device Import Pipeline

## Motivation
Umożliwienie masowego importu urządzeń z pliku CSV/XLSX przez HTTP. Pipeline jest
generyczny (ImportType enum) i będzie reużywany dla przyszłych typów importu.
Przetwarzanie odbywa się asynchronicznie przez Hangfire — odpowiedź HTTP jest natychmiastowa,
a wynik dostępny przez polling statusu joba.

## In Scope
- Upload pliku CSV/XLSX przez HTTP (multipart/form-data)
- Przechowywanie pliku w bazie (varbinary(max))
- Asynchroniczne przetwarzanie przez Hangfire
- Walidacja strukturalna w Import (format, brakujące kolumny, duplikaty EanCode w pliku)
- Walidacja domenowa w Inventory (TypeCode/SubtypeCode/ManufacturerCode muszą istnieć w słownikach)
- Bulk insert przez SqlBulkCopy w transakcji
- Statystyki na jobie: Added, Skipped, Updated, Deleted
- Per-wiersz błędy w ImportJobErrors (błędy parsowania + domenowe)
- Endpoint do sprawdzenia statusu joba

## Out of Scope
- Update / Delete istniejących urządzeń (RegisterDevices = only create)
- Powiadomienia push o ukończeniu joba
- Retry mechanizm dla failowanych jobów
- Podgląd/preview pliku przed importem

## Domain Changes

### New Aggregates / Entities
**Pricing.Import.Domain:**
- `ImportJob` — AggregateRoot<ImportJobId>
  - `ImportJobId`, `FileName`, `FileType` (enum: Csv/Xlsx), `ImportType` (Shared.Contracts),
    `Status` (Shared.Contracts), `FileContent` (byte[])
  - Statystyki: `Added`, `Skipped`, `Updated`, `Deleted` (int, domyślnie 0)
  - Metody: `MarkAsProcessing()`, `MarkAsCompleted(stats)`, `MarkAsFailed(error)`
- `ImportJobId` — `record(Guid Value)` z `static New()`
- `ImportJobError` — child entity (nie aggregate root)
  - `ImportJobErrorId`, `RowNumber` (int), `ErrorMessage` (string), `ErrorType` (enum: Parse/Domain)
- `ImportJobErrorId` — `record(Guid Value)` z `static New()`
- `IImportJobRepository` — `AddAsync`, `FindByIdAsync`

**Pricing.Inventory.Domain:**
- `Device` — AggregateRoot<DeviceId>
  - `DeviceId`, `EanCode` (immutable, unikalny), `Name`, `TypeCode`, `SubtypeCode` (nullable),
    `ManufacturerCode` (nullable)
- `DeviceId` — `record(Guid Value)` z `static New()`
- `IDeviceRepository` — nie potrzebny dla bulk path; Inventory facade bezpośrednio przez SqlBulkCopy

### Modified Aggregates / Entities
- None

### New Value Objects
- None

### Domain Events
- None

### Domain Services
- None

### Business Rules
- `EanCode` urządzenia jest immutable po utworzeniu
- `EanCode` musi być unikalny w tabeli `inventory.Devices`
- Duplikaty `EanCode` w obrębie jednego pliku → błąd Parse na drugim wystąpieniu
- `TypeCode` musi istnieć w `inventory.DeviceTypes`
- `SubtypeCode` (jeśli podany) musi istnieć w `inventory.DeviceSubtypes` i należeć do wskazanego `TypeCode`
- `ManufacturerCode` (jeśli podany) musi istnieć w `inventory.Manufacturers`

## Application Layer

### Use Cases
**Pricing.Import.Application:**
- `UploadDeviceImportUseCase` — waliduje rozszerzenie pliku, tworzy `ImportJob`, zapisuje do DB,
  kolejkuje `ProcessDeviceImportJob` przez Hangfire, zwraca JobId
- `ListDeviceImportsUseCase` — zwraca stronicowaną listę `ImportJob` posortowaną po `CreatedAt` DESC,
  opcjonalny filtr po `ImportJobStatus`
- `ProcessDeviceImportJobHandler` (Hangfire IJob) — ładuje ImportJob, parsuje plik przez `IFileReader`,
  waliduje strukturalnie (deduplikacja EanCode, wymagane kolumny), zbiera błędy Parse,
  wywołuje `IInventoryFacade.RegisterDevicesAsync(validRows)` raz dla całej partii,
  scala wynik (`RegisterDevicesResult`) ze statystykami parsowania, wywołuje `MarkAsCompleted`/`MarkAsFailed`

**Pricing.Inventory.Application:**
- `RegisterDevicesUseCase` — przyjmuje `IReadOnlyList<DeviceImportRow>`, waliduje domenowo
  (TypeCode/SubtypeCode/ManufacturerCode przez zapytania do DB), segreguje na valid/invalid,
  wywołuje `SqlBulkCopy` przez `ExecuteInTransactionAsync`, zwraca `RegisterDevicesResult`

### Port Interfaces (Abstractions)
- `IImportJobRepository` — `AddAsync(ImportJob)`, `FindByIdAsync(ImportJobId)`, `ListAsync(ImportJobStatus? status, int page, int pageSize)`
- `IImportUnitOfWork : IUnitOfWork`
- `IInventoryFacade` (rozszerzony) — `RegisterDevicesAsync(IReadOnlyList<DeviceImportRow>)`
  → `Task<RegisterDevicesResult>`

## Infrastructure Layer

### Repositories
- `ImportJobRepository` — implementuje `IImportJobRepository`

### EF Configurations
- `ImportJobConfiguration` — `ToTable("ImportJobs")`, schema `import`, `varbinary(max)` dla FileContent,
  cascade delete dla ImportJobErrors
- `ImportJobErrorConfiguration` — `ToTable("ImportJobErrors")`, schema `import`, FK do ImportJob

### Migrations
- `AddImportJobsAndErrors` — tworzy tabele `import.ImportJobs` i `import.ImportJobErrors`

### External Services
- **Hangfire** — nowe pakiety: `Hangfire.AspNetCore`, `Hangfire.SqlServer`
  - Schema: `hangfire` (per CLAUDE.md)
  - Konfiguracja w `AddImportModule()` + `StartImportModuleAsync()`
- **SqlBulkCopy** — w `InventoryFacadeImpl` (`Pricing.Inventory.Infrastructure`),
  używa `ExecuteInTransactionAsync` z `IInventoryUnitOfWork`,
  wstawia do `inventory.Devices`

### InventoryFacade implementation
- `InventoryFacadeImpl` w `Pricing.Inventory.Infrastructure` — implementuje `IInventoryFacade`,
  wstrzykuje `RegisterDevicesUseCase`, `IInventoryUnitOfWork`, `InventoryDbContext`

## Contracts Layer

### `Pricing.Shared.Contracts` (nowe enumy)
- `ImportType` enum: `DeviceImport`
- `ImportJobStatus` enum: `Queued`, `Processing`, `Completed`, `Failed`
- `FileType` enum: `Csv`, `Xlsx`

### `Pricing.Inventory.Contracts/Devices/`
- `DeviceImportRow(string EanCode, string Name, string TypeCode, string? SubtypeCode, string? ManufacturerCode)`
- `RegisterDevicesResult(int Added, int Skipped, int Updated, int Deleted, IReadOnlyList<DeviceImportError> Errors)`
- `DeviceImportError(int RowNumber, string ErrorMessage)`

### `Pricing.Import.Contracts/DeviceImports/`
- `UploadDeviceImportResponse(Guid JobId, ImportJobStatus Status)`
- `GetDeviceImportResponse(Guid JobId, ImportJobStatus Status, ImportType ImportType, string FileName, int Added, int Skipped, int Updated, int Deleted, IReadOnlyList<ImportJobErrorDto> Errors)`
- `ImportJobErrorDto(int RowNumber, string ErrorMessage, string ErrorType)`
- `ListDeviceImportsResponse(IReadOnlyList<DeviceImportSummaryDto> Items, int TotalCount, int Page, int PageSize)`
- `DeviceImportSummaryDto(Guid JobId, string FileName, ImportJobStatus Status, int Added, int Skipped, int Updated, int Deleted, DateTime CreatedAt)`

## Api Layer

### Endpoints
- `POST /import/device-imports` — multipart/form-data (`IFormFile file`), MaxRequestBodySize = brak limitu,
  → 202 Accepted z `UploadDeviceImportResponse`
- `GET /import/device-imports` — query params: `status` (opcjonalny), `page` (domyślnie 1), `pageSize` (domyślnie 20),
  sortowanie po `CreatedAt` DESC → 200 OK z `ListDeviceImportsResponse`
- `GET /import/device-imports/{jobId}` — zwraca `GetDeviceImportResponse` → 200 OK, 404 gdy brak joba

### Validators
- `UploadDeviceImportValidator` — plik wymagany, rozszerzenie `.csv` lub `.xlsx`

## Web Layer
- None

## Dependencies
- Other specs: SPEC-004 (DeviceTypes), SPEC-005 (Manufacturers) — słowniki używane w walidacji domenowej
- External services: Hangfire (nowy)
- Must be done first: SPEC-004, SPEC-005 muszą być na main przed implementacją

## Acceptance Criteria
- [ ] `POST /import/device-imports` z poprawnym plikiem zwraca 202 z JobId
- [ ] `POST /import/device-imports` z plikiem innym niż .csv/.xlsx zwraca 422
- [ ] `GET /import/device-imports` zwraca stronicowaną listę jobów posortowaną po dacie malejąco
- [ ] `GET /import/device-imports?status=Completed` filtruje po statusie
- [ ] `GET /import/device-imports/{jobId}` zwraca status i statystyki po zakończeniu
- [ ] Duplikat EanCode w pliku → błąd Parse, drugi wiersz odrzucony
- [ ] Nieznany TypeCode → błąd Domain, wiersz odrzucony, reszta wstawiona
- [ ] Poprawne wiersze wstawiane przez SqlBulkCopy w jednej transakcji
- [ ] ImportJob.Status = Completed po udanym przetwarzaniu
- [ ] ImportJob.Status = Failed gdy parsowanie rzuci wyjątek
- [ ] ImportJobErrors zawiera błędy z numerami wierszy

## Implementation Checklist
- [x] Shared.Contracts: ImportType, ImportJobStatus, FileType enums
- [x] Import Domain: ImportJob, ImportJobError, IDs, IImportJobRepository
- [x] Inventory Domain: Device, DeviceId
- [x] Inventory Contracts: DeviceImportRow, RegisterDevicesResult, DeviceImportError
- [x] Import Contracts: UploadDeviceImportResponse, GetDeviceImportResponse, ImportJobErrorDto
- [x] Import Application: UploadDeviceImportUseCase, ProcessDeviceImportJobHandler
- [x] Inventory Application: RegisterDevicesUseCase
- [x] Import Infrastructure: ImportJobRepository, EF config, migration, Hangfire setup
- [x] Inventory Infrastructure: InventoryFacadeImpl z SqlBulkCopy
- [x] Import Api: upload endpoint, get status endpoint, validator
- [x] Import Application: ListDeviceImportsUseCase
- [x] Import Api: list endpoint (`GET /import/device-imports`)
- [x] Unit tests — Import Domain
- [ ] Unit tests — Import Application
- [ ] Unit tests — Inventory Application (RegisterDevicesUseCase)
- [ ] Integration tests — Infrastructure

## Open Questions
- None

## Technical Notes
- `ProcessDeviceImportJobHandler` działa jako Hangfire background job — wstrzykiwany przez DI
- Kolumny CSV/XLSX (case-insensitive): `EanCode`, `Name`, `TypeCode`, `SubtypeCode`, `ManufacturerCode`
- `SqlBulkCopy` dostaje `SqlConnection` i `SqlTransaction` z `context.Database` wewnątrz `ExecuteInTransactionAsync`
- `IInventoryFacade` jest w `Pricing.Inventory.Facade` — Import referencuje tylko Facade, nigdy Application ani Domain Inventory
- `DeviceImportRow` i `RegisterDevicesResult` w `Pricing.Inventory.Contracts` — Import może referencować Inventory.Contracts przez Facade (kontrakt cross-modułowy)
