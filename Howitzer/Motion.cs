using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Howitzer
{
    /// <summary>
    /// アニメーションの動きを表す
    /// </summary>
    interface Motion
    {
        System.Drawing.PointF Calculate(float t);
    }
}
