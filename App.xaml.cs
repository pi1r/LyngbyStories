//using System.Configuration;
//using System.Data;
//using System.Windows;

//namespace MoveGame
//{
//    /// <summary>
//    /// Interaction logic for App.xaml
//    /// </summary>
//    public partial class App : Application
//    {
//    }

//}

using System;
using System.Configuration;
using System.Windows;
using LiteDB;

namespace MoveGame
{
    public partial class App : Application
    {
        public static LiteDatabase Database { get; private set; }
        public static ILiteCollection<HighScore> HighScores { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Инициализация базы данных
            Database = new LiteDatabase(@"MyGameData.db");
            HighScores = Database.GetCollection<HighScore>("highScores");

            // Проверяем, есть ли в базе данных текущий лучший результат
            var bestScore = HighScores.FindOne(Query.All());

            if (bestScore == null)
            {
                // Если результата нет, создаем новый с нулевым значением
                bestScore = new HighScore { Score = 0, Date = DateTime.Now };
                HighScores.Insert(bestScore);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Database?.Dispose();
            base.OnExit(e);
        }
    }
}
