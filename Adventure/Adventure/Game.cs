using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace Adventure
{
    class Game
    {
        static void Main(string[] args)
        {
            var character = new Warrior("Jake");

            character.Dead += Game_End;

            character.Hit(50);
            Thread.Sleep(3000);
            character.Heal(20);
            Thread.Sleep(3000);
            character.Hit(50);
            Thread.Sleep(3000);
            character.Hit(50);
            Thread.Sleep(3000);
            character.Hit(50);
            Thread.Sleep(3000);

        }

        private static void Game_End(object source, EventArgs e)
        {
            Console.WriteLine("The game has ended");
            Thread.Sleep(10000);
        }
    }
}
