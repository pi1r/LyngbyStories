using System.Windows.Controls;

namespace MoveGame
{
    public class Projectile
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Speed { get; set; }
        public double DirectionX { get; set; }
        public double DirectionY { get; set; }
        public int Damage { get; set; } 


        public Projectile(double x, double y, double speed, double directionX, double directionY, int damage)
        {
            X = x;
            Y = y;
            Speed = speed;
            DirectionX = directionX;
            DirectionY = directionY;
            Damage = damage; 
        }

        public void Move()
        {
            X += DirectionX * Speed;
            Y += DirectionY * Speed;
        }
    }
}
