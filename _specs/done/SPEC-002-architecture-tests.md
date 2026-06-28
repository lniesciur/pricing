---
id: SPEC-002
title: Architecture Tests
module: Shared
status: implemented
created: 2026-06-28
updated: 2026-06-28
branch: feature/SPEC-002-architecture-tests
related: []
---

# Spec: Architecture Tests

## Motivation
Wymusić granice architektoniczne kompilatorem to za mało — reguły cross-modułowe i konwencje
nazewnicze można złamać bez błędu kompilacji. Zestaw testów NetArchTest wykrywa takie naruszenia
w CI zanim trafią do main.

## In Scope
- Nowy projekt `tests/Pricing.ArchitectureTests` z jedną referencją do `Pricing.Api` (transitive)
- Reguły izolacji warstw (Layer Rules)
- Reguły izolacji modułów (Module Boundary Rules)
- Reguły konwencji nazewniczych (Naming Convention Rules)
- Reguły czystości domeny (Domain Purity Rules)

## Out of Scope
- Testy wydajności ani analiza statyczna (Roslyn analyzers)
- Reguły dotyczące warstwy Web (brak warstwy UI w projekcie)

## Domain Changes
None

## Application Layer
None

## Infrastructure Layer
None

## Contracts Layer
None

## Api Layer
None

## Test Project

### Nowy projekt
- `tests/Pricing.ArchitectureTests/Pricing.ArchitectureTests.csproj`
  - TargetFramework: `net10.0`
  - PackageReference: `NetArchTest.Rules` (1.3.2), `xunit`, `Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`, `coverlet.collector`
  - ProjectReference: `src/Pricing.Api/Pricing.Api.csproj` (transitive — ładuje wszystkie assemblies)

### Grupy reguł

**A — Layer Rules** (`ArchitectureTests/LayerRulesTests.cs`)
- `Domain` nie może zależeć od `Application`, `Infrastructure`, `Api`
- `Application` nie może zależeć od `Infrastructure`, `Api`
- `Contracts` nie ma żadnych ProjectReference (czyste rekordy)
- `Facade` może zależeć wyłącznie od `Shared.Domain`

**B — Module Boundary Rules** (`ArchitectureTests/ModuleBoundaryTests.cs`)
- `Inventory.Domain` nie referuje `Import.*` ani `Rating.*`
- `Import.Domain` nie referuje `Inventory.*` ani `Rating.*`
- `Rating.Domain` nie referuje `Inventory.*` ani `Import.*`
- Analogicznie dla warstwy `Application` każdego modułu
- Komunikacja cross-modułowa tylko przez `*.Facade` (w Application dozwolone `*.Facade.*`)

**C — Naming Convention Rules** (`ArchitectureTests/NamingConventionTests.cs`)
- Klasy kończące się na `UseCase` muszą być `sealed`
- Interfejsy `I*Repository` mogą istnieć tylko w namespace `*.Domain.*`
- Klasy kończące się na `Endpoint` muszą dziedziczyć po `Endpoint<,>` (FastEndpoints)
- Klasy kończące się na `Configuration` w namespace `*.Infrastructure.*` muszą implementować `IEntityTypeConfiguration<>`

**D — Domain Purity Rules** (`ArchitectureTests/DomainPurityTests.cs`)
- Typy w `*.Domain.*` nie mogą zależeć od `Microsoft.EntityFrameworkCore`
- Typy w `*.Domain.*` nie mogą zależeć od `Microsoft.AspNetCore.*`
- Typy w `*.Application.*` nie mogą zależeć od `Microsoft.EntityFrameworkCore` (wyjątek: DbContext przez UnitOfWork jest w Infrastructure)
- Typy w `*.Application.*` nie mogą zależeć od `Microsoft.AspNetCore.*`

## Acceptance Criteria
- [ ] Projekt `Pricing.ArchitectureTests` buduje się bez błędów (`dotnet build`)
- [ ] Wszystkie testy przechodzą na zielonej gałęzi (`dotnet test`)
- [ ] Naruszenie reguły warstwy (np. Domain referująca Infrastructure) powoduje failujący test
- [ ] Naruszenie izolacji modułów (np. Inventory.Domain → Import.Domain) powoduje failujący test
- [ ] Klasa `UseCase` bez `sealed` powoduje failujący test
- [ ] Typ domenowy zależny od EF Core powoduje failujący test
- [ ] Projekt dodany do `Pricing.slnx`

## Implementation Checklist
- [x] Utwórz `tests/Pricing.ArchitectureTests/Pricing.ArchitectureTests.csproj`
- [x] Dodaj projekt do `Pricing.slnx`
- [x] `LayerRulesTests.cs` — reguły A
- [x] `ModuleBoundaryTests.cs` — reguły B
- [x] `NamingConventionTests.cs` — reguły C
- [x] `DomainPurityTests.cs` — reguły D
- [x] `dotnet test` — wszystkie testy zielone

## Open Questions
- Czy `*.Facade` może zależeć wyłącznie od `Shared.Domain`, czy dopuszczamy też `Shared.Contracts`?

## Technical Notes
- NetArchTest operuje na załadowanych assembly — `Pricing.Api` jako transitive entry point
  jest wystarczające, by refleksja widziała wszystkie typy produkcyjne.
- Reguły namespace-based (nie project-based) — konwencja `Pricing.<Module>.<Layer>.*` musi być
  konsekwentnie stosowana, żeby reguły działały poprawnie.
