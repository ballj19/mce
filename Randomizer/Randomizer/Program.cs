using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer
{
    class Program
    {
        static List<Player> players = new List<Player>();
        
        static void Main(string[] args)
        {
            Random rnd = new Random();
            List<string> rolesList = new List<string>(new string[] { "top", "jg", "mid", "adc", "sup" });
            
            players.Add(new Player("Big Jake", 1, 2, 3, 4, 5));
            players.Add(new Player("Justin", 1, 2, 3, 4, 5));
            players.Add(new Player("Little Jake", 4, 5, 1, 2, 3));
            players.Add(new Player("Randy", 3, 4, 5, 1, 2));
            players.Add(new Player("Clay", 2, 3, 4, 5, 1));

            int player0total = 0;
            int player1total = 0;
            int player2total = 0;
            int player3total = 0;
            int player4total = 0;

            int dr_int = 0;
            double[] player_dr = { 0, 0, 0, 0, 0 }; //desireability rating

            foreach (Player player in players)
            {
                double top_dr = 0;
                double jg_dr = 0;
                double mid_dr = 0;
                double adc_dr = 0;
                double sup_dr = 0;

                foreach (Player player_role in players)
                {
                    top_dr += player_role.top;
                }
                top_dr = top_dr / (double)player.top;

                foreach (Player player_role in players)
                {
                    jg_dr += player_role.jg;
                }
                jg_dr = jg_dr / (double)player.jg;

                foreach (Player player_role in players)
                {
                    mid_dr += player_role.mid;
                }
                mid_dr = mid_dr / (double)player.mid;

                foreach (Player player_role in players)
                {
                    adc_dr += player_role.adc;
                }
                adc_dr = adc_dr / (double)player.adc;

                foreach (Player player_role in players)
                {
                    sup_dr += player_role.sup;
                }
                sup_dr = sup_dr / (double)player.sup;

                player_dr[dr_int] = top_dr + jg_dr + mid_dr + adc_dr + sup_dr;

                dr_int++;
            }

            int n = rolesList.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                string value = rolesList[k];
                rolesList[k] = rolesList[n];
                rolesList[n] = value;
            }

            for (int i = 0; i < 1000; i++)
            {
                players.Clear();
                players.Add(new Player("Big Jake", 1, 2, 3, 4, 5));
                players.Add(new Player("Justin", 1, 2, 3, 4, 5));
                players.Add(new Player("Little Jake", 4, 5, 1, 2, 3));
                players.Add(new Player("Randy", 3, 4, 5, 1, 2));
                players.Add(new Player("Clay", 2, 3, 4, 5, 1));

                n = rolesList.Count;
                while (n > 1)
                {
                    n--;
                    int k = rnd.Next(n + 1);
                    string value = rolesList[k];
                    rolesList[k] = rolesList[n];
                    rolesList[n] = value;
                }

                string top_player = "";
                string jg_player = "";
                string mid_player = "";
                string adc_player = "";
                string sup_player = "";

                bool top_Locked = false;
                bool jg_Locked = false;
                bool mid_Locked = false;
                bool adc_Locked = false;
                bool sup_Locked = false;

                while (!top_Locked || !jg_Locked || !mid_Locked || !adc_Locked || !sup_Locked)
                {
                    foreach(string role in rolesList)
                    {
                        if(role == "top")
                        {
                            if (!top_Locked)
                            {
                                Player top_random = Random_Top(rnd);
                                if (!top_random.locked && top_random.top == 1)
                                {
                                    top_Locked = true;
                                    top_random.locked = true;
                                    top_player = top_random.name;
                                    top_random.happyIncrementor += top_random.roles[0];

                                    foreach (Player player in players)
                                    {
                                        if (!player.locked)
                                        {
                                            if (player.jg > player.top)
                                            {
                                                player.jg--;
                                            }
                                            if (player.mid > player.top)
                                            {
                                                player.mid--;
                                            }
                                            if (player.adc > player.top)
                                            {
                                                player.adc--;
                                            }
                                            if (player.sup > player.top)
                                            {
                                                player.sup--;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if(role == "jg")
                        {
                            if (!jg_Locked)
                            {
                                Player jg_random = Random_Jg(rnd);
                                if (!jg_random.locked && jg_random.jg == 1)
                                {
                                    jg_Locked = true;
                                    jg_random.locked = true;
                                    jg_player = jg_random.name;
                                    jg_random.happyIncrementor += jg_random.roles[1];

                                    foreach (Player player in players)
                                    {
                                        if (!player.locked)
                                        {
                                            if (player.top > player.jg)
                                            {
                                                player.top--;
                                            }
                                            if (player.mid > player.jg)
                                            {
                                                player.mid--;
                                            }
                                            if (player.adc > player.jg)
                                            {
                                                player.adc--;
                                            }
                                            if (player.sup > player.jg)
                                            {
                                                player.sup--;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if(role == "mid")
                        {
                            if (!mid_Locked)
                            {
                                Player mid_random = Random_Mid(rnd);
                                if (!mid_random.locked && mid_random.mid == 1)
                                {
                                    mid_Locked = true;
                                    mid_random.locked = true;
                                    mid_player = mid_random.name;
                                    mid_random.happyIncrementor += mid_random.roles[2];

                                    foreach (Player player in players)
                                    {
                                        if (!player.locked)
                                        {
                                            if (player.top > player.mid)
                                            {
                                                player.top--;
                                            }
                                            if (player.jg > player.mid)
                                            {
                                                player.jg--;
                                            }
                                            if (player.adc > player.mid)
                                            {
                                                player.adc--;
                                            }
                                            if (player.sup > player.mid)
                                            {
                                                player.sup--;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if(role == "adc")
                        {
                            if (!adc_Locked)
                            {
                                Player adc_random = Random_Adc(rnd);
                                if (!adc_random.locked && adc_random.adc == 1)
                                {
                                    adc_Locked = true;
                                    adc_random.locked = true;
                                    adc_player = adc_random.name;
                                    adc_random.happyIncrementor += adc_random.roles[3];

                                    foreach (Player player in players)
                                    {
                                        if (!player.locked)
                                        {
                                            if (player.top > player.adc)
                                            {
                                                player.top--;
                                            }
                                            if (player.jg > player.adc)
                                            {
                                                player.jg--;
                                            }
                                            if (player.mid > player.adc)
                                            {
                                                player.mid--;
                                            }
                                            if (player.sup > player.adc)
                                            {
                                                player.sup--;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if(role == "sup")
                        {
                            if (!sup_Locked)
                            {
                                Player sup_random = Random_Sup(rnd);
                                if (!sup_random.locked && sup_random.sup == 1)
                                {
                                    sup_Locked = true;
                                    sup_random.locked = true;
                                    sup_player = sup_random.name;
                                    sup_random.happyIncrementor += sup_random.roles[4];

                                    foreach (Player player in players)
                                    {
                                        if (!player.locked)
                                        {
                                            if (player.top > player.sup)
                                            {
                                                player.top--;
                                            }
                                            if (player.jg > player.sup)
                                            {
                                                player.jg--;
                                            }
                                            if (player.mid > player.sup)
                                            {
                                                player.mid--;
                                            }
                                            if (player.adc > player.sup)
                                            {
                                                player.adc--;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                player0total += players[0].happyIncrementor;
                player1total += players[1].happyIncrementor;
                player2total += players[2].happyIncrementor;
                player3total += players[3].happyIncrementor;
                player4total += players[4].happyIncrementor;
                int happytotal = players[0].happyIncrementor + players[1].happyIncrementor + players[2].happyIncrementor + players[3].happyIncrementor + players[4].happyIncrementor;
            }

            /*Console.Write("TOP: " + top_player + "\n");
            Console.Write("JG: " + jg_player + "\n");
            Console.Write("MID: " + mid_player + "\n");
            Console.Write("ADC: " + adc_player + "\n");
            Console.Write("SUP: " + sup_player + "\n");*/

            Console.Write("Player 0 " + player0total / 1000.0 + " " + scale_rating(player_dr[0]) + "\n");
            Console.Write("Player 1 " + player1total / 1000.0 + " " + scale_rating(player_dr[1]) + "\n");
            Console.Write("Player 2 " + player2total / 1000.0 + " " + scale_rating(player_dr[2]) + "\n");
            Console.Write("Player 3 " + player3total / 1000.0 + " " + scale_rating(player_dr[3]) + "\n");
            Console.Write("Player 4 " + player4total / 1000.0 + " " + scale_rating(player_dr[4]) + "\n");
        }

        static Player Random_Top(Random rnd)
        {
            int top_total = 0;

            List<int> endpoints = new List<int>();
            endpoints.Add(1);

            List<Player> possiblePlayers = new List<Player>();

            foreach (Player player in players)
            {
                if (!player.locked)
                {
                    endpoints.Add(top_total + 600 / player.top);
                    top_total += 600 / player.top;
                    possiblePlayers.Add(player);
                }
            }

            int top_ri = rnd.Next(1, top_total);

            int i = 0;

            foreach (Player player in possiblePlayers)
            {
                if (top_ri >= endpoints[i] && top_ri < endpoints[i + 1])
                {
                    return player;
                }
                i++;
            }

            return null;
        }

        static Player Random_Jg(Random rnd)
        {
            int jg_total = 0;

            List<int> endpoints = new List<int>();
            endpoints.Add(1);

            List<Player> possiblePlayers = new List<Player>();

            foreach (Player player in players)
            {
                if (!player.locked)
                {
                    endpoints.Add(jg_total + 600 / player.jg);
                    jg_total += 600 / player.jg;
                    possiblePlayers.Add(player);
                }
            }

            int jg_ri = rnd.Next(1, jg_total);

            int i = 0;

            foreach (Player player in possiblePlayers)
            {
                if (jg_ri >= endpoints[i] && jg_ri < endpoints[i + 1])
                {
                    return player;
                }
                i++;
            }

            return null;
        }

        static Player Random_Mid(Random rnd)
        {
            int mid_total = 0;

            List<int> endpoints = new List<int>();
            endpoints.Add(1);

            List<Player> possiblePlayers = new List<Player>();

            foreach (Player player in players)
            {
                if (!player.locked)
                {
                    endpoints.Add(mid_total + 600 / player.mid);
                    mid_total += 600 / player.mid;
                    possiblePlayers.Add(player);
                }
            }

            int mid_ri = rnd.Next(1, mid_total);

            int i = 0;

            foreach (Player player in possiblePlayers)
            {
                if (mid_ri >= endpoints[i] && mid_ri < endpoints[i + 1])
                {
                    return player;
                }
                i++;
            }

            return null;
        }

        static Player Random_Adc(Random rnd)
        {
            int adc_total = 0;

            List<int> endpoints = new List<int>();
            endpoints.Add(1);

            List<Player> possiblePlayers = new List<Player>();

            foreach (Player player in players)
            {
                if (!player.locked)
                {
                    endpoints.Add(adc_total + 600 / player.adc);
                    adc_total += 600 / player.adc;
                    possiblePlayers.Add(player);
                }
            }

            int adc_ri = rnd.Next(1, adc_total);

            int i = 0;

            foreach (Player player in possiblePlayers)
            {
                if (adc_ri >= endpoints[i] && adc_ri < endpoints[i + 1])
                {
                    return player;
                }
                i++;
            }

            return null;
        }

        static Player Random_Sup(Random rnd)
        {
            int sup_total = 0;

            List<int> endpoints = new List<int>();
            endpoints.Add(1);

            List<Player> possiblePlayers = new List<Player>();

            foreach (Player player in players)
            {
                if (!player.locked)
                {
                    endpoints.Add(sup_total + 600 / player.sup);
                    sup_total += 600 / player.sup;
                    possiblePlayers.Add(player);
                }
            }

            int sup_ri = rnd.Next(1, sup_total);

            int i = 0;

            foreach (Player player in possiblePlayers)
            {
                if (sup_ri >= endpoints[i] && sup_ri < endpoints[i + 1])
                {
                    return player;
                }
                i++;
            }

            return null;
        }

        static double scale_rating(double value)
        {
            return Math.Round(value * 6.75676 - 168.919,2);
        }
    }

    class Player
    {
        public string name { get; set; }
        public int top { get; set; }
        public int jg { get; set; }
        public int mid { get; set; }
        public int adc { get; set; }
        public int sup { get; set; }
        public int happyIncrementor{ get; set; }
        public bool locked { get; set; }
        public List<int> roles;

        public Player(string name, int top, int jg, int mid, int adc, int sup)
        {
            this.name = name;
            this.top = top;
            this.jg = jg;
            this.mid = mid;
            this.adc = adc;
            this.sup = sup;
            this.happyIncrementor = 0;
            this.locked = false;
            this.roles = new List<int>(new int[] { this.top, this.jg, this.mid, this.adc, this.sup });
        }
    }

}
