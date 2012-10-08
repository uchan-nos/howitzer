using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DxLibDLL;

namespace Howitzer
{
    class Mouse
    {
        public int X
        {
            private set;
            get;
        }

        public int Y
        {
            private set;
            get;
        }

        public bool Left
        {
            private set;
            get;
        }

        public bool Right
        {
            private set;
            get;
        }

        public bool Middle
        {
            private set;
            get;
        }

        /// <summary>
        /// マウスの状態を更新する
        /// </summary>
        /// <returns>成功したら0</returns>
        public int Update()
        {
            int x, y;
            int res = DX.GetMouseInput();

            Left = (res & DX.MOUSE_INPUT_LEFT) != 0;
            Right = (res & DX.MOUSE_INPUT_RIGHT) != 0;
            Middle = (res & DX.MOUSE_INPUT_MIDDLE) != 0;

            res = DX.GetMousePoint(out x, out y);

            if (res == 0)
            {
                X = x;
                Y = y;
            }

            return res;
        }
    }
}
