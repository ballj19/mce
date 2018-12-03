using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure
{
    public abstract class Character
    {
        public int max_health;
        public int health;
        public int energy;
        public int mana;
        public string name;
        public int max_energy;
        public int max_mana;

        public EventHandler Dead;

        public void Hit(int damage)
        {
            if(health - damage <= 0)
            {
                health = 0;
                OnDeath();
                //raise death event
            }
            else
            {
                health -= damage;
            }

            Console.WriteLine("Hit for {0} Damage, Health Remaining: {1}", damage, health);
        }

        public void Heal(int heal)
        {
            if(heal + health > max_health)
            {
                health = max_health;
            }
            else
            {
                health += heal;
            }
            Console.WriteLine("Healed for {0} Health, Health Remaining: {1}", heal, health);
        }

        protected virtual void OnDeath()
        {
            if (Dead != null)
                Dead(this, EventArgs.Empty);
        }

        public void AddTitle()
        {
            List<string> titles = new List<string> { "The Defiled", "The Legendary", "The Beast" };

            Random r = new Random();

             name += " " + titles[r.Next(0, 2)];
        }

        public abstract bool ValidateWeapon(Weapon weapon);
        
    }
}
