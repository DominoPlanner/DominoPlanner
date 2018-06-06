using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiralTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            double näherung = newton(Math.PI, Math.PI + 1, 0.1);
            for (int i = 0; i < 10000; i++)
            {
                newton(Math.PI, Math.PI + 1, 0.1);
            }
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
           
            Console.WriteLine("Näherung " + näherung + ", Zeit: " + elapsedMs);
        }
        static double newton(double theta, double startwert, double abstand)
        {
            double a = 2;
            double cos = Math.Cos(theta - startwert);
            double result = startwert - a* (- theta * theta + 2 * theta * startwert * cos - startwert * startwert +
                Math.Sqrt((theta * theta - 2 * theta * startwert * cos + startwert * startwert)) * abstand) /
                (a*(theta * startwert * Math.Sin(theta - startwert) + theta * cos - startwert));
            if (Math.Abs((result - startwert)) < 0.0001) return result;
            else return newton(theta, result, abstand);
        }
    }

}
