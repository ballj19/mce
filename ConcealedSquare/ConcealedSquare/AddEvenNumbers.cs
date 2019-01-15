using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcealedSquare
{
    class AddEvenNumbers
    {
        static void Main(string[] args)
        {
            double Kilometers = Double.Parse(Console.ReadLine());

            Console.WriteLine(Kilometers * 1000);
        }

    }

    public sealed class Circle
    {
        private double radius = 10;

        public double Calculate(Func<double, double> op)
        {
            return op(radius);
        }
    }
}
