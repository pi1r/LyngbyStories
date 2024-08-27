using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

namespace MoveGame
{
    public class Enemy
    {
        public double Health { get; set; }
        public double Speed { get; set; }
        public double Damage { get; set; }

        public Enemy(double health, double speed, double damage)
        {
            Health = health; 
            Speed = speed;
            Damage = damage;
        }
        
    }

    public class TankEnemy : Enemy
    {
        public TankEnemy() : base(health: 300, 0.5, 20)
        {

        }
    }


    public class FastEnemy: Enemy
    {
        public FastEnemy() : base(health: 100, speed: 4, damage: 20)
        {
            {

            }
        }
    }
}
