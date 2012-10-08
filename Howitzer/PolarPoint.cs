using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Howitzer
{
    class PolarPoint
    {
        public double Length
        {
            private set;
            get;
        }
        public double Elevation
        {
            private set;
            get;
        }
        public double Azimuth
        {
            private set;
            get;
        }

        public PolarPoint(double length, double elevation, double azimuth)
        {
            Length = length;
            Elevation = elevation;
            Azimuth = azimuth;
        }
    }
}
