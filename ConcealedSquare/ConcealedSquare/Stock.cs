using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcealedSquare
{
    class Stock
    {
        static void xMain(string[] args)
        {
            int max = MaxProfit(new int[] { 7,6,5,3,1});
        }

        public static int MaxProfit(int[] prices)
        {
            int max = 0;

            for(int i = 0; i < prices.Length - 1; i++)
            {
                int buy = prices[i];
                int[] possible_sells = new int[prices.Length - i - 1];
                Array.Copy(prices, i + 1, possible_sells, 0, prices.Length - i - 1);

                int high_sale = possible_sells.Max();

                if(high_sale - buy > max)
                {
                    max = high_sale - buy;
                } 
            }

            return max;
        }
    }
}
