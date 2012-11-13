using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Howitzer
{
    class BasicShapeObject : GameObject
    {
        /// <summary>
        /// オブジェクトの横位置
        /// </summary>
        public int X
        {
            get;
            set;
        }

        /// <summary>
        /// オブジェクトの縦位置
        /// </summary>
        public int Y
        {
            get;
            set;
        }

        /// <summary>
        /// オブジェクトの色
        /// </summary>
        public int Color
        {
            get;
            set;
        }

        /// <summary>
        /// 塗りつぶすかどうか
        /// </summary>
        public bool Fill
        {
            get;
            set;
        }

        protected override void _Init(GameSettings settings)
        {
            X = settings.WindowWidth / 2;
            Y = settings.WindowHeight / 2;
            Color = DxLibDLL.DX.GetColor(0, 0, 0);
            Fill = false;
        }
    }
}
