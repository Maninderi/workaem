using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Linq;

namespace workaem
{
    public partial class MainWindow : Window
    {
        private Dictionary<(int, int), string> board;
        private bool isComputerGame;
        private bool isGameActive;
        private string currentPlayer;
        private Random random;
        private const int WIN_LENGTH = 3;
        private int minRow = -5, maxRow = 5;
        private int minCol = -5, maxCol = 5;

        public MainWindow()
        {
            InitializeComponent();
            random = new Random();
            InitializeGame();
        }

        private void InitializeGame()
        {
            board = new Dictionary<(int, int), string>();
            isGameActive = false;
            currentPlayer = "X";
            InitializeGrid();
        }

        private void InitializeGrid()
        {
            GameGrid.Children.Clear();
            GameGrid.RowDefinitions.Clear();
            GameGrid.ColumnDefinitions.Clear();

            // Создаем строки и столбцы
            for (int i = minRow; i <= maxRow; i++)
            {
                GameGrid.RowDefinitions.Add(new RowDefinition());
                GameGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            // Создаем кнопки
            for (int i = minRow; i <= maxRow; i++)
            {
                for (int j = minCol; j <= maxCol; j++)
                {
                    Button button = new Button();
                    // Используем буквы m для отрицательных чисел
                    string rowName = i < 0 ? $"m{Math.Abs(i)}" : i.ToString();
                    string colName = j < 0 ? $"m{Math.Abs(j)}" : j.ToString();
                    button.Name = $"Button{rowName}_{colName}";
                    button.Tag = new Tuple<int, int>(i, j); // Сохраняем координаты в Tag
                    button.Click += Button_Click;
                    Grid.SetRow(button, i - minRow);
                    Grid.SetColumn(button, j - minCol);
                    GameGrid.Children.Add(button);
                }
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!isGameActive) return;

            Button button = (Button)sender;
            var coordinates = (Tuple<int, int>)button.Tag;
            int row = coordinates.Item1;
            int col = coordinates.Item2;

            if (!board.ContainsKey((row, col)))
            {
                MakeMove(row, col);

                if (isComputerGame && isGameActive)
                {
                    await Task.Delay(500);
                    MakeComputerMove();
                }
            }
        }

        private void MakeMove(int row, int col)
        {
            board[(row, col)] = currentPlayer;
            Button button = FindButton(row, col);
            button.Content = currentPlayer;

            if (CheckWinner(row, col))
            {
                MessageBox.Show($"Победил игрок {currentPlayer}!");
                isGameActive = false;
                return;
            }

            currentPlayer = currentPlayer == "X" ? "O" : "X";
            UpdateStatus();
            
            // Расширяем поле при необходимости
            if (ShouldExpandGrid(row, col))
            {
                ExpandGrid();
            }
        }

        private bool ShouldExpandGrid(int row, int col)
        {
            return row == minRow || row == maxRow || col == minCol || col == maxCol;
        }

        private void ExpandGrid()
        {
            minRow--;
            maxRow++;
            minCol--;
            maxCol++;
            InitializeGrid();
            // Восстанавливаем существующие ходы
            foreach (var move in board)
            {
                Button button = FindButton(move.Key.Item1, move.Key.Item2);
                if (button != null)
                {
                    button.Content = move.Value;
                }
            }
        }

        private void MakeComputerMove()
        {
            // Поиск выигрышного хода
            foreach (var cell in GetEmptyCells())
            {
                board[cell] = "O";
                if (CheckWinner(cell.Item1, cell.Item2))
                {
                    MakeMove(cell.Item1, cell.Item2);
                    return;
                }
                board.Remove(cell);
            }

            // Блокировка выигрышного хода противника
            foreach (var cell in GetEmptyCells())
            {
                board[cell] = "X";
                if (CheckWinner(cell.Item1, cell.Item2))
                {
                    board.Remove(cell);
                    MakeMove(cell.Item1, cell.Item2);
                    return;
                }
                board.Remove(cell);
            }

            // Ход рядом с существующими символами
            var nearMoves = GetNearMoves();
            if (nearMoves.Count > 0)
            {
                var move = nearMoves[random.Next(nearMoves.Count)];
                MakeMove(move.Item1, move.Item2);
                return;
            }

            // Случайный ход в центре поля
            int row = random.Next(-2, 3);
            int col = random.Next(-2, 3);
            while (board.ContainsKey((row, col)))
            {
                row = random.Next(-2, 3);
                col = random.Next(-2, 3);
            }
            MakeMove(row, col);
        }

        private List<(int, int)> GetEmptyCells()
        {
            var emptyCells = new List<(int, int)>();
            for (int i = minRow; i <= maxRow; i++)
            {
                for (int j = minCol; j <= maxCol; j++)
                {
                    if (!board.ContainsKey((i, j)))
                    {
                        emptyCells.Add((i, j));
                    }
                }
            }
            return emptyCells;
        }

        private List<(int, int)> GetNearMoves()
        {
            var nearMoves = new List<(int, int)>();
            var directions = new[] { (-1, -1), (-1, 0), (-1, 1), (0, -1), (0, 1), (1, -1), (1, 0), (1, 1) };

            foreach (var pos in board.Keys)
            {
                foreach (var dir in directions)
                {
                    var newPos = (pos.Item1 + dir.Item1, pos.Item2 + dir.Item2);
                    if (!board.ContainsKey(newPos))
                    {
                        nearMoves.Add(newPos);
                    }
                }
            }
            return nearMoves;
        }

        private bool CheckWinner(int row, int col)
        {
            var directions = new[] { (1, 0), (0, 1), (1, 1), (1, -1) };
            foreach (var dir in directions)
            {
                int count = 1;
                count += CountInDirection(row, col, dir.Item1, dir.Item2);
                count += CountInDirection(row, col, -dir.Item1, -dir.Item2);
                if (count >= WIN_LENGTH) return true;
            }
            return false;
        }

        private int CountInDirection(int row, int col, int dRow, int dCol)
        {
            int count = 0;
            int r = row + dRow;
            int c = col + dCol;
            string player = board[(row, col)];

            while (board.TryGetValue((r, c), out string value) && value == player)
            {
                count++;
                r += dRow;
                c += dCol;
            }
            return count;
        }

        private Button FindButton(int row, int col)
        {
            return (Button)GameGrid.Children.Cast<UIElement>()
                .FirstOrDefault(e => e is Button button && 
                    ((Tuple<int, int>)button.Tag).Item1 == row && 
                    ((Tuple<int, int>)button.Tag).Item2 == col);
        }

        private void UpdateStatus()
        {
            StatusText.Text = $"Ход игрока: {currentPlayer}";
        }

        private void PlayVsComputer_Click(object sender, RoutedEventArgs e)
        {
            InitializeGame();
            isComputerGame = true;
            isGameActive = true;
            currentPlayer = "O"; // Компьютер ходит первым
            UpdateStatus();
            MakeComputerMove();
        }

        private void PlayVsPlayer_Click(object sender, RoutedEventArgs e)
        {
            InitializeGame();
            isComputerGame = false;
            isGameActive = true;
            UpdateStatus();
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            if (isComputerGame)
                PlayVsComputer_Click(sender, e);
            else
                PlayVsPlayer_Click(sender, e);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Rules_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Правила игры:\n\n" +
                "1. Игроки по очереди ставят на свободные клетки поля знаки (один всегда крестики, другой всегда нолики).\n" +
                "2. Первый, выстроивший в ряд 3 своих фигуры по вертикали, горизонтали или диагонали, выигрывает.\n" +
                "3. Поле бесконечное - оно расширяется при достижении края.\n" +
                "4. В игре против компьютера игрок всегда ходит вторым.",
                "Правила игры",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Крестики-нолики\n\n" +
                "Версия: 1.0\n" +
                "Бесконечное поле\n" +
                "Игра против компьютера или другого игрока",
                "О программе",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
