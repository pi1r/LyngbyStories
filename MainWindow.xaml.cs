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
        private List<Image> _enemies;
        private int _expPoints;

        public MainWindow()
        {
            InitializeComponent();
            this.Focusable = true;
            this.Focus();

            _hero = new Hero(100, 100, speed: 20);
            _tower = new Tower(health: 100, 1, 10); // Устанавливаем начальное здоровье башни

            _enemies = new List<Image>();
            _expPoints = 0;

            Canvas.SetLeft(Hero, _hero.X);
            Canvas.SetTop(Hero, _hero.Y);

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
                1,
                0,
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

            double enemyY = 0;
            switch (zoneIndex)
            {
                case 1: enemyY = Canvas.GetTop(BombZone) + BombZone.Height / 2; break;
                case 2: enemyY = Canvas.GetTop(BombZone2) + BombZone.Height / 2; break;
                case 3: enemyY = Canvas.GetTop(BombZone3) + BombZone.Height / 2; break;
            }

            Image enemyImage = new Image
            {
                Width = 40,
                Height = 40,
                Source = new BitmapImage(new Uri("D:\\c#\\Мои игры\\MoveGame\\MoveGame\\textures\\enemy.png")) // Путь к изображению
            };

            Enemy enemyData = new Enemy
            {
                Health = 100,
                Speed = 2
            };

            enemyImage.Tag = enemyData;

            Canvas.SetLeft(enemyImage, EnemySpace.Width - enemyImage.Width);
            Canvas.SetTop(enemyImage, enemyY - enemyImage.Height / 2);

            EnemySpace.Children.Add(enemyImage);
            _enemies.Add(enemyImage);

            DispatcherTimer moveTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(20)
            };
            moveTimer.Tick += (s, ev) =>
            {
                if (!_enemies.Contains(enemyImage))
                {
                    moveTimer.Stop();
                    return;
                }

                double currentX = Canvas.GetLeft(enemyImage);
                if (currentX <= 0)
                {
                    moveTimer.Stop();
                    EnemySpace.Children.Remove(enemyImage);
                    _enemies.Remove(enemyImage);

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
                    Canvas.SetLeft(enemyImage, currentX - enemyData.Speed);
                }
            };
            moveTimer.Start();
        }

        private void StopGame()
        {
            _projectileTimer1.Stop();
            _projectileTimer2.Stop();
            _projectileTimer3.Stop();
            _enemySpawnTimer.Stop();

            ClearEnemiesAndProjectiles();
        }

        private void CheckProjectileCollisionWithEnemies(Rectangle projectile, Projectile projectileData, DispatcherTimer moveTimer)
        {
            List<Image> enemiesToRemove = new List<Image>();

            foreach (var enemyImage in _enemies.ToList())
            {
                double enemyX = Canvas.GetLeft(enemyImage);
                double enemyY = Canvas.GetTop(enemyImage);

                if (projectileData.X < enemyX + enemyImage.Width &&
                    projectileData.X + projectile.Width > enemyX &&
                    projectileData.Y < enemyY + enemyImage.Height &&
                    projectileData.Y + projectile.Height > enemyY)
                {
                    moveTimer.Stop();
                    EnemySpace.Children.Remove(projectile);

                    Enemy enemy = (Enemy)enemyImage.Tag;
                    enemy.Health -= projectileData.Damage;

                    if (enemy.Health <= 0)
                    {
                        EnemySpace.Children.Remove(enemyImage);
                        _enemies.Remove(enemyImage);
                        _expPoints += 10; // Добавляем очки за уничтожение врага
                        UpdateExpText();
                    }
                }
            }
        }

        private void ClearEnemiesAndProjectiles()
        {
            List<UIElement> elementsToRemove = new List<UIElement>();

            foreach (var element in EnemySpace.Children)
            {
                if (element is Image || element is Rectangle)
                {
                    elementsToRemove.Add((UIElement)element);
                }
            }

            foreach (var element in elementsToRemove)
            {
                EnemySpace.Children.Remove(element);
            }

            _enemies.Clear();
        }

        private void UpdateExpText()
        {
            ExpPointsText.Text = $"Experience: {_expPoints}";
        }

        private void UpdateTowerHealthText()
        {
            TowerHealthText.Text = $"Health: {_tower.Health}";
        }
    }
}
