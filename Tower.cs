namespace MoveGame
{
    internal class Tower
    {
        public double Health { get; set; }
        public double GunsNumbers { get; set; }
        public double Damage { get; set; }

        public Tower(double health, double gunsNumbers, double damage)
        {
            Health = health;
            GunsNumbers = gunsNumbers;
            Damage = damage;
        }
    }
}
