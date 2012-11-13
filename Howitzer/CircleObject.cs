using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Howitzer
{
    class CircleObject : BasicShapeObject
    {
        /// <summary>
        /// 円の半径
        /// </summary>
        public int Radius
        {
            get;
            set;
        }

        protected override void _Init(GameSettings settings)
        {
            base._Init(settings);
            Radius = 10;
        }

        protected override void _Draw()
        {
            base._Draw();
            DxLibDLL.DX.DrawCircle(X, Y, Radius, Color, Fill ? DxLibDLL.DX.TRUE : DxLibDLL.DX.FALSE);
        }
    }
}
