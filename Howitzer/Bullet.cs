using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Howitzer
{
    class Bullet
    {
        public class KineticParameter
        {
            public double Acceleration
            {
                set;
                get;
            }

            public double Velocity
            {
                set;
                get;
            }

            public double Position
            {
                set;
                get;
            }

            public void Update(int timeInMillis)
            {
                double newVelocity = Velocity + Acceleration * timeInMillis / 1000.0;
                double newPosition = Position + Velocity * timeInMillis / 1000.0;
                Velocity = newVelocity;
                Position = newPosition;
            }
        }

        public KineticParameter Horizontal
        {
            private set;
            get;
        }

        public KineticParameter Vertical
        {
            private set;
            get;
        }

        public Bullet()
        {
            Horizontal = new KineticParameter();
            Vertical = new KineticParameter();
        }

        public void Update(int timeInMillis)
        {
            Horizontal.Update(timeInMillis);
            Vertical.Update(timeInMillis);
        }
    }
}
