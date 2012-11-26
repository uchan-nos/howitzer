using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Howitzer
{
    class BezierMotion : Motion
    {
        public enum Parameter
        {
            SlowLv5, SlowLv4, SlowLv3, SlowLv2, SlowLv1, NoAcceleration,
            RapidLv1, RapidLv2, RapidLv3, RapidLv4, RapidLv5
        }

        private int[][] pascalTriangle = new int[12][] {
                              new int[] {1},
                             new int[] {1,1},
                            new int[] {1,2,1},
                           new int[] {1,3,3,1},
                          new int[] {1,4,6,4,1},
                        new int[] {1,5,10,10,5,1},
                      new int[] {1,6,15,20,15,6,1},
                     new int[] {1,7,21,35,35,21,7,1},
                   new int[] {1,8,28,56,70,56,28,8,1},
                 new int[] {1,9,36,84,126,126,84,36,9,1},
              new int[] {1,10,45,120,210,252,210,120,45,10,1},
            new int[] {1,11,55,165,330,464,464,330,165,55,11,1}
        };
        private int dim;
        private float[] yParam;

        public BezierMotion(Parameter begin, Parameter end)
        {
            int n = 3;
            float y1 = 0, y2 = 1;

            switch (begin)
            {
                case Parameter.SlowLv5:
                case Parameter.SlowLv4:
                case Parameter.SlowLv3:
                case Parameter.SlowLv2:
                case Parameter.SlowLv1:
                    y1 = 0;
                    break;
                case Parameter.NoAcceleration:
                    y1 = 1.0f / 3.0f;
                    break;
                case Parameter.RapidLv1:
                case Parameter.RapidLv2:
                case Parameter.RapidLv3:
                case Parameter.RapidLv4:
                case Parameter.RapidLv5:
                    y1 = 1;
                    break;
            }

            switch (end)
            {
                case Parameter.SlowLv5: y2 = 1; n = 11; break;
                case Parameter.SlowLv4: y2 = 1; n = 9; break;
                case Parameter.SlowLv3: y2 = 1; n = 7; break;
                case Parameter.SlowLv2: y2 = 1; n = 5; break;
                case Parameter.SlowLv1: y2 = 1; n = 3; break;
                case Parameter.NoAcceleration: y2 = 2.0f / 3.0f; n = 3; break;
                case Parameter.RapidLv1: y2 = 0; n = 3; break;
                case Parameter.RapidLv2: y2 = 0; n = 5; break;
                case Parameter.RapidLv3: y2 = 0; n = 7; break;
                case Parameter.RapidLv4: y2 = 0; n = 9; break;
                case Parameter.RapidLv5: y2 = 0; n = 11; break;
            }

            this.dim = n;
            this.yParam = new float[4] { 0, y1, y2, 1 };
        }

        public System.Drawing.PointF Calculate(float t)
        {
            float b = t;
            if (t > 1) b = 1;
            else if (t < 0) b = 0;

            float a = 1 - b;
            float y = 0;

            for (int i = 0; i <= dim; ++i)
            {
                int m = i == 0 ? 0
                      : i == dim ? 3
                      : i <= dim / 2 ? 1
                      : 2;
                y += (float)(pascalTriangle[dim][i] * Math.Pow(a, dim - i) * Math.Pow(b, i) * yParam[m]);
            }

            return new System.Drawing.PointF(y, 0);
        }
    }
}
