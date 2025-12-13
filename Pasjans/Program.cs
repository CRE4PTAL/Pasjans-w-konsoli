using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pasjans
{
    // Określa poziom trudności gry (łatwy - 1 karta, trudny - 3 karty)
    public enum Difficulty
    {
        Easy = 1,    // Łatwy - dobieranie po 1 karcie
        Hard = 3     // Trudny - dobieranie po 3 karty
    }

    // Reprezentuje pojedynczy wpis w rankingu najlepszych wyników
    public class ScoreEntry
    {
        // Nazwa gracza (maksymalnie 15 znaków)
        public string PlayerName { get; set; }

        // Liczba ruchów wykonanych w grze
        public int Moves { get; set; }

        // Poziom trudności gry (Easy/Hard)
        public Difficulty Difficulty { get; set; }

        // Data i czas uzyskania wyniku
        public DateTime Date { get; set; }

        /// <summary>
        /// Konwertuje wpis rankingu na sformatowany string
        /// </summary>
        /// <returns>
        /// Sformatowany string w postaci:
        /// "NazwaGracza     Ruchy Poziom     Data"
        /// </returns>
        public override string ToString()
        {
            // Formatowanie: 
            // - Nazwa gracza wyrównana do lewej (15 znaków)
            // - Liczba ruchów wyrównana do prawej (5 znaków)
            // - Poziom trudności (10 znaków)
            // - Data w formacie YYYY-MM-DD
            return $"{PlayerName.PadRight(15)} {Moves.ToString().PadLeft(5)} {Difficulty.ToString().PadRight(10)} {Date:yyyy-MM-dd}";
        }
    }

    // Zarządza rankingiem wyników (zapisywanie, wczytywanie, wyświetlanie)
    public class ScoreManager
    {
        private const string ScoresFile = "scores.txt";
        private List<ScoreEntry> scores = new List<ScoreEntry>();

        public ScoreManager()
        {
            LoadScores();
        }

        // Wczytaj wyniki z pliku
        private void LoadScores()
        {
            if (!File.Exists(ScoresFile)) return;

            foreach (var line in File.ReadAllLines(ScoresFile))
            {
                var parts = line.Split('|');
                if (parts.Length < 4) continue;

                scores.Add(new ScoreEntry
                {
                    PlayerName = parts[0],
                    Moves = int.Parse(parts[1]),
                    Difficulty = (Difficulty)Enum.Parse(typeof(Difficulty), parts[2]),
                    Date = DateTime.Parse(parts[3])
                });
            }
        }

        // Zapisz nowy wynik
        public void AddScore(ScoreEntry entry)
        {
            scores.Add(entry);
            SaveScores();
        }

        // Zapisz wyniki do pliku
        private void SaveScores()
        {
            var lines = scores.Select(s => $"{s.PlayerName}|{s.Moves}|{s.Difficulty}|{s.Date:o}");
            File.WriteAllLines(ScoresFile, lines);
        }

        // Pobierz ranking posortowany po liczbie ruchów (rosnąco)
        public List<ScoreEntry> GetTopScores(int count = 10)
        {
            return scores
                .OrderBy(s => s.Moves)
                .ThenByDescending(s => s.Date)
                .Take(count)
                .ToList();
        }

        // Wyświetl ranking
        public void DisplayTopScores()
        {
            var topScores = GetTopScores(10);

            if (topScores.Count == 0)
            {
                Console.WriteLine("Brak wyników w rankingu.");
                return;
            }

            Console.WriteLine("========== TOP 10 RANKING ==========");
            Console.WriteLine("Pozycja  Gracz          Ruchy  Poziom     Data");
            Console.WriteLine("-------------------------------------");

            for (int i = 0; i < topScores.Count; i++)
            {
                Console.WriteLine($"#{i + 1,-6} {topScores[i]}");
            }
        }
    }

    // Reprezentuje pojedynczą kartę do gry z kolorem i wartością
    public class Card
    {
        public string Value { get; set; }
        public string Suit { get; set; }
        public bool IsFaceUp { get; set; }

        public override string ToString()
            => IsFaceUp ? $"{Value}{Suit}" : "[XX]";

        // Dodaj metodę do kolorowego wypisywania
        public void WriteColored()
        {
            if (!IsFaceUp)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write("[XX]");
            }
            else if (Suit == "♥" || Suit == "◆")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"{Value}{Suit}");
            }
            else // ♠ lub ♣
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{Value}{Suit}");
            }
            Console.ResetColor();
        }
    }

    // Zarządza główną planszą gry (7 kolumn kart)
    class Tableau
    {
        // Lista kolumn kart (każda kolumna to lista kart)
        public List<List<Card>> Columns { get; private set; } = new List<List<Card>>();

        // Konstruktor - inicjalizuje planszę z podanymi kartami
        public Tableau(List<Card> cards) => SetCards(cards);

        // Rozdaje karty do kolumn zgodnie z zasadami pasjansa
        private void SetCards(List<Card> deck)
        {
            int index = 0;
            // Tworzy 7 kolumn o rosnącej liczbie kart
            for (int i = 1; i <= 7; i++)
            {
                var column = new List<Card>();
                for (int j = 0; j < i; j++)
                {
                    var card = deck[index++];
                    // Tylko ostatnia karta w kolumnie jest odkryta
                    card.IsFaceUp = (j == i - 1);
                    column.Add(card);
                }
                Columns.Add(column);
            }
        }

        // Wyświetla wszystkie kolumny z kartami (z kolorami)
        public void ShowCards()
        {
            for (int i = 0; i < Columns.Count; i++)
            {
                Console.Write($"Kolumna {i + 1}: ");
                foreach (var c in Columns[i])
                {
                    c.WriteColored();
                    Console.Write(" ");
                }
                Console.WriteLine();
            }
        }

        // Przenosi sekwencję kart między kolumnami
        public void MoveCards()
        {
            // Pobierz dane od użytkownika
            Console.Write("Z której kolumny chcesz przenieść kartę: ");
            int from = int.Parse(Console.ReadLine()) - 1;
            Console.Write("Do której kolumny chcesz przenieść kartę: ");
            int to = int.Parse(Console.ReadLine()) - 1;
            Console.Write("Ile chcesz przenieść kart?: ");
            int amount = int.Parse(Console.ReadLine());

            var src = Columns[from];
            var dst = Columns[to];

            // Sprawdź czy ruch jest możliwy
            int idx = src.FindIndex(c => c.IsFaceUp);
            if (idx < 0 || src.Count - idx < amount)
            {
                Console.WriteLine("Nieprawidłowa liczba kart."); return;
            }

            // Pobierz sekwencję kart do przeniesienia
            var seq = src.GetRange(src.Count - amount, amount);
            var topSrc = seq[0];
            var topDst = dst.Count > 0 ? dst.Last() : null;

            // Walidacja zasad pasjansa
            if (topDst != null)
            {
                if (!IsOpposite(topDst, topSrc) || !IsOneLower(topDst, topSrc))
                {
                    Console.WriteLine("Ruch niemożliwy."); return;
                }
            }
            else if (topSrc.Value != "K") // Na pustą kolumnę tylko Król
            {
                Console.WriteLine("Na pustą kolumnę tylko Król."); return;
            }

            // Wykonaj przeniesienie
            src.RemoveRange(src.Count - amount, amount);
            dst.AddRange(seq);

            // Odkryj nową kartę w kolumnie źródłowej
            if (src.Count > 0) src[src.Count - 1].IsFaceUp = true;
        }

        // Przenosi kartę z waste na wskazaną kolumnę
        public void MoveFromWaste(Waste waste)
        {
            if (!waste.HasCard()) { Console.WriteLine("Waste pusty."); return; }

            Console.Write("Na kolumnę (1-7): ");
            int to = int.Parse(Console.ReadLine()) - 1;
            var dst = Columns[to];
            var card = waste.Peek();

            // Walidacja ruchu
            var topDst = dst.Count > 0 ? dst.Last() : null;
            if (topDst != null)
            {
                if (!IsOpposite(topDst, card) || !IsOneLower(topDst, card))
                { Console.WriteLine("Nieprawidłowy ruch."); return; }
            }
            else if (card.Value != "K") // Na pustą kolumnę tylko Król
            { Console.WriteLine("Na pustą kolumnę tylko Król."); return; }

            // Wykonaj przeniesienie
            waste.RemoveTop();
            dst.Add(card);
        }

        // Przenosi kartę z kolumny na foundation
        public void MoveToFoundation(Foundation foundation)
        {
            Console.Write("Z której kolumny (1-7): ");
            int from = int.Parse(Console.ReadLine()) - 1;

            // Walidacja wejścia
            if (from < 0 || from >= Columns.Count || Columns[from].Count == 0)
            {
                Console.WriteLine("Nieprawidłowa kolumna lub kolumna pusta.");
                return;
            }

            var card = Columns[from].Last();
            if (!card.IsFaceUp)
            {
                Console.WriteLine("Karta musi być odkryta.");
                return;
            }

            if (foundation.CanAdd(card))
            {
                // Wykonaj przeniesienie
                foundation.AddCard(card);
                Columns[from].RemoveAt(Columns[from].Count - 1);

                // Odkryj następną kartę w kolumnie
                if (Columns[from].Count > 0)
                    Columns[from].Last().IsFaceUp = true;
            }
            else
            {
                Console.WriteLine("Nie można przenieść tej karty do foundation.");
            }
        }

        // Przenosi kartę z foundation na kolumnę
        public void MoveFromFoundation(Foundation foundation)
        {
            Console.Write("Z którego foundation (♥/◆/♣/♠): ");
            string suit = Console.ReadLine();
            Console.Write("Na którą kolumnę (1-7): ");
            int to = int.Parse(Console.ReadLine()) - 1;

            // Walidacja wejścia
            if (to < 0 || to >= Columns.Count)
            {
                Console.WriteLine("Nieprawidłowa kolumna.");
                return;
            }

            var card = foundation.GetTopCard(suit);
            if (card == null)
            {
                Console.WriteLine("Brak karty w tym foundation.");
                return;
            }

            var dst = Columns[to];
            var topDst = dst.Count > 0 ? dst.Last() : null;

            // Walidacja ruchu
            if (topDst != null)
            {
                if (!IsOpposite(topDst, card) || !IsOneLower(topDst, card))
                {
                    Console.WriteLine("Nieprawidłowy ruch.");
                    return;
                }
            }
            else if (card.Value != "K") // Na pustą kolumnę tylko Król
            {
                Console.WriteLine("Na pustą kolumnę tylko Król.");
                return;
            }

            // Wykonaj przeniesienie
            foundation.RemoveTopCard(suit);
            dst.Add(card);
        }

        // Sprawdza czy karty są przeciwnego koloru (czerwona/czarna)
        private bool IsOpposite(Card a, Card b)
            => (a.Suit == "♥" || a.Suit == "◆") != (b.Suit == "♥" || b.Suit == "◆");

        // Sprawdza czy karta 'higher' jest o 1 wyższa niż 'lower' (np. 10>9)
        private bool IsOneLower(Card higher, Card lower)
        {
            string[] order = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
            return Array.IndexOf(order, lower.Value) + 1 == Array.IndexOf(order, higher.Value);
        }
    }

    // Przechowuje stan gry umożliwiając cofanie ruchów
    class GameState
    {
        public List<List<Card>> TableauColumns { get; }
        public List<Card> DrawPileDeck { get; }
        public List<Card> WasteCards { get; }
        public Dictionary<string, List<Card>> FoundationPiles { get; }

        public GameState(Tableau tableau, DrawPile drawPile, Waste waste, Foundation foundation)
        {
            // Głębokie kopiowanie
            TableauColumns = tableau.Columns.Select(col => col.Select(c => new Card { Value = c.Value, Suit = c.Suit, IsFaceUp = c.IsFaceUp }).ToList()).ToList();
            DrawPileDeck = drawPile.GetDeck().Select(c => new Card { Value = c.Value, Suit = c.Suit, IsFaceUp = c.IsFaceUp }).ToList();
            WasteCards = waste.GetCards().Select(c => new Card { Value = c.Value, Suit = c.Suit, IsFaceUp = c.IsFaceUp }).ToList();
            FoundationPiles = foundation.GetAllPiles().ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Select(c => new Card { Value = c.Value, Suit = c.Suit, IsFaceUp = c.IsFaceUp }).ToList()
            );
        }

        // Przywracanie stanu gry
        public void Restore(Tableau tableau, DrawPile drawPile, Waste waste, Foundation foundation)
        {
            // Przywróć kolumny
            for (int i = 0; i < tableau.Columns.Count; i++)
            {
                tableau.Columns[i].Clear();
                tableau.Columns[i].AddRange(TableauColumns[i].Select(c => new Card { Value = c.Value, Suit = c.Suit, IsFaceUp = c.IsFaceUp }));
            }
            // Przywróć draw pile
            drawPile.SetDeck(DrawPileDeck.Select(c => new Card { Value = c.Value, Suit = c.Suit, IsFaceUp = c.IsFaceUp }).ToList());
            // Przywróć waste
            waste.SetCards(WasteCards.Select(c => new Card { Value = c.Value, Suit = c.Suit, IsFaceUp = c.IsFaceUp }).ToList());
            // Przywróć foundation
            foundation.SetPiles(FoundationPiles.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Select(c => new Card { Value = c.Value, Suit = c.Suit, IsFaceUp = c.IsFaceUp }).ToList()
            ));
        }
    }

    // Reprezentuje talię kart, z której gracz dobiera karty
    class DrawPile
    {
        private List<Card> deck;
        private static Random rnd = new Random();
        private Difficulty difficulty;

        public DrawPile(List<Card> cards, Difficulty diff)
        {
            deck = new List<Card>(cards);
            difficulty = diff;
        }

        public List<Card> GetDeck() => deck.Select(c => new Card { Value = c.Value, Suit = c.Suit, IsFaceUp = c.IsFaceUp }).ToList();
        public void SetDeck(List<Card> cards)
        {
            deck = cards.Select(c => new Card { Value = c.Value, Suit = c.Suit, IsFaceUp = c.IsFaceUp }).ToList();
        }

        // Dobieranie kart zgodnie z poziomem trudności
        public List<Card> DrawCards()
        {
            int cardsToDraw = (int)difficulty;
            var drawnCards = new List<Card>();

            for (int i = 0; i < cardsToDraw && deck.Count > 0; i++)
            {
                var card = deck.Last();
                deck.RemoveAt(deck.Count - 1);
                card.IsFaceUp = true;
                drawnCards.Add(card);
            }

            return drawnCards;
        }

        // Przetasuj karty przed dodaniem do draw pile
        public void Recycle(IEnumerable<Card> cards)
        {
            var list = cards.ToList();
            // Tasowanie Fisher-Yates
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                var tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
            foreach (var c in list)
            {
                c.IsFaceUp = false;
                deck.Add(c);
            }
        }

        public bool HasCards() => deck.Count > 0;
        public int Count => deck.Count;
        public Difficulty GetDifficulty() => difficulty;
    }

    // Zarządza stosem kart odrzuconych (waste)
    class Waste
    {
        private List<Card> cards = new List<Card>();
        private Difficulty difficulty;

        public Waste(Difficulty diff)
        {
            difficulty = diff;
        }

        public List<Card> GetCards() => cards.Select(c => new Card { Value = c.Value, Suit = c.Suit, IsFaceUp = c.IsFaceUp }).ToList();
        public void SetCards(List<Card> newCards)
        {
            cards = newCards.Select(c => new Card { Value = c.Value, Suit = c.Suit, IsFaceUp = c.IsFaceUp }).ToList();
        }

        // Dodawanie kart zgodnie z poziomem trudności
        public void AddCards(List<Card> newCards)
        {
            cards.AddRange(newCards);
        }

        public bool HasCard() => cards.Count > 0;

        // Peek pokazuje tylko dostępną kartę
        public Card Peek()
        {
            if (!HasCard()) return null;

            if (difficulty == Difficulty.Easy)
            {
                // Na łatwym poziomie - zawsze ostatnia karta
                return cards.Last();
            }
            else
            {
                // Na trudnym poziomie - ostatnia karta z grupy 3
                return cards.Last();
            }
        }

        public void RemoveTop()
        {
            if (HasCard())
                cards.RemoveAt(cards.Count - 1);
        }

        // Wyświetlanie zgodnie z poziomem trudności
        public void Show()
        {
            Console.Write("Waste: ");

            if (cards.Count == 0)
            {
                Console.WriteLine("[]");
                return;
            }

            if (difficulty == Difficulty.Easy)
            {
                // Poziom łatwy - pokazuj tylko ostatnią kartę
                cards.Last().WriteColored();
            }
            else
            {
                // Poziom trudny - pokazuj do 3 ostatnich kart, ale tylko ostatnia jest dostępna
                int startIndex = Math.Max(0, cards.Count - 3);
                for (int i = startIndex; i < cards.Count; i++)
                {
                    if (i == cards.Count - 1)
                    {
                        // Ostatnia karta - dostępna (normalne kolory)
                        cards[i].WriteColored();
                    }
                    else
                    {
                        // Karty widoczne ale niedostępne (szare)
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write($"{cards[i].Value}{cards[i].Suit}");
                        Console.ResetColor();
                    }
                    Console.Write(" ");
                }
            }
            Console.WriteLine();
        }

        public List<Card> EmptyAll()
        {
            var tmp = new List<Card>(cards);
            cards.Clear();
            return tmp;
        }

        // Przenoszenie z waste do foundation
        public void MoveToFoundation(Foundation foundation)
        {
            if (!HasCard())
            {
                Console.WriteLine("Waste pusty.");
                return;
            }

            var card = Peek();
            if (foundation.CanAdd(card))
            {
                foundation.AddCard(card);
                RemoveTop();
                Console.WriteLine($"Przeniesiono {card} do foundation.");
            }
            else
            {
                Console.WriteLine("Nie można przenieść tej karty do foundation.");
            }
        }
    }

    // Reprezentuje 4 stosy końcowe (dla każdego koloru)
    class Foundation
    {
        // Słownik przechowujący stosy kart (klucz: kolor, wartość: lista kart)
        private Dictionary<string, List<Card>> piles = new Dictionary<string, List<Card>>();

        // Konstruktor - inicjalizuje puste stosy dla każdego koloru
        public Foundation()
        {
            piles["♥"] = new List<Card>();
            piles["◆"] = new List<Card>();
            piles["♣"] = new List<Card>();
            piles["♠"] = new List<Card>();
        }

        // Sprawdza czy można dodać kartę do odpowiedniego stosu
        public bool CanAdd(Card card)
        {
            var pile = piles[card.Suit];

            // Jeśli stos jest pusty, można dodać tylko Asa
            if (pile.Count == 0)
                return card.Value == "A";

            // Sprawdź czy karta jest następną w kolejności
            var topCard = pile.Last();
            return IsOneHigher(card, topCard);
        }

        // Zwraca kopię wszystkich stosów (używane przy cofaniu ruchów)
        public Dictionary<string, List<Card>> GetAllPiles()
        {
            return piles.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Select(c => new Card { Value = c.Value, Suit = c.Suit, IsFaceUp = c.IsFaceUp }).ToList()
            );
        }

        // Ustawia stan stosów (używane przy cofaniu ruchów)
        public void SetPiles(Dictionary<string, List<Card>> newPiles)
        {
            foreach (var suit in piles.Keys.ToList())
            {
                piles[suit] = newPiles[suit].Select(c => new Card { Value = c.Value, Suit = c.Suit, IsFaceUp = c.IsFaceUp }).ToList();
            }
        }

        // Dodaje kartę do odpowiedniego stosu (jeśli jest to dozwolone)
        public void AddCard(Card card)
        {
            if (CanAdd(card))
                piles[card.Suit].Add(card);
        }

        // Pobiera wierzchnią kartę z danego stosu
        public Card GetTopCard(string suit)
        {
            if (piles.ContainsKey(suit) && piles[suit].Count > 0)
                return piles[suit].Last();
            return null;
        }

        // Usuwa wierzchnią kartę z danego stosu
        public void RemoveTopCard(string suit)
        {
            if (piles.ContainsKey(suit) && piles[suit].Count > 0)
                piles[suit].RemoveAt(piles[suit].Count - 1);
        }

        // Wyświetla aktualny stan foundation z kolorowym formatowaniem
        public void Show()
        {
            Console.WriteLine("Foundation:");
            foreach (var kvp in piles)
            {
                Console.Write($"{kvp.Key}: ");
                if (kvp.Value.Count > 0)
                {
                    // Wyświetl wierzchnią kartę z kolorami
                    kvp.Value.Last().WriteColored();
                    Console.Write(" ");
                }
                else
                {
                    // Pokaż puste miejsce
                    Console.Write("[] ");
                }
            }
            Console.WriteLine();
        }

        // Sprawdza czy gra jest ukończona (wszystkie stosy pełne)
        public bool IsComplete()
        {
            return piles.Values.All(pile => pile.Count == 13);
        }

        // Zwraca wszystkie karty ze wszystkich stosów (używane przy sprawdzaniu przegranej)
        public List<Card> GetAllCards()
        {
            var allCards = new List<Card>();
            foreach (var pile in piles.Values)
                allCards.AddRange(pile);
            return allCards;
        }

        // Sprawdza czy karta 'higher' jest o 1 wyższa od 'lower' w hierarchii
        private bool IsOneHigher(Card higher, Card lower)
        {
            string[] order = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
            return Array.IndexOf(order, higher.Value) == Array.IndexOf(order, lower.Value) + 1;
        }
    }

    // Sprawdza stan gry pod kątem możliwych ruchów i określa czy gra jest przegrana
    class GameChecker
    {
        // Sprawdza czy gracz ma jeszcze jakiekolwiek dostępne ruchy
        public static bool IsGameLost(Tableau tableau, DrawPile drawPile, Waste waste, Foundation foundation)
        {
            // 1. Sprawdź możliwość przeniesienia karty z waste
            if (waste.HasCard())
            {
                var wasteCard = waste.Peek();

                // Czy można przenieść z waste do foundation?
                if (foundation.CanAdd(wasteCard))
                    return false;

                // Czy można przenieść z waste do kolumny?
                if (CanMoveWasteToTableau(wasteCard, tableau))
                    return false;
            }

            // 2. Sprawdź możliwość przeniesienia kart między kolumnami
            if (CanMoveWithinTableau(tableau))
                return false;

            // 3. Sprawdź możliwość przeniesienia kart z kolumn do foundation
            if (CanMoveTableauToFoundation(tableau, foundation))
                return false;

            // 4. Sprawdź możliwość przeniesienia kart z foundation do kolumn
            if (CanMoveFoundationToTableau(foundation, tableau))
                return false;

            // 5. Sprawdź czy można dobierać karty z talii
            if (HasDrawPileCards(drawPile, waste))
                return false;

            // Jeśli żaden ruch nie jest możliwy - gra przegrana
            return true;
        }

        // Sprawdza czy kartę z waste można przenieść na którąkolwiek kolumnę
        private static bool CanMoveWasteToTableau(Card wasteCard, Tableau tableau)
        {
            foreach (var column in tableau.Columns)
            {
                // Na pustą kolumnę można przenieść tylko króla
                if (column.Count == 0 && wasteCard.Value == "K")
                    return true;

                if (column.Count > 0)
                {
                    var topCard = column.Last();
                    // Sprawdź zgodność koloru i wartości
                    if (IsOpposite(topCard, wasteCard) && IsOneLower(topCard, wasteCard))
                        return true;
                }
            }
            return false;
        }

        // Sprawdza czy istnieje jakikolwiek możliwy ruch między kolumnami
        private static bool CanMoveWithinTableau(Tableau tableau)
        {
            for (int i = 0; i < tableau.Columns.Count; i++)
            {
                var sourceColumn = tableau.Columns[i];
                if (sourceColumn.Count == 0) continue;

                // Znajdź pierwszą odkrytą kartę w kolumnie
                int firstFaceUpIndex = sourceColumn.FindIndex(card => card.IsFaceUp);
                if (firstFaceUpIndex == -1) continue;  // Brak odkrytych kart

                // Sprawdź wszystkie możliwe sekwencje zaczynając od odkrytych kart
                for (int startIdx = firstFaceUpIndex; startIdx < sourceColumn.Count; startIdx++)
                {
                    var cardToMove = sourceColumn[startIdx];

                    // Sprawdź wszystkie kolumny docelowe
                    for (int j = 0; j < tableau.Columns.Count; j++)
                    {
                        if (i == j) continue;  // Pomijaj tę samą kolumnę

                        var targetColumn = tableau.Columns[j];

                        // Na pustą kolumnę można przenieść tylko króla
                        if (targetColumn.Count == 0 && cardToMove.Value == "K")
                            return true;

                        if (targetColumn.Count > 0)
                        {
                            var topCard = targetColumn.Last();
                            // Sprawdź zgodność koloru i wartości
                            if (IsOpposite(topCard, cardToMove) && IsOneLower(topCard, cardToMove))
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        // Sprawdza czy można przenieść którąkolwiek wierzchnią kartę z kolumn do foundation
        private static bool CanMoveTableauToFoundation(Tableau tableau, Foundation foundation)
        {
            foreach (var column in tableau.Columns)
            {
                if (column.Count > 0)
                {
                    var topCard = column.Last();
                    // Karta musi być odkryta i pasować do foundation
                    if (topCard.IsFaceUp && foundation.CanAdd(topCard))
                        return true;
                }
            }
            return false;
        }

        // Sprawdza czy można przenieść którąkolwiek kartę z foundation na kolumny
        private static bool CanMoveFoundationToTableau(Foundation foundation, Tableau tableau)
        {
            string[] suits = { "♥", "◆", "♣", "♠" };

            foreach (var suit in suits)
            {
                var topCard = foundation.GetTopCard(suit);
                if (topCard == null) continue;  // Brak kart w tym foundation

                foreach (var column in tableau.Columns)
                {
                    // Na pustą kolumnę można przenieść tylko króla
                    if (column.Count == 0 && topCard.Value == "K")
                        return true;

                    if (column.Count > 0)
                    {
                        var columnTop = column.Last();
                        // Sprawdź zgodność koloru i wartości
                        if (IsOpposite(columnTop, topCard) && IsOneLower(columnTop, topCard))
                            return true;
                    }
                }
            }
            return false;
        }

        // Sprawdza czy można dobierać karty (z talii lub przez przetasowanie waste)
        private static bool HasDrawPileCards(DrawPile drawPile, Waste waste)
        {
            // Jeśli w talii są karty - można dobierać
            if (drawPile.HasCards())
                return true;

            // Jeśli waste nie jest pusty - można przetasować
            if (waste.HasCard())
                return true;

            return false;
        }

        // Sprawdza czy karty są przeciwnego koloru (czerwona vs czarna)
        private static bool IsOpposite(Card a, Card b)
            => (a.Suit == "♥" || a.Suit == "◆") != (b.Suit == "♥" || b.Suit == "◆");

        // Sprawdza czy karta 'higher' jest o jedną wartość wyższa od 'lower'
        private static bool IsOneLower(Card higher, Card lower)
        {
            string[] order = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
            return Array.IndexOf(order, lower.Value) + 1 == Array.IndexOf(order, higher.Value);
        }
    }

    // Główna klasa programu - zarządza całym przebiegiem gry
    class Program
    {
        // Menadżer rankingu wyników (singleton)
        private static ScoreManager scoreManager = new ScoreManager();

        // Metoda wyboru poziomu trudności
        static Difficulty ChooseDifficulty()
        {
            Console.WriteLine("=== PASJANS - WYBÓR POZIOMU TRUDNOŚCI ===");
            Console.WriteLine("1. Łatwy (dobieranie po 1 karcie)");
            Console.WriteLine("2. Trudny (dobieranie po 3 karty, dostępna tylko wierzchnia)");
            Console.Write("Wybierz poziom trudności (1/2): ");

            // Pętla walidacji wyboru użytkownika
            while (true)
            {
                var input = Console.ReadLine();
                if (input == "1")
                {
                    Console.WriteLine("Wybrano poziom ŁATWY\n");
                    return Difficulty.Easy;
                }
                else if (input == "2")
                {
                    Console.WriteLine("Wybrano poziom TRUDNY\n");
                    return Difficulty.Hard;
                }
                else
                {
                    Console.Write("Nieprawidłowy wybór. Wpisz 1 lub 2: ");
                }
            }
        }

        // Tworzenie nowej gry z losowym ułożeniem kart
        static (Tableau, DrawPile, Waste, Foundation) CreateNewGame(Difficulty difficulty)
        {
            // Generowanie pełnej talii 52 kart
            var all = new List<Card>();
            string[] suits = { "♥", "◆", "♣", "♠" };
            string[] vals = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };

            // Tworzenie wszystkich kombinacji kolor-wartość
            foreach (var s in suits)
                foreach (var v in vals)
                    all.Add(new Card { Value = v, Suit = s, IsFaceUp = false });

            var rnd = new Random();
            var tab = new List<Card>();  // Karty na planszę
            var draw = new List<Card>(); // Karty do dobierania

            // Rozdanie 28 kart na planszę (7 kolumn)
            for (int i = 0; i < 28; i++)
            {
                int r = rnd.Next(all.Count);
                tab.Add(all[r]);
                all.RemoveAt(r);
            }

            // Pozostałe 24 karty do talii dobierania
            for (int i = 0; i < 24; i++)
            {
                int r = rnd.Next(all.Count);
                draw.Add(all[r]);
                all.RemoveAt(r);
            }

            // Zwróć komponenty nowej gry
            return (new Tableau(tab), new DrawPile(draw, difficulty), new Waste(difficulty), new Foundation());
        }

        // Główna pętla gry - zarządza rozgrywką
        static void RunGame()
        {
            bool playAgain = true;

            // Główna pętla "zagraj ponownie"
            while (playAgain)
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8; // Obsługa znaków specjalnych
                var difficulty = ChooseDifficulty();  // Wybór trudności

                // Inicjalizacja komponentów gry
                var (tableau, drawPile, waste, foundation) = CreateNewGame(difficulty);
                var undoStack = new Stack<GameState>();  // Stos do cofania ruchów
                int moveCount = 0; // Licznik ruchów

                bool gameRunning = true;
                // Główna pętla gry
                while (gameRunning)
                {
                    // Wyświetlanie aktualnego stanu gry
                    tableau.ShowCards();
                    waste.Show();
                    foundation.Show();

                    // Wyświetlanie informacji dodatkowych
                    Console.WriteLine($"Poziom trudności: {(difficulty == Difficulty.Easy ? "ŁATWY (1 karta)" : "TRUDNY (3 karty)")}");
                    Console.WriteLine($"Draw pile: {drawPile.Count} kart");
                    Console.WriteLine($"Liczba ruchów: {moveCount}");

                    // Wyświetlanie menu opcji
                    Console.WriteLine("\n1: Kolumna -> Kolumna");
                    Console.WriteLine("2: Dobierz z talii");
                    Console.WriteLine("3: Waste -> Kolumna");
                    Console.WriteLine("4: Kolumna -> Foundation");
                    Console.WriteLine("5: Waste -> Foundation");
                    Console.WriteLine("6: Foundation -> Kolumna");
                    Console.WriteLine("7: Wyjdź z gry");
                    Console.WriteLine("8: Cofnij ruch");
                    Console.WriteLine("9: ZOBACZ RANKING");
                    Console.Write("Wybor: ");
                    var ch = Console.ReadLine();
                    Console.WriteLine();

                    // Zapis stanu gry przed wykonaniem ruchu (dla cofania)
                    if (ch != "8" && ch != "7" && ch != "9")
                    {
                        undoStack.Push(new GameState(tableau, drawPile, waste, foundation));
                        // Ograniczenie historii do 3 ostatnich ruchów
                        if (undoStack.Count > 3) undoStack = new Stack<GameState>(undoStack.Take(3).Reverse());
                    }

                    // Obsługa wyboru użytkownika
                    if (ch == "1") // Przenoszenie między kolumnami
                    {
                        tableau.MoveCards();
                        moveCount++;
                        Console.Clear();
                    }
                    else if (ch == "2") // Dobieranie kart z talii
                    {
                        var drawnCards = drawPile.DrawCards();
                        if (drawnCards.Count > 0)
                        {
                            waste.AddCards(drawnCards);
                            Console.WriteLine($"Dobrano {drawnCards.Count} kart z talii.");
                        }
                        else // Brak kart w talii - przetasowanie waste
                        {
                            var rec = waste.EmptyAll();
                            if (rec.Count > 0)
                            {
                                drawPile.Recycle(rec);
                                Console.WriteLine("Ponowne tasowanie talii.");
                                var newCards = drawPile.DrawCards();
                                if (newCards.Count > 0)
                                {
                                    waste.AddCards(newCards);
                                    Console.WriteLine($"Dobrano {newCards.Count} kart po przetasowaniu.");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Brak kart do dobierania.");
                            }
                        }
                        moveCount++;
                        Console.Clear();
                    }
                    else if (ch == "3") // Przenoszenie z waste do kolumny
                    {
                        tableau.MoveFromWaste(waste);
                        moveCount++;
                        Console.Clear();
                    }
                    else if (ch == "4") // Przenoszenie z kolumny do foundation
                    {
                        tableau.MoveToFoundation(foundation);
                        moveCount++;
                        Console.Clear();
                    }
                    else if (ch == "5") // Przenoszenie z waste do foundation
                    {
                        waste.MoveToFoundation(foundation);
                        moveCount++;
                        Console.Clear();
                    }
                    else if (ch == "6") // Przenoszenie z foundation do kolumny
                    {
                        tableau.MoveFromFoundation(foundation);
                        moveCount++;
                        Console.Clear();
                    }
                    else if (ch == "8") // Cofanie ostatniego ruchu
                    {
                        if (undoStack.Count > 0)
                        {
                            var prev = undoStack.Pop();
                            prev.Restore(tableau, drawPile, waste, foundation);
                            moveCount--;
                            Console.WriteLine("Cofnięto ruch.");
                        }
                        else
                        {
                            Console.WriteLine("Brak ruchów do cofnięcia.");
                        }
                        Console.WriteLine("Naciśnij Enter...");
                        Console.ReadLine();
                        Console.Clear();
                    }
                    else if (ch == "9") // Wyświetlanie rankingu
                    {
                        Console.Clear();
                        scoreManager.DisplayTopScores();
                        Console.WriteLine("\nNaciśnij Enter, aby kontynuować...");
                        Console.ReadLine();
                        Console.Clear();
                    }
                    else if (ch == "7") // Wyjście z gry
                    {
                        gameRunning = false;
                        playAgain = false;
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Nieprawidłowy wybór. Wpisz liczbę od 1 do 9.");
                        Console.WriteLine("Naciśnij Enter, aby kontynuować...");
                        Console.ReadLine();
                        Console.Clear();
                    }

                    // Sprawdzenie warunku wygranej
                    if (foundation.IsComplete())
                    {
                        Console.Clear();
                        Console.WriteLine("🎉 GRATULACJE! WYGRAŁEŚ PASJANSA! 🎉");
                        Console.WriteLine($"Poziom trudności: {(difficulty == Difficulty.Easy ? "ŁATWY" : "TRUDNY")}");
                        Console.WriteLine($"Liczba ruchów: {moveCount}");

                        // Zapisywanie wyniku do rankingu
                        Console.Write("Podaj swoją nazwę (max 15 znaków): ");
                        string playerName = Console.ReadLine();
                        if (string.IsNullOrWhiteSpace(playerName))
                            playerName = "Anonim";

                        scoreManager.AddScore(new ScoreEntry
                        {
                            PlayerName = playerName.Length > 15 ? playerName.Substring(0, 15) : playerName,
                            Moves = moveCount,
                            Difficulty = difficulty,
                            Date = DateTime.Now
                        });

                        // Wyświetlenie aktualnego rankingu
                        Console.WriteLine("\nAktualny ranking:");
                        scoreManager.DisplayTopScores();

                        // Zapytanie o ponowną grę
                        Console.Write("\nChcesz zagrać ponownie? (t/n): ");
                        var restart = Console.ReadLine();
                        if (restart?.ToLower() == "t" || restart?.ToLower() == "tak")
                        {
                            gameRunning = false;
                            playAgain = true;
                        }
                        else
                        {
                            gameRunning = false;
                            playAgain = false;
                        }
                        break;
                    }

                    // Sprawdzenie warunku przegranej
                    if (GameChecker.IsGameLost(tableau, drawPile, waste, foundation))
                    {
                        Console.Clear();
                        tableau.ShowCards();
                        waste.Show();
                        foundation.Show();
                        Console.WriteLine("💀 PRZEGRANA! Nie ma więcej dostępnych ruchów. 💀");
                        Console.WriteLine("Chcesz zagrać ponownie? (t/n): ");
                        var restart = Console.ReadLine();
                        if (restart?.ToLower() == "t" || restart?.ToLower() == "tak")
                        {
                            gameRunning = false;
                            playAgain = true;
                        }
                        else
                        {
                            gameRunning = false;
                            playAgain = false;
                        }
                        break;
                    }
                }
            }
        }

        // Punkt wejścia programu
        static void Main()
        {
            RunGame(); // Uruchomienie głównej pętli gry
        }
    }
}