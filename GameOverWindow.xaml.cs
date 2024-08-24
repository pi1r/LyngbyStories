using System;
using System.Windows;

namespace MoveGame
{
    public partial class GameOverWindow : Window
    {
        public GameOverWindow()
        {
            InitializeComponent();
        }

        private void PlayAgain_Click(object sender, RoutedEventArgs e)
        {
            // Закрываем текущее окно
            this.Close();

            // Перезапускаем приложение
            System.Diagnostics.Process.Start(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

            // Завершаем текущее приложение
            Application.Current.Shutdown();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
