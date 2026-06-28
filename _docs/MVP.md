# Cennik — MVP

---

## Główny problem

Firmy (konteksty) generują cenniki ręcznie z Exceli — proces podatny na błędy, niestandaryzowany i nieautomatyczny. System ma pobierać dane od partnerów, automatycznie generować ceny według algorytmu i serwować je jako REST API dla frontów sklepów.

---

## Najmniejszy zestaw funkcjonalności (MVP)

### 1. Katalog urządzeń
- Urządzenie posiada klasyfikację: `type / subtype / range`
- Cena bazowa urządzenia pochodzi od partnera (ingestion)
- Bundle: urządzenie + N akcesoriów (powiązanie wiele-do-wielu)

### 2. Zarządzanie kontekstem
- Kontekst = kraj + firma (np. MediaMarktES, CzechPartnerXYZ)
- Per kontekst: waluta, tryb (wynajem / sprzedaż / oba), lista okresów wynajmu (np. 6, 12, 24, 36 mies.)
- Konteksty są niezależne — brak dziedziczenia

### 3. Ingestion danych od partnera
- Konfigurowalne źródło danych per kontekst: URL do pliku lub endpoint API
- Mapowanie pól źródłowych na model domeny (format różni się per partner)
- Scheduled refresh per kontekst (konfigurowalny cron)
- **MVP: obsługa 2 formatów — JSON REST API + CSV/XLSX**

### 4. Algorytm generowania cen
- Wzór MVP: `cena_końcowa = cena_od_partnera × mnożnik + marża`
- Parametry algorytmu (`mnożnik`, `marża`) są konfigurowane per `type / subtype / range`
- Cena generowana per urządzenie × per kontekst × per okres wynajmu (dla trybu wynajem)
- Trigger generowania: automatyczny po zakończeniu ingestii + ręczny przez API

### 5. API cennika (output dla front-of-store)
- REST JSON, bez autentykacji (open na MVP)
- Zwraca cenę urządzenia lub bundla per kontekst i opcjonalnie per okres wynajmu

### 6. Backoffice
- Na MVP: tylko API + Swagger (brak UI)
- Operacje dostępne przez API: CRUD kontekstów, konfiguracja źródeł, parametry algorytmu, ręczny trigger ingestii i regeneracji cen, podgląd wygenerowanych cen

---

## Co NIE wchodzi w zakres MVP

- Złożony algorytm wyceny (wielopoziomowe reguły, warunki warunkowe)
- Obsługa więcej niż 2 formatów źródłowych
- Role i uprawnienia użytkowników / autentykacja API
- Historia zmian cen i audyt
- Notyfikacje o błędach ingestii (poza standardowym logowaniem)
- Backoffice UI (Blazor)
- Dziedziczenie / hierarchia kontekstów

---

## Kryteria sukcesu

MVP uznaje się za udane, jeśli:

- System pobiera dane od co najmniej 2 partnerów w różnych formatach bez ręcznej interwencji
- Ceny są generowane automatycznie po każdej ingestii dla wszystkich urządzeń w kontekście
- API cennika zwraca poprawne ceny per urządzenie/bundle, kontekst i okres wynajmu
- Backoffice może skonfigurować nowy kontekst i źródło danych wyłącznie przez API

### Soft KPI
- Czas konfiguracji nowego kontekstu < 30 minut (vs. dzień roboczy z Excelem)
