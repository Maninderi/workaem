using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace workaem
{
    public partial class MainWindow : Window
    {
        public partial class MainWindow : Window
        {
            private string currentPlayer = "X";
            private string[,] board = new string[3, 3];

            public MainWindow()
            {
                InitializeComponent();
            }

            private void Button_Click(object sender, RoutedEventArgs e)
            {
                Button button = sender as Button;

                if (button.Content == null)
                {
                    button.Content = currentPlayer;
                    int row = Grid.GetRow(button);
                    int col = Grid.GetColumn(button);
                    board[row, col] = currentPlayer;

                    if (CheckForWinner())
                    {
                        MessageBox.Show($"Игрок {currentPlayer} выиграл!");
                        ResetGame();
                    }
                    else
                    {
                        currentPlayer = currentPlayer == "X" ? "O" : "X";
                    }
                }
            }

            private bool CheckForWinner()
            {
                // Проверка строк, столбцов и диагоналей
                for (int i = 0; i < 3; i++)
                {
                    if (board[i, 0] == currentPlayer && board[i, 1] == currentPlayer && board[i, 2] == currentPlayer)
                        return true;
                    if (board[0, i] == currentPlayer && board[1, i] == currentPlayer && board[2, i] == currentPlayer)
                        return true;
                }
                if (board[0, 0] == currentPlayer && board[1, 1] == currentPlayer && board[2, 2] == currentPlayer)
                    return true;
                if (board[0, 2] == currentPlayer && board[1, 1] == currentPlayer && board[2, 0] == currentPlayer)
                    return true;

                return false;
            }

            private void ResetGame()
            {
                currentPlayer = "X";
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        board[i, j] = null;
                        Button button = (Button)FindName($"Button{i}{j}");
                        button.Content = null;
                    }
                }
            }

            private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Разработчик: Рыбалка Евгений и Пантелеев Александр\nГруппа: Исппк-22-1", "Информация о разработчике", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


    }
}
