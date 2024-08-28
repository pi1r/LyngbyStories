using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MoveGame
{
    public partial class MainWindow : Window
    {
        private Hero _hero;
        private Tower _tower;
        private DispatcherTimer _projectileTimer1;
        private DispatcherTimer _projectileTimer2;
        private DispatcherTimer _projectileTimer3;
        private DispatcherTimer _enemySpawnTimer;
        private bool _inBombZone1;
        private bool _inBombZone2;
        private bool _inBombZone3;
        private List<Button> _enemies;
        private int _expPoints;
        private int _level;

        public MainWindow()

        {

            InitializeComponent();
            DatabaseHelper.CreateDatabase();
            this.Focusable = true;
            this.Focus();

            _hero = new Hero(100, 100, speed: 10);
            _tower = new Tower(health: 100, gunsNumbers: 1, damage: 10);

            _enemies = new List<Button>();
            _expPoints = 0;
            _level = 1;  // Начальный уровень

            Canvas.SetLeft(Hero, _hero.X);
            Canvas.SetTop(Hero, _hero.Y);

            // Применяем стиль для отключения эффектов наведения к герою
            Hero.Style = (Style)FindResource("NoHoverEffectButtonStyle");

            _projectileTimer1 = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1000)
            };
            _projectileTimer1.Tick += (s, e) => LaunchProjectileFromZone(BombZone);

            _projectileTimer2 = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1000)
            };
            _projectileTimer2.Tick += (s, e) => LaunchProjectileFromZone(BombZone2);

            _projectileTimer3 = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1000)
            };
            _projectileTimer3.Tick += (s, e) => LaunchProjectileFromZone(BombZone3);

            _enemySpawnTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _enemySpawnTimer.Tick += SpawnEnemy;
            _enemySpawnTimer.Start();

            UpdateTowerHealthText();
            UpdateExpText();
            UpdateLevelText();

            // Создание базы данных и таблицы, если они не существуют
            DatabaseHelper.CreateDatabase();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            double left = Canvas.GetLeft(Hero);
            double top = Canvas.GetTop(Hero);

            double canvasWidth = GameBorder.ActualWidth;
            double canvasHeight = GameBorder.ActualHeight;
            double buttonWidth = Hero.ActualWidth;
            double buttonHeight = Hero.ActualHeight;

            switch (e.Key)
            {
                case Key.A:
                    if (left - _hero.Speed >= 0)
                        _hero.MoveLeft();
                    break;
                case Key.D:
                    if (left + _hero.Speed + buttonWidth <= canvasWidth)
                        _hero.MoveRight();
                    break;
                case Key.W:
                    if (top - _hero.Speed >= 0)
                        _hero.MoveUp();
                    break;
                case Key.S:
                    if (top + _hero.Speed + buttonHeight <= canvasHeight)
                        _hero.MoveDown();
                    break;
            }

            Canvas.SetLeft(Hero, _hero.X);
            Canvas.SetTop(Hero, _hero.Y);

            CheckBombZoneCollision();
        }

        private void CheckBombZoneCollision()
        {
            double heroLeft = Canvas.GetLeft(Hero);
            double heroTop = Canvas.GetTop(Hero);

            CheckZoneCollision(heroLeft, heroTop, BombZone, ref _inBombZone1, _projectileTimer1);
            CheckZoneCollision(heroLeft, heroTop, BombZone2, ref _inBombZone2, _projectileTimer2);
            CheckZoneCollision(heroLeft, heroTop, BombZone3, ref _inBombZone3, _projectileTimer3);
        }

        private void CheckZoneCollision(double heroLeft, double heroTop, Canvas bombZone, ref bool inBombZone, DispatcherTimer timer)
        {
            double bombZoneLeft = Canvas.GetLeft(bombZone);
            double bombZoneTop = Canvas.GetTop(bombZone);

            if (heroLeft < bombZoneLeft + bombZone.Width &&
                heroLeft + Hero.Width > bombZoneLeft &&
                heroTop < bombZoneTop + bombZone.Height &&
                heroTop + Hero.Height > bombZoneTop)
            {
                if (!inBombZone)
                {
                    inBombZone = true;
                    timer.Start();
                }
            }
            else
            {
                if (inBombZone)
                {
                    inBombZone = false;
                    timer.Stop();
                }
            }
        }

        private void LaunchProjectileFromZone(Canvas bombZone)
        {
            Rectangle projectile = new Rectangle
            {
                Width = 10,
                Height = 10,
                Fill = Brushes.Black
            };

            double bombZoneLeft = Canvas.GetLeft(bombZone);
            double bombZoneTop = Canvas.GetTop(bombZone);

            double startX = bombZoneLeft + bombZone.Width - 250;
            double startY = bombZoneTop + bombZone.Height / 2 - projectile.Height / 2;

            Canvas.SetLeft(projectile, startX);
            Canvas.SetTop(projectile, startY);
            Canvas.SetZIndex(projectile, 100);

            EnemySpace.Children.Add(projectile);

            Projectile projectileData = new Projectile(
                startX,
                startY,
                speed: 5,
                directionX: 1,
                directionY: 0,
                damage: 100
            );

            DispatcherTimer moveTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(20)
            };
            moveTimer.Tick += (s, ev) =>
            {
                projectileData.Move();
                Canvas.SetLeft(projectile, projectileData.X);
                Canvas.SetTop(projectile, projectileData.Y);

                CheckProjectileCollisionWithEnemies(projectile, projectileData, moveTimer);
            };
            moveTimer.Start();
        }

        private void SpawnEnemy(object sender, EventArgs e)
        {
            if (_tower.Health <= 0)
            {
                StopGame();
                this.Hide();

                GameOverWindow gameOverWindow = new GameOverWindow();
                gameOverWindow.ShowDialog();

                Application.Current.Shutdown();
                return;
            }

            Random rand = new Random();
            int zoneIndex = rand.Next(1, 4);

            // Вероятности появления разных врагов
            bool isTankEnemy = _level >= 3 && rand.Next(0, 5) == 0; // 20% шанс появления TankEnemy на 3+ уровне
            bool isFastEnemy = _level >= 2 && rand.Next(0, 4) == 0; // 25% шанс появления FastEnemy на 2+ уровне

            double enemyY = 0;
            switch (zoneIndex)
            {
                case 1: enemyY = Canvas.GetTop(BombZone) + BombZone.Height / 2; break;
                case 2: enemyY = Canvas.GetTop(BombZone2) + BombZone.Height / 2; break;
                case 3: enemyY = Canvas.GetTop(BombZone3) + BombZone.Height / 2; break;
            }

            Button enemyButton = new Button
            {
                Width = 20,
                Height = 20,
                Background = new ImageBrush(new BitmapImage(new Uri(GetEnemyTexture(isTankEnemy, isFastEnemy)))),
                Style = (Style)this.Resources["NoHoverEffectButtonStyle"]
            };

            Enemy enemyData;
            if (isTankEnemy)
            {
                enemyData = new TankEnemy(); // Медленный, но с большим здоровьем
            }
            else if (isFastEnemy)
            {
                enemyData = new FastEnemy(); // Быстрый, но с низким здоровьем
            }
            else
            {
                enemyData = new Enemy(100, 2, 20); // Обычный враг
            }

            enemyButton.Tag = enemyData;

            Canvas.SetLeft(enemyButton, EnemySpace.Width - enemyButton.Width);
            Canvas.SetTop(enemyButton, enemyY - enemyButton.Height / 2);

            EnemySpace.Children.Add(enemyButton);
            _enemies.Add(enemyButton);

            DispatcherTimer moveTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(20)
            };
            moveTimer.Tick += (s, ev) =>
            {
                if (!_enemies.Contains(enemyButton))
                {
                    moveTimer.Stop();
                    return;
                }

                double currentX = Canvas.GetLeft(enemyButton);
                if (currentX <= 0)
                {
                    moveTimer.Stop();
                    EnemySpace.Children.Remove(enemyButton);
                    _enemies.Remove(enemyButton);

                    _tower.Health -= 20;
                    UpdateTowerHealthText();

                    if (_tower.Health <= 0)
                    {
                        StopGame();
                        this.Hide();

                        GameOverWindow gameOverWindow = new GameOverWindow();
                        gameOverWindow.ShowDialog();

                        Application.Current.Shutdown();
                    }
                }
                else
                {
                    Canvas.SetLeft(enemyButton, currentX - enemyData.Speed);
                }
            };
            moveTimer.Start();
        }


        private string GetEnemyTexture(bool isTankEnemy, bool isFastEnemy)
        {
            if (isTankEnemy)
                return "D:\\c#\\Мои игры\\MoveGame\\MoveGame\\textures\\enemyTank.png"; 
            if (isFastEnemy)
                return "D:\\c#\\Мои игры\\MoveGame\\MoveGame\\textures\\enemyFast.png"; // Пример пути для быстрого врага

            return "D:\\c#\\Мои игры\\MoveGame\\MoveGame\\textures\\enemy.png"; // Пример пути для обычного врага
        }

        private void CheckProjectileCollisionWithEnemies(Rectangle projectile, Projectile projectileData, DispatcherTimer moveTimer)
        {
            foreach (Button enemyButton in _enemies.ToList())
            {
                double projectileLeft = Canvas.GetLeft(projectile);
                double projectileTop = Canvas.GetTop(projectile);

                double enemyLeft = Canvas.GetLeft(enemyButton);
                double enemyTop = Canvas.GetTop(enemyButton);

                if (projectileLeft < enemyLeft + enemyButton.Width &&
                    projectileLeft + projectile.Width > enemyLeft &&
                    projectileTop < enemyTop + enemyButton.Height &&
                    projectileTop + projectile.Height > enemyTop)
                {
                    Enemy enemyData = (Enemy)enemyButton.Tag;
                    enemyData.Health -= projectileData.Damage;

                    if (enemyData.Health <= 0)
                    {
                        _expPoints += 20;
                        UpdateExpText();

                        // Проверка на уровень
                        if (_expPoints >= _level * 100)
                        {
                            _level++;
                            UpdateLevelText();
                        }

                        // Удаление врага
                        EnemySpace.Children.Remove(enemyButton);
                        _enemies.Remove(enemyButton);
                    }

                    // Удаление снаряда
                    EnemySpace.Children.Remove(projectile);
                    moveTimer.Stop();
                    return;
                }
            }
        }

        private void BackToMenu_Click(object sender, RoutedEventArgs e)
        {
            StopGame();
            this.Close();
            StartMenu startMenu = new StartMenu();
            startMenu.Show();
        }


        public void StopGame()
        {
            _enemySpawnTimer.Stop();
            _projectileTimer1.Stop();
            _projectileTimer2.Stop();
            _projectileTimer3.Stop();


            EndGame();

        }

        private void UpdateTowerHealthText()
        {
            TowerHealthText.Text = $"Health: {_tower.Health}";
        }

        private void UpdateExpText()
        {
            ExpPointsText.Text = $"EXP Points: {_expPoints}";
        }

        public void UpdateBestScoreText()
        {
            int highScore = DatabaseHelper.GetHighScore();
            StartMenu startMenu = new StartMenu();
            startMenu.BestScoreText.Text = $"Best Score: {highScore}";
        }


        private void LevelUp()
        {
            _level++;
            UpdateLevelText();
        }
        private void EndGame()
        {
            // Сохранение нового рекорда
            DatabaseHelper.SaveHighScore("Player1", _expPoints);

            // Обновление лучшего результата
            StartMenu startMenu = new StartMenu();
            startMenu.UpdateBestScoreText();
            this.Close();
        }

        private void UpdateLevelText()
        {
            LevelText.Text = $"Level: {_level}";
        }



    }
}
