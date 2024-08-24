namespace MoveGame
{
    public class Hero
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Speed { get; set; }

        public Hero(double x, double y, double speed)
        {
            X = x;
            Y = y;
            Speed = speed;
        }

        public void MoveLeft()
        {
            X -= Speed;
        }

        public void MoveRight()
        {
            X += Speed;
        }

        public void MoveUp()
        {
            Y -= Speed;
        }

        public void MoveDown()
        {
            Y += Speed;
        }
    }
}
