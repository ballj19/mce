using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure
{
    public class Warrior : Character
    {
        private List<string> weapon_types = new List<string> { "Sword", "Greatsword", "Axe", "Greataxe" };

        public Warrior(string name)
        {
            this.name = name;
            max_health = 100;
            health = 100;
            max_energy = 100;
            energy = 100;
            max_mana = 50;
            mana = 50;
        }

        public override bool ValidateWeapon(Weapon weapon)
        {
            return weapon_types.Contains(weapon.type);
        }
    }
}
