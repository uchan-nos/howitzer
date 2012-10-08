using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Howitzer
{
    static class Util
    {
        /// <summary>
        /// 与えられたサイン・コサインの値からラジアン値を求める
        /// </summary>
        /// <param name="sin">サイン値</param>
        /// <param name="cos">コサイン値</param>
        /// <returns>ラジアン値 -PI &lt;= x &lt;= PI</returns>
        public static double CalcAngle(double sin, double cos)
        {
            double angleSin = Math.Asin(sin); // -90 から 90
            double angleCos = Math.Acos(cos); // 0 から 180

            if (angleSin >= 0)
            {
                return angleCos;
            }
            else
            {
                return -angleCos;
            }
        }

        public static double[] CalcFiringAngle(double r, double el, double v0, double g)
        {
            double sin = Math.Sin(el);
            double cos = Math.Cos(el);
            double alpha = CalcAngle(-sin, cos);
            double asin = Math.Asin(sin + g * r * Math.Pow(cos / v0, 2));
            return new double[] {(asin - alpha) / 2, (Math.PI - asin - alpha) / 2};
        }

        static void Main(string[] args)
        {
            Func<double[], string> f = x => string.Format("{0:F2},{1:F2}", x[0] * 180 / Math.PI, x[1] * 180 / Math.PI);
            Console.WriteLine(f(CalcFiringAngle(1000, 0, 10, 9.8)));
        }

        public static double CalcAngleOfTwoVector(Vector3D<double> a, Vector3D<double> b)
        {
            double dotProd = a.X * b.X + a.Y * b.Y + a.Z + b.Z;
            double aAbs = Math.Sqrt(CalcLengthPow2(a));
            double bAbs = Math.Sqrt(CalcLengthPow2(b));
            double cos = dotProd / (aAbs * bAbs);

            if (cos < -1)
            {
                cos = -1;
            }
            else if (cos > 1)
            {
                cos = 1;
            }

            return Math.Acos(cos);
        }

        public static Vector3D<double> ConvertCoordinateLeftHand(PolarPoint p)
        {
            Vector3D<double> v = new Vector3D<double>();
            v.Y = p.Length * Math.Sin(p.Elevation);

            double cos = p.Length * Math.Cos(p.Elevation);
            v.X = Math.Sin(p.Azimuth) * cos;
            v.Z = Math.Cos(p.Azimuth) * cos;

            return v;
        }

        public static double CalcLengthPow2(Vector3D<double> v)
        {
            return Math.Pow(v.X, 2) + Math.Pow(v.Y, 2) + Math.Pow(v.Z, 2);
        }

        /*
        public static PolarPoint CalcTargetPositionFromHowitzer(PolarPoint targetPosition, Vector3D<double> howitzerPosition)
        {
            Vector3D<double> targetPosition_ = ConvertCoordinateLeftHand(targetPosition);

            double angleOfTwo = CalcAngleOfTwoVector(targetPosition_, howitzerPosition);
            double howitzerPositionLengthPow2 = CalcLengthPow2(howitzerPosition);

            double newR = Math.Sqrt(Math.Pow(targetPosition.Length, 2) + howitzerPositionLengthPow2
                - 2 * targetPosition.Length * Math.Sqrt(howitzerPositionLengthPow2) * Math.Cos(angleOfTwo));
        }
         * */

        /// <summary>
        /// dataディレクトリを検索する。
        /// まずカレントディレクトリの中でdataディレクトリを探す。
        /// 見つからなければ1段ずつ親ディレクトリをさかのぼりながら探す。
        /// </summary>
        /// <returns></returns>
        public static DirectoryInfo GetDataDirectory()
        {
            DirectoryInfo current = new DirectoryInfo(System.Environment.CurrentDirectory);
            DirectoryInfo data = SearchDirectory(current, "data");

            if (data != null)
            {
                return data;
            }

            DirectoryInfo dir = current.Parent;
            while (data == null && dir != null && dir.Exists)
            {
                data = SearchDirectory(dir, "data");
                dir = dir.Parent;
            }
            return data;
        }

        /// <summary>
        /// 指定されたディレクトリの中に含まれる指定された名前のディレクトリを検索する
        /// </summary>
        /// <param name="dir">テストするパス</param>
        /// <param name="name">探したいディレクトリの名前</param>
        /// <returns>名前に合致するディレクトリへのパス（見つからなければnull）</returns>
        public static DirectoryInfo SearchDirectory(DirectoryInfo dir, string name)
        {
            DirectoryInfo[] found = dir.GetDirectories(name);
            if (found.Length > 0)
            {
                return found[0];
            }
            return null;
        }
    }
}
