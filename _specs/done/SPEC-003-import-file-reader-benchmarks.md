---
id: SPEC-003
title: Import File Reader Performance Benchmarks
module: Import
status: in-progress
created: 2026-06-29
updated: 2026-06-29
branch: feature/SPEC-003-import-file-reader-benchmarks
related: []
---

# Spec: Import File Reader Performance Benchmarks

## Motivation
Przed optymalizacją (lub jako baseline do przyszłych optymalizacji) potrzebujemy zmierzonych danych
o wydajności parserów plików w warstwie Infrastructure. Docelowy rozmiar pliku to 500 000 wierszy
× 10 kolumn. Bez twardych liczb nie wiadomo, gdzie leży bottleneck ani czy refaktor cokolwiek poprawia.

## In Scope
- Benchmarki BenchmarkDotNet dla `ExcelFileReader` (MiniExcelLibs, .xlsx)
- Benchmarki BenchmarkDotNet dla `CsvFileReader` (CsvHelper, .csv)
- Parametryzacja: 1 000 / 10 000 / 100 000 / 500 000 wierszy × 10 kolumn
- Pomiar czasu i alokacji pamięci (`[MemoryDiagnoser]`)
- Generator fikstur w pamięci (MemoryStream) — brak plików na dysku w repo

## Out of Scope
- Benchmarki endpointów HTTP / pełnego pipeline'u
- Testy walidacji wierszy (RowValidator)
- Inne formaty plików (XLS, ODS, itp.)
- Testy obciążeniowe współbieżności

## Domain Changes
None

## Application Layer

### Use Cases
None

### Port Interfaces (Abstractions)
None

## Infrastructure Layer

### Repositories
None

### EF Configurations
None

### Migrations
None

### External Services
None — dane generowane in-memory przez `FileFixtureGenerator`

## Contracts Layer

### Requests
None

### Responses
None

### Dtos
None

## Api Layer

### Endpoints
None

### Validators
None

## Web Layer
None

## Dependencies
- Other specs: brak
- External services: brak
- Must be done first: brak — `ExcelFileReader` i `CsvFileReader` już istnieją

## Acceptance Criteria
- [x] Projekt `tests/Pricing.Import.PerformanceBenchmarks` kompiluje się i wchodzi do `Pricing.slnx`
- [x] `ExcelFileReaderBenchmarks` uruchamia się dla 1k / 10k / 100k / 500k wierszy bez błędów
- [x] `CsvFileReaderBenchmarks` uruchamia się dla 1k / 10k / 100k / 500k wierszy bez błędów
- [x] Wynik zawiera kolumnę `Allocated` (MemoryDiagnoser aktywny)
- [x] `FileFixtureGenerator` generuje pliki in-memory (bez I/O dyskowego)
- [x] `Pricing.Import.Infrastructure` ma `InternalsVisibleTo("Pricing.Import.PerformanceBenchmarks")`

## Implementation Checklist
- [x] Infrastructure: dodać `InternalsVisibleTo` w `Pricing.Import.Infrastructure`
- [x] Benchmarks: nowy projekt `tests/Pricing.Import.PerformanceBenchmarks`
- [x] Benchmarks: `FileFixtureGenerator` — generuje `MemoryStream` CSV i XLSX
- [x] Benchmarks: `ExcelFileReaderBenchmarks` z `[Params(1_000, 10_000, 100_000, 500_000)]`
- [x] Benchmarks: `CsvFileReaderBenchmarks` z `[Params(1_000, 10_000, 100_000, 500_000)]`
- [x] Benchmarks: `Program.cs` z `BenchmarkRunner`
- [x] Slnx: dodać projekt do `Pricing.slnx`

## Open Questions
- Czy fixture generowany przez `FileFixtureGenerator` ma używać konkretnego modelu (np. istniejącego
  `ImportRow` z projektu), czy anonimowego rekordu z polami `Col1`…`Col10`?
- Czy benchmarki mają wejść do CI (długi czas) czy być uruchamiane tylko lokalnie?

## Technical Notes
- `ExcelFileReader` robi 2 passy po pliku (header + typed) — MemoryStream musi być seekable;
  `FileFixtureGenerator` musi generować seekable stream dla XLSX
- `ExcelFileReader` i `CsvFileReader` są `internal sealed` → konieczny `InternalsVisibleTo`
  lub benchmarkowanie przez `FileReaderFacade` (wtedy `IFileReader` jako punkt wejścia)
- BenchmarkDotNet wymaga konfiguracji Release build; nie dodawać do regularnego `dotnet test`
