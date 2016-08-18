using Game.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Player.Interfaces;
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
using System.Windows.Threading;

namespace TicTacToe.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IPlayer player;
        IPlayer cpu;
        IGame game;
        bool gameStarted = false;
        DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();

            InitializeGame();
        }

        private async void InitializeGame()
        {
            startButton.Visibility = Visibility.Collapsed;

            RefreshBoard();

            player = ActorProxy.Create<IPlayer>(ActorId.CreateRandom(), "fabric:/ActorTicTacToeApplication");
            cpu = ActorProxy.Create<IPlayer>(ActorId.CreateRandom(), "fabric:/ActorTicTacToeApplication");
            game = ActorProxy.Create<IGame>(ActorId.CreateRandom(), "fabric:/ActorTicTacToeApplication");

            await player.JoinGameAsync(game.GetActorId(), "player");
            await cpu.JoinGameAsync(game.GetActorId(), "cpu");

            startButton.Visibility = Visibility.Visible;
        }

        private void StartGame()
        {
            RefreshBoard();

            startButton.Visibility = Visibility.Collapsed;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += Timer_Tick;
            timer.Start();

            gameStarted = true;
        }

        private async void StopGame()
        {
            gameStarted = false;
            
            if (timer != null)
            {
                timer.Stop();
                timer.Tick -= Timer_Tick;
                timer = null;
            }

            await game.ResetGameAsync();

            startButton.Visibility = Visibility.Visible;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            MakeCpuMove();
        }

        private async void RefreshBoard()
        {
            if (game == null)
            {
                for (int x = 0; x < 3; x++)
                    for (int y = 0; y < 3; y++)
                        GetTextAtPosition(x, y).Text = "";
                return;
            }

            var board = await game.GetGameBoardAsync();
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    if (board[y * 3 + x] == 0)
                        GetTextAtPosition(x, y).Text = "";
                    else if (board[y * 3 + x] < 0)
                        GetTextAtPosition(x, y).Text = "O";
                    if (board[y * 3 + x] > 0)
                        GetTextAtPosition(x, y).Text = "X";
                }
            }

            var winner = await game.GetWinnerAsync();
            if (winner != "")
            {
                StopGame();

                MessageBox.Show($"The winner is: {winner}", "GAME OVER", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!gameStarted)
                StartGame();
        }

        private async void BoardButton_Click(object sender, RoutedEventArgs e)
        {
            if (!gameStarted) return;

            int x = Grid.GetRow(sender as Button);
            int y = Grid.GetColumn(sender as Button);

            bool ok = await player.MakeMoveAsync(game.GetActorId(), x, y);

            if (ok)
                RefreshBoard();
        }

        private TextBlock GetTextAtPosition(int x, int y)
        {
            return (TextBlock)gameBoardGrid.Children.Cast<UIElement>().First(i => i.GetType() == typeof(TextBlock) && Grid.GetRow(i) == x && Grid.GetColumn(i) == y);
        }

        private async void MakeCpuMove()
        {
            Random rand = new Random();
            await cpu.MakeMoveAsync(game.GetActorId(), rand.Next(0, 3), rand.Next(0, 3));
            RefreshBoard();
        }
    }
}
