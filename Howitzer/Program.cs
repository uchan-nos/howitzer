using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DxLibDLL;
using System.IO;

namespace Howitzer
{
    class Program
    {
        static void Main(string[] args)
        {
            Configuration.GetGlobal().DataDirectory = Util.GetDataDirectory();

            Game game = new Game();
            game.Run();
        }

        static void Error(string msg)
        {
            Console.WriteLine(msg);
        }

        static List<double> CalcElevationAngle(double v0, double x, double y, double g)
        {
            var angles = new List<double>();

            var r = Math.Sqrt(x * x + y * y);
            var v02 = v0 * v0;
            var theta_alpha1 = Math.Asin((g * x * x + y * v02) / (v02 * r));
            if (double.IsNaN(theta_alpha1))
            {
                return angles;
            }

            var alpha_cos = Math.Acos(x / r);
            var alpha_sin = Math.Asin(-y / r);
            var alpha = alpha_sin >= 0 ? alpha_cos : 2 * Math.PI - alpha_cos;

            var theta_alpha2 = Math.PI - theta_alpha1;
            if (Math.Abs(theta_alpha1 - theta_alpha2) < 1e-6)
            {
                angles.Add((theta_alpha1 - alpha) / 2);
            }
            else
            {
                angles.Add((theta_alpha1 - alpha) / 2);
                angles.Add((theta_alpha2 - alpha) / 2);
            }

            return angles;
        }
    }
}
