# Pasjans Klondike (C# Console) – Projekt konkursowy Gigathon🃏

Konsolowa implementacja klasycznego **Pasjansa Klondike** napisana w języku **C#**.  
Projekt powstał na potrzeby konkursu **Gigathon**, skupiając się na logice gry, poprawności zasad, obsłudze wejścia użytkownika i estetycznym wyświetlaniu kart w konsoli (również w kolorze).

---

## 🎮 Opis gry

Gra odwzorowuje klasyczne zasady Klondike:

- 7 kolumn kart (tableau), każda z rosnącą liczbą zakrytych kart — ostatnia zawsze odkryta.
- Talia dobierania (draw pile) — zależnie od trybu trudności:
  - **Easy** — dobieranie po 1 karcie.
  - **Hard** — dobieranie po 3 karty.
- Stos kart odrzuconych (waste).
- 4 stosy wyjściowe (foundation) po jednym na każdy kolor ♥ ◆ ♣ ♠.
- Możliwość wykonywania wszystkich standardowych ruchów:
  - przenoszenie sekwencji kart między kolumnami,
  - przenoszenie ze stosu dobierania,
  - przenoszenie do *foundation* i cofanie tych ruchów.
- Zapis / wczytywanie wyniku gracza do rankingu.

---

## ✨ Funkcjonalności

### 🃏 Logika gry
- Pełna obsługa zasad Klondike:
  - kolory naprzemienne (czerwony/czarny),
  - wartości w dół na tableau (np. 10 → 9 → 8),
  - wartości w górę na foundation (A → 2 → 3 → … → K),
  - król może być położony tylko na pustą kolumnę.
- Walidacja każdego ruchu (gry nie da się “zepsuć”).

### 🎨 Kolorowe wyświetlanie kart
- ♥ ◆ wyświetlane jako **czerwone**,  
- ♠ ♣ jako **cyjanowe**,  
- zakryte karty jako `[XX]` w kolorze szarym.

### 🗂️ System cofania ruchów (undo)
- Przechowywanie pełnego stanu gry (głębokie kopiowanie).
- Możliwość przywrócenia dowolnego zapisanego stanu.

### 🏆 System rankingu
- Klasa `ScoreEntry` przechowująca:
  - nazwę gracza,
  - liczbę wykonanych ruchów,
  - poziom trudności,
  - datę wyniku.
- Automatyczne zapisywanie do pliku `scores.txt`.
- Sortowanie według najmniejszej liczby ruchów.
- Wyświetlanie TOP 10 najlepszych wyników w estetycznej tabeli.

### 🎚️ Poziomy trudności
- **Easy (1 karta)**
- **Hard (3 karty)**

---

## 📂 Struktura projektu (najważniejsze klasy)

### 🔹 `Card`
Reprezentacja pojedynczej karty (wartość, kolor, stan odkrycia) + funkcja kolorowego wypisywania.

### 🔹 `Tableau`
Logika 7 kolumn, przenoszenie kart, walidacja ruchów, wyświetlanie kart.

### 🔹 `DrawPile`
Zarządza talią dobierania zależnie od trudności.

### 🔹 `Waste`
Obsługa stosu kart odrzuconych.

### 🔹 `Foundation`
Obsługa 4 stosów koloru i weryfikacja czy karta może być dodana/usunięta.

### 🔹 `ScoreManager`
Zapis/odczyt rankingu, parsowanie pliku, generowanie tabeli wyników.

### 🔹 `GameState`
System cofania ruchów (przechowywanie pełnej kopii układu gry).
