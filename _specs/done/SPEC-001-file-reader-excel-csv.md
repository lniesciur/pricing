---
id: SPEC-001
title: File Reader Library (Excel & CSV)
module: Import
status: implemented
created: 2026-06-28
updated: 2026-06-28
branch: feature/SPEC-001-file-reader-excel-csv
related: []
---

# Spec: File Reader Library (Excel & CSV)

## Motivation
Moduł Import będzie przetwarzał pliki CSV i Excel przesyłane przez partnerów.
Potrzebna jest spójna biblioteka do czytania tych plików z walidacją nagłówka
i wierszy biznesowych — jako fundament pod przyszłe use case'y ingestii.

## In Scope
- Interfejs portu `IFileReader` w Application (testowalność use case'ów przez NSubstitute)
- Typy wynikowe: `ParseResult<TRow>`, `FileParseError`, `FileReaderOptions<TRow>`
- Interfejs `IRowValidator<TRow>` (bez FluentValidation)
- Auto-wykrywanie formatu pliku po rozszerzeniu (`fileName`) — brak `FileFormat` w API
- Implementacja `ExcelFileReader` — MiniExcel (streaming, stałe zużycie pamięci)
- Implementacja `CsvFileReader` — CsvHelper (streaming)
- `FileReaderFacade : IFileReader` — routuje po rozszerzeniu pliku
- Walidacja wiersza nagłówka (oczekiwane nazwy kolumn)
- Walidacja wierszy biznesowych przez `IRowValidator<TRow>`
- Błędny wiersz → zbierz błąd, kontynuuj czytanie pozostałych
- Unit testy wszystkich trzech klas z in-memory `MemoryStream`

## Out of Scope
- Endpoint HTTP (pojawi się w kolejnym spec)
- Zapis sparsowanych danych do bazy
- Obsługa plików wieloarkuszowych (Excel — tylko pierwszy arkusz)
- Wykrywanie formatu po magic bytes (rozszerzenie wystarczy)
- Testy wydajnościowe (BenchmarkDotNet — rozważyć w v2)

## Domain Changes
None.

## Application Layer

### Port Interface
```csharp
// Pricing.Import.Application/FileReading/IFileReader.cs
public interface IFileReader
{
    Task<ParseResult<TRow>> ReadAsync<TRow>(
        Stream stream,
        string fileName,
        FileReaderOptions<TRow> options)
        where TRow : class, new();
}
```

### Supporting Types (Application/FileReading/)
```csharp
public sealed class FileReaderOptions<TRow> where TRow : class
{
    public IReadOnlyList<string> ExpectedHeaders { get; init; } = [];
    public IRowValidator<TRow>? RowValidator { get; init; }
}

public interface IRowValidator<TRow>
{
    IEnumerable<string> Validate(TRow row);
}

public sealed class ParseResult<TRow>
{
    public IReadOnlyList<TRow> Rows { get; init; } = [];
    public IReadOnlyList<FileParseError> Errors { get; init; } = [];
    public bool IsSuccess => Errors.Count == 0;
}

public sealed record FileParseError(int RowNumber, string Message);
```

### Use Cases
Brak w tej wersji. `IFileReader` jest wstrzykiwany do przyszłych use case'ów.

### Port Interfaces
- `IFileReader` — jak wyżej

## Infrastructure Layer

### Implementacje (Pricing.Import.Infrastructure/FileReading/)
- `CsvFileReader` — CsvHelper; mapuje kolumny po nagłówkach; streaming
- `ExcelFileReader` — MiniExcel; czyta pierwszy arkusz; streaming (stałe RAM niezależnie od rozmiaru)
- `FileReaderFacade : IFileReader` — wykrywa format po rozszerzeniu `fileName`
  (`.csv` → `CsvFileReader`, `.xlsx` / `.xls` → `ExcelFileReader`),
  rzuca `NotSupportedException` dla nieznanych rozszerzeń

### Rejestracja DI
`FileReaderFacade` rejestrowany jako `IFileReader` (scoped) w `AddImportInfrastructure`.

### External Services / NuGet
- `MiniExcel` — czytanie .xlsx / .xls (streaming, MIT)
- `CsvHelper` — czytanie .csv (streaming, Apache 2.0)

### Migrations
Brak.

## Contracts Layer
Brak w tej wersji (brak endpointu HTTP).

## Api Layer
Brak w tej wersji.

## Dependencies
- Other specs: brak
- External services: brak
- Must be done first: brak

## Acceptance Criteria
- [ ] `ReadAsync` poprawnie parsuje plik CSV z nagłówkiem do listy `TRow`
- [ ] `ReadAsync` poprawnie parsuje plik Excel (.xlsx) z nagłówkiem do listy `TRow`
- [ ] Format wykrywany automatycznie po rozszerzeniu `fileName`
- [ ] Nieznane rozszerzenie → `NotSupportedException`
- [ ] Brakujące kolumny nagłówka → `IsSuccess = false`, błąd z listą brakujących kolumn
- [ ] Wiersz niespełniający `IRowValidator<TRow>` → trafia do `Errors`, pozostałe przetwarzane dalej
- [ ] Plik z 200k wierszy przetwarzany bez OOM (streaming)
- [ ] `IFileReader` wstrzykiwany przez DI (`AddImportModule`)
- [ ] Unit testy: `CsvFileReader`, `ExcelFileReader`, `FileReaderFacade` — in-memory stream

## Implementation Checklist
- [x] Application: `IFileReader`, `FileReaderOptions<TRow>`, `IRowValidator<TRow>`
- [x] Application: `ParseResult<TRow>`, `FileParseError`
- [x] Infrastructure: `CsvFileReader`
- [x] Infrastructure: `ExcelFileReader`
- [x] Infrastructure: `FileReaderFacade : IFileReader`
- [x] Infrastructure: rejestracja DI
- [x] Infrastructure: NuGet — MiniExcel, CsvHelper
- [x] Unit testy: `CsvFileReader` — happy path, błędny nagłówek, błędny wiersz
- [x] Unit testy: `ExcelFileReader` — happy path, błędny nagłówek, błędny wiersz
- [x] Unit testy: `FileReaderFacade` — routing po rozszerzeniu, nieznane rozszerzenie

## Open Questions
Brak.

## Technical Notes
- `FileReaderFacade` to "smart facade" ukrywająca dwie implementacje za jednym interfejsem
- `IRowValidator<TRow>` bez FluentValidation — wywołujący implementuje jak chce (lambda, klasa, switch expression)
- MiniExcel zamiast ClosedXML — kluczowe dla 200k wierszy; ClosedXML ładuje cały DOM do RAM
- `ReadAsync` async end-to-end — MiniExcel i CsvHelper obydwa mają natywne async API
- Test wydajnościowy OOM weryfikowany ręcznie lub w integration teście z prawdziwym plikiem; BenchmarkDotNet rozważyć w v2
