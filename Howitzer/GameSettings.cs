using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Howitzer
{
    class GameSettings
    {
        public int WindowWidth
        {
            set;
            get;
        }

        public int WindowHeight
        {
            set;
            get;
        }

        public int ScreenBits
        {
            set;
            get;
        }

        public GameLogic GameLogic
        {
            get;
            set;
        }
    }
}
