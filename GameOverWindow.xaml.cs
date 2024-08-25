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
            // закрываем текущее окно
            this.Close();

            // перезапускаем приложение
            System.Diagnostics.Process.Start(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

            // завершаем текущее приложение
            Application.Current.Shutdown();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
