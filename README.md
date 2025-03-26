# Budżet Domowy - Aplikacja do Zarządzania Finansami

## Autorzy
- Maria Mrozek - 322956
- Michał Kuchnicki - 317129

## Opis Projektu
Budżet Domowy to rozbudowana aplikacja umożliwiająca efektywne zarządzanie finansami osobistymi. Użytkownicy mogą śledzić swoje wydatki i przychody, planować budżet, generować raporty oraz analizować swoje finanse za pomocą zaawansowanych narzędzi analitycznych.

## Funkcje

### Podstawowe funkcje
- **Zarządzanie transakcjami**: dodawanie, edycja i usuwanie wydatków oraz przychodów.
- **Kategorie transakcji**: domyślne (np. jedzenie, rachunki, rozrywka) oraz możliwość tworzenia własnych kategorii.
- **Tworzenie budżetów**: określanie limitów dla poszczególnych kategorii i monitorowanie ich realizacji.
- **Generowanie raportów i wykresów**: analiza miesięcznych i rocznych wydatków, zestawienia salda oraz przepływów pieniężnych.
- **Eksport danych**: zapis transakcji do plików CSV oraz PDF.

### Zaawansowane funkcje
- **Planowanie przyszłych wydatków**: zaplanowane płatności oraz powiadomienia o nadchodzących rachunkach.
- **Obsługa wielu walut**: automatyczne pobieranie kursów walut z API.
- **System powiadomień**: alerty o przekroczeniu budżetu oraz sugestie oszczędności.
- **Lokalne przechowywanie danych**: możliwość wyboru SQLite lub JSON jako metody zapisu danych.

### Dodatkowe funkcje
- **AI do analizy wydatków**: identyfikacja trendów oraz proponowanie zmian w budżecie.
- **OCR dla paragonów**: automatyczne dodawanie transakcji na podstawie zeskanowanych paragonów.

## Struktura Projektu

### Warstwa Modelu (Logika Biznesowa)
- `Transaction` - ID, data, kwota, kategoria, opis, cykliczność.
- `Category` - ID, nazwa, typ.
- `Budget` - ID, miesiąc, limit, suma wydatków.
- `Report` - generowanie wykresów, eksport do CSV/PDF.
- `Currency` - symbol, kod waluty, kurs wymiany.
- `ExchangeRateAPI` - pobieranie kursów walut.
- `AIAnalyzer` - analiza finansowa i propozycje oszczędności.
- `OCRProcessor` - skanowanie i rozpoznawanie paragonów.

### Warstwa Dostępu do Danych (Persistence)
- `DatabaseManager` - obsługa SQLite/JSON.
- `FileExporter` - eksport danych do CSV/PDF.
- `SettingsManager` - konfiguracja użytkownika.

### Warstwa Logiki Aplikacji (Usługi)
- `TransactionService` - zarządzanie transakcjami.
- `BudgetService` - obsługa budżetów.
- `NotificationService` - powiadomienia.
- `ExchangeRateService` - pobieranie kursów walut.

### Warstwa Prezentacji (UI)
- `UIManager` - zarządzanie interfejsem.
- `MainMenu` - menu główne.
- `TransactionView` - zarządzanie transakcjami.
- `BudgetView` - zarządzanie budżetem.
- `ReportView` - generowanie raportów.
- `SettingsView` - ustawienia aplikacji.
