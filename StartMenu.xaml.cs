﻿using System;
using System.Windows;
using System.Data.SQLite;
using System.IO;


namespace MoveGame
{
    public partial class StartMenu : Window
    {
        public StartMenu()
        {
            string dbPath = "HighScores.db";
            InitializeComponent();
            DatabaseHelper.CreateDatabase();
            //MessageBox.Show($"Database Path: {Path.GetFullPath(dbPath)}");
            UpdateBestScoreText(); // Обновляем текст при инициализации окна

        }

        // Метод для обновления текста с лучшим результатом
        public void UpdateBestScoreText()
        {
            int bestScore = DatabaseHelper.GetHighScore(); // Получаем лучший результат из базы данных
            BestScoreText.Text = $"Best Result: {bestScore}";
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {

            MainWindow main  = new MainWindow();
            main.Show();
            this.Close();

        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {

            About about = new About();  
            about.Show();
            this.Close();
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
