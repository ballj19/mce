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

        static Player player0 = new Player("Big Jake", 4, 5, 1, 2, 3);
        static Player player1 = new Player("Justin", 1, 2, 3, 4, 5);
        static Player player2 = new Player("Little Jake", 2, 3, 1, 4, 5);
        static Player player3 = new Player("Jared", 2, 3, 5, 4, 1);
        static Player player4 = new Player("Meghan", 4, 3, 5, 1, 2);

        

        static void Main(string[] args)
        {
            Random rnd = new Random();

            players.Add(player0);
            players.Add(player1);
            players.Add(player2);
            players.Add(player3);
            players.Add(player4);

            int player0_total = 0;
            int player1_total = 0;
            int player2_total = 0;
            int player3_total = 0;
            int player4_total = 0;

            for (int i = 0; i < 100; i++)
            {
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
                    if (!top_Locked)
                    {
                        Player top_random = Random_Top(rnd);
                        if (!top_random.locked && top_random.top == top_random.roles.Min())
                        {
                            top_Locked = true;
                            top_random.locked = true;
                            top_player = top_random.name;

                            if (!player0.locked)
                            {
                                player0.roles.Remove(player0.top);
                            }
                            if (!player1.locked)
                            {
                                player1.roles.Remove(player1.top);
                            }
                            if (!player2.locked)
                            {
                                player2.roles.Remove(player2.top);
                            }
                            if (!player3.locked)
                            {
                                player3.roles.Remove(player3.top);
                            }
                            if (!player4.locked)
                            {
                                player4.roles.Remove(player4.top);
                            }
                        }
                    }
                    if (!jg_Locked)
                    {
                        Player jg_random = Random_Jg(rnd);
                        if (!jg_random.locked && jg_random.jg == jg_random.roles.Min())
                        {
                            jg_Locked = true;
                            jg_random.locked = true;
                            jg_player = jg_random.name;

                            if (!player0.locked)
                            {
                                player0.roles.Remove(player0.jg);
                            }
                            if (!player1.locked)
                            {
                                player1.roles.Remove(player1.jg);
                            }
                            if (!player2.locked)
                            {
                                player2.roles.Remove(player2.jg);
                            }
                            if (!player3.locked)
                            {
                                player3.roles.Remove(player3.jg);
                            }
                            if (!player4.locked)
                            {
                                player4.roles.Remove(player4.jg);
                            }
                        }
                    }
                    if (!mid_Locked)
                    {
                        Player mid_random = Random_Mid(rnd);
                        if (!mid_random.locked && mid_random.mid == mid_random.roles.Min())
                        {
                            mid_Locked = true;
                            mid_random.locked = true;
                            mid_player = mid_random.name;

                            if (!player0.locked)
                            {
                                player0.roles.Remove(player0.mid);
                            }
                            if (!player1.locked)
                            {
                                player1.roles.Remove(player1.mid);
                            }
                            if (!player2.locked)
                            {
                                player2.roles.Remove(player2.mid);
                            }
                            if (!player3.locked)
                            {
                                player3.roles.Remove(player3.mid);
                            }
                            if (!player4.locked)
                            {
                                player4.roles.Remove(player4.mid);
                            }
                        }
                    }
                    if (!adc_Locked)
                    {
                        Player adc_random = Random_Adc(rnd);
                        if (!adc_random.locked && adc_random.adc == adc_random.roles.Min())
                        {
                            adc_Locked = true;
                            adc_random.locked = true;
                            adc_player = adc_random.name;

                            if (!player0.locked)
                            {
                                player0.roles.Remove(player0.adc);
                            }
                            if (!player1.locked)
                            {
                                player1.roles.Remove(player1.adc);
                            }
                            if (!player2.locked)
                            {
                                player2.roles.Remove(player2.adc);
                            }
                            if (!player3.locked)
                            {
                                player3.roles.Remove(player3.adc);
                            }
                            if (!player4.locked)
                            {
                                player4.roles.Remove(player4.adc);
                            }
                        }
                    }
                    if (!sup_Locked)
                    {
                        Player sup_random = Random_Sup(rnd);
                        if (!sup_random.locked && sup_random.sup == sup_random.roles.Min())
                        {
                            sup_Locked = true;
                            sup_random.locked = true;
                            sup_player = sup_random.name;

                            if (!player0.locked)
                            {
                                player0.roles.Remove(player0.sup);
                            }
                            if (!player1.locked)
                            {
                                player1.roles.Remove(player1.sup);
                            }
                            if (!player2.locked)
                            {
                                player2.roles.Remove(player2.sup);
                            }
                            if (!player3.locked)
                            {
                                player3.roles.Remove(player3.sup);
                            }
                            if (!player4.locked)
                            {
                                player4.roles.Remove(player4.sup);
                            }
                        }
                    }
                }

                Console.WriteLine(i + "\n");

                player0_total += player0.roles.Min();
                player1_total += player1.roles.Min();
                player2_total += player2.roles.Min();
                player3_total += player3.roles.Min();
                player4_total += player4.roles.Min();
            }

            /*Console.Write("TOP: " + top_player + "\n");
            Console.Write("JG: " + jg_player + "\n");
            Console.Write("MID: " + mid_player + "\n");
            Console.Write("ADC: " + adc_player + "\n");
            Console.Write("SUP: " + sup_player + "\n");*/

            Console.Write("Player 0 " + player0_total / 100.0 + "\n");
            Console.Write("Player 1 " + player1_total / 100.0 + "\n");
            Console.Write("Player 2 " + player2_total / 100.0 + "\n");
            Console.Write("Player 3 " + player3_total / 100.0 + "\n");
            Console.Write("Player 4 " + player4_total / 100.0 + "\n");
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
                    top_total += 600 / player.top;
                    endpoints.Add(600 / player.top);
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

            /*int t0 = 600 / player0.top;
            int t1 = 600 / player1.top + t0;
            int t2 = 600 / player2.top + t1;
            int t3 = 600 / player3.top + t2;
            int t4 = 600 / player4.top + t3;

            if (top_ri >= 1 && top_ri < t0)
            {
                return player0;
            }
            if (top_ri >= t0 && top_ri < t1)
            {
                return player1;
            }
            if (top_ri >= t1 && top_ri < t2)
            {
                return player2;
            }
            if (top_ri >= t2 && top_ri < t3)
            {
                return player3;
            }
            if (top_ri >= t3 && top_ri < t4)
            {
                return player4;
            }

            return null;*/
        }

        static Player Random_Jg(Random rnd)
        {
            int jg_total = 120 * (5 / player0.jg + 5 / player1.jg + 5 / player2.jg + 5 / player3.jg + 5 / player4.jg);

            int jg_ri = rnd.Next(1, jg_total);

            int t0 = 600 / player0.jg;
            int t1 = 600 / player1.jg + t0;
            int t2 = 600 / player2.jg + t1;
            int t3 = 600 / player3.jg + t2;
            int t4 = 600 / player4.jg + t3;

            if (jg_ri >= 1 && jg_ri < t0)
            {
                return player0;
            }
            if (jg_ri >= t0 && jg_ri < t1)
            {
                return player1;
            }
            if (jg_ri >= t1 && jg_ri < t2)
            {
                return player2;
            }
            if (jg_ri >= t2 && jg_ri < t3)
            {
                return player3;
            }
            if (jg_ri >= t3 && jg_ri < t4)
            {
                return player4;
            }

            return null;
        }

        static Player Random_Mid(Random rnd)
        {
            int mid_total = 120 * (5 / player0.mid + 5 / player1.mid + 5 / player2.mid + 5 / player3.mid + 5 / player4.mid);

            int mid_ri = rnd.Next(1, mid_total);

            int t0 = 600 / player0.mid;
            int t1 = 600 / player1.mid + t0;
            int t2 = 600 / player2.mid + t1;
            int t3 = 600 / player3.mid + t2;
            int t4 = 600 / player4.mid + t3;

            if (mid_ri >= 1 && mid_ri < t0)
            {
                return player0;
            }
            if (mid_ri >= t0 && mid_ri < t1)
            {
                return player1;
            }
            if (mid_ri >= t1 && mid_ri < t2)
            {
                return player2;
            }
            if (mid_ri >= t2 && mid_ri < t3)
            {
                return player3;
            }
            if (mid_ri >= t3 && mid_ri < t4)
            {
                return player4;
            }

            return null;
        }

        static Player Random_Adc(Random rnd)
        {
            int adc_total = 120 * (5 / player0.adc + 5 / player1.adc + 5 / player2.adc + 5 / player3.adc + 5 / player4.adc);

            int adc_ri = rnd.Next(1, adc_total);

            int t0 = 600 / player0.adc;
            int t1 = 600 / player1.adc + t0;
            int t2 = 600 / player2.adc + t1;
            int t3 = 600 / player3.adc + t2;
            int t4 = 600 / player4.adc + t3;

            if (adc_ri >= 1 && adc_ri < t0)
            {
                return player0;
            }
            if (adc_ri >= t0 && adc_ri < t1)
            {
                return player1;
            }
            if (adc_ri >= t1 && adc_ri < t2)
            {
                return player2;
            }
            if (adc_ri >= t2 && adc_ri < t3)
            {
                return player3;
            }
            if (adc_ri >= t3 && adc_ri < t4)
            {
                return player4;
            }

            return null;
        }

        static Player Random_Sup(Random rnd)
        {
            int sup_total = 120 * (5 / player0.sup + 5 / player1.sup + 5 / player2.sup + 5 / player3.sup + 5 / player4.sup);

            int sup_ri = rnd.Next(1, sup_total);

            int t0 = 600 / player0.sup;
            int t1 = 600 / player1.sup + t0;
            int t2 = 600 / player2.sup + t1;
            int t3 = 600 / player3.sup + t2;
            int t4 = 600 / player4.sup + t3;

            if (sup_ri >= 1 && sup_ri < t0)
            {
                return player0;
            }
            if (sup_ri >= t0 && sup_ri < t1)
            {
                return player1;
            }
            if (sup_ri >= t1 && sup_ri < t2)
            {
                return player2;
            }
            if (sup_ri >= t2 && sup_ri < t3)
            {
                return player3;
            }
            if (sup_ri >= t3 && sup_ri < t4)
            {
                return player4;
            }

            return null;
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
        public int maxHappy{ get; set; }
        public bool locked { get; set; }
        public List<int> roles = new List<int>(new int[] { 1, 2, 3, 4, 5 });

        public Player(string name, int top, int jg, int mid, int adc, int sup)
        {
            this.name = name;
            this.top = top;
            this.jg = jg;
            this.mid = mid;
            this.adc = adc;
            this.sup = sup;
            this.maxHappy = 1;
            this.locked = false;
        }
    }

}
