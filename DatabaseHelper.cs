using System;
using System.Data.SQLite;

namespace MoveGame
{
    public static class DatabaseHelper
    {
        private const string ConnectionString = "Data Source=HighScores.db;Version=3;";

        public static void CreateDatabase()
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS HighScores (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        PlayerName TEXT NOT NULL,
                        Score INTEGER NOT NULL
                    );";

                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void SaveHighScore(string playerName, int score)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string insertQuery = @"
                    INSERT INTO HighScores (PlayerName, Score)
                    VALUES (@PlayerName, @Score);";

                using (var command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@PlayerName", playerName);
                    command.Parameters.AddWithValue("@Score", score);
                    command.ExecuteNonQuery();
                }
            }
        }

        public static int GetHighScore()
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string selectQuery = "SELECT MAX(Score) FROM HighScores;";

                using (var command = new SQLiteCommand(selectQuery, connection))
                {
                    object result = command.ExecuteScalar();
                    return result != DBNull.Value ? Convert.ToInt32(result) : 0;
                }
            }
        }
    }
}

