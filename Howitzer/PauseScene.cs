using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DxLibDLL;

namespace Howitzer
{
    class PauseScene : Scene
    {
        protected override void _Update(GameStatus status)
        {
            base._Update(status);

            if (GameSettings.GameLogic.Keyborad.IsHit(DX.KEY_INPUT_RETURN))
            {
                EndScene();
            }
        }

        protected override void _Draw()
        {
            base._Draw();

            int blendMode, blendParam;
            DX.GetDrawBlendMode(out blendMode, out blendParam);
            DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, 127);
            DX.DrawBox(0, 0, GameSettings.WindowWidth, GameSettings.WindowHeight, DX.GetColor(0, 0, 0), DX.TRUE);
            DX.SetDrawBlendMode(blendMode, blendParam);
        }
    }
}
