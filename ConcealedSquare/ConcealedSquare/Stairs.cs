using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcealedSquare
{
    class Stairs
    {
        static void xMain(string[] args)
        {
            var result = ClimbStairs(44);
        }

        static Dictionary<int, int> cache = new Dictionary<int, int>();

        public static int ClimbStairs(int n)
        {
            if(cache.ContainsKey(n))
            {
                return cache[n];
            }


            if(n == 0)
            {
                return 0;
            }
            if(n == 1)
            {
                return 1;
            }
            if(n == 2)
            {
                return 2;
            }

            var result = ClimbStairs(n - 1) + ClimbStairs(n - 2);
            cache.Add(n, result);

            return result;

        }
    }
}
