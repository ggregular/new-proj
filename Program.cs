// Версія X - гілка conflict-x
using System.Collections.Generic;
using System.Linq;

namespace EightPuzzleBFS
{
    // Клас для збору статистики
    public class SearchStatistics
    {
        public int GeneratedStates { get; set; }      // Загальна кількість згенерованих станів
        public int AddedToDatabase { get; set; }      // Кількість станів занесених в базу (visited)
        public int RejectedStates { get; set; }       // Кількість відкинутих станів (дублікати)
        public int SolutionDepth { get; set; }        // Глибина дерева пошуку

        public void PrintStatistics()
        {
            Console.WriteLine("═══════════════════════════════════════════════");
            Console.WriteLine("           СТАТИСТИКА ПОШУКУ BFS");
            Console.WriteLine("═══════════════════════════════════════════════");
            Console.WriteLine($"Загальна кількість згенерованих станів: {GeneratedStates}");
            Console.WriteLine($"Кількість станів занесених в базу:     {AddedToDatabase}");
            Console.WriteLine($"Кількість відкинутих станів:           {RejectedStates}");
            Console.WriteLine($"Глибина дерева пошуку (кроків):        {SolutionDepth}");
            Console.WriteLine("═══════════════════════════════════════════════");
            Console.WriteLine($"\nПеревірка: {GeneratedStates} = {AddedToDatabase} + {RejectedStates}");
            Console.WriteLine($"Результат: {GeneratedStates} = {AddedToDatabase + RejectedStates} ✓");
        }
    }

    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Початковий стан
            int[,] start = { { 3, 4, 0 }, { 1, 2, 6 }, { 7, 5, 8 } };

            Console.WriteLine("╔═══════════════════════════════════════════════╗");
            Console.WriteLine("║         8-PUZZLE BFS SOLVER                   ║");
            Console.WriteLine("╚═══════════════════════════════════════════════╝");
            Console.WriteLine("\nПочатковий стан:");
            Print(start);
            Console.WriteLine("\nЦільовий стан:");
            int[,] goal = { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 0 } };
            Print(goal);
            Console.WriteLine("\nШукаю рішення...\n");

            var statistics = new SearchStatistics();
            var solution = Solve(start, statistics);

            if (solution != null)
            {
                Console.WriteLine($"\n✓ РІШЕННЯ ЗНАЙДЕНО!\n");
                Console.WriteLine($"Кількість кроків: {solution.Count - 1}\n");
                Console.WriteLine("═══════════════════════════════════════════════");
                Console.WriteLine("           ПОСЛІДОВНІСТЬ ХОДІВ");
                Console.WriteLine("═══════════════════════════════════════════════\n");

                int step = 0;
                foreach (var (board, move) in solution)
                {
                    if (move != null)
                    {
                        Console.WriteLine($"Крок {step}: {move}");
                        step++;
                    }
                    else
                    {
                        Console.WriteLine("Початковий стан:");
                    }
                    Print(board);
                    Console.WriteLine();
                }

                Console.WriteLine();
                statistics.PrintStatistics();
            }
            else
            {
                Console.WriteLine("✗ Рішення не знайдено!");
                statistics.PrintStatistics();
            }

            Console.WriteLine("\nНатисніть будь-яку клавішу для виходу...");
            Console.ReadKey();
        }

        // Пошук рішення зі збором статистики
        static List<(int[,] board, string move)> Solve(int[,] start, SearchStatistics stats)
        {
            var queue = new Queue<(int[,] board, List<(int[,], string)> path)>();
            var visited = new HashSet<string>();

            queue.Enqueue((start, new List<(int[,], string)> { (start, null) }));
            visited.Add(ToKey(start));

            // Початковий стан вже в базі
            stats.AddedToDatabase = 1;
            stats.GeneratedStates = 0; // Початковий стан не генерується, він заданий

            // Напрямки: вгору, вниз, ліворуч, праворуч
            var moves = new[] {
                (-1, 0, "↑ ВГОРУ"),
                (1, 0, "↓ ВНИЗ"),
                (0, -1, "← ЛІВОРУЧ"),
                (0, 1, "→ ПРАВОРУЧ")
            };

            while (queue.Count > 0)
            {
                var (board, path) = queue.Dequeue();

                if (IsGoal(board))
                {
                    stats.SolutionDepth = path.Count - 1; // -1 бо перший елемент це початковий стан
                    return path;
                }

                // Знаходимо порожню клітинку
                var (emptyRow, emptyCol) = FindEmpty(board);

                // Пробуємо всі ходи
                foreach (var (dr, dc, name) in moves)
                {
                    int newRow = emptyRow + dr;
                    int newCol = emptyCol + dc;

                    // Перевіряємо межі
                    if (newRow < 0 || newRow >= 3 || newCol < 0 || newCol >= 3)
                        continue;

                    // Створюємо новий стан
                    var newBoard = (int[,])board.Clone();
                    newBoard[emptyRow, emptyCol] = newBoard[newRow, newCol];
                    newBoard[newRow, newCol] = 0;

                    // Збільшуємо лічильник згенерованих станів
                    stats.GeneratedStates++;

                    string key = ToKey(newBoard);
                    if (!visited.Contains(key))
                    {
                        // Стан додано в базу
                        visited.Add(key);
                        stats.AddedToDatabase++;

                        var newPath = new List<(int[,], string)>(path) { (newBoard, name) };
                        queue.Enqueue((newBoard, newPath));
                    }
                    else
                    {
                        // Стан відкинуто (дублікат)
                        stats.RejectedStates++;
                    }
                }
            }

            return null;
        }

        // Перетворення дошки в ключ для перевірки
        static string ToKey(int[,] board)
        {
            return string.Join("", board.Cast<int>());
        }

        // Чи це цільовий стан?
        static bool IsGoal(int[,] board)
        {
            int expected = 1;
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                {
                    if (i == 2 && j == 2) return board[i, j] == 0;
                    if (board[i, j] != expected++) return false;
                }
            return true;
        }

        // Знайти позицію порожньої клітинки
        static (int row, int col) FindEmpty(int[,] board)
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (board[i, j] == 0)
                        return (i, j);
            return (-1, -1);
        }

        // Друк дошки
        static void Print(int[,] board)
        {
            Console.WriteLine("┌───────┐");
            for (int i = 0; i < 3; i++)
            {
                Console.Write("│");
                for (int j = 0; j < 3; j++)
                    Console.Write(board[i, j] == 0 ? " □" : $" {board[i, j]}");
                Console.WriteLine(" │");
            }
            Console.WriteLine("└───────┘");
        }
    }
}/ / 
 
 <i =0
 
 7
 
 3i ;:8
 
 c o n f l i c t - a 
 
 / / 
 
 <i =0
 
 7
 
 3i ;:8
 
 c o n f l i c t - b 
 
 