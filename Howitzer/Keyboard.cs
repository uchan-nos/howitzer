using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DxLibDLL;

namespace Howitzer
{
    class Keyboard
    {
        private byte[] keyStates = new byte[256];
        private uint[] keyHitPeriod = new uint[256];

        /// <summary>
        /// キーボードの押下状態を更新する
        /// </summary>
        /// <returns>成功したら0</returns>
        public int Update()
        {
            int res = DX.GetHitKeyStateAll(out keyStates[0]);
            if (res != 0)
            {
                return res;
            }

            for (int i = 0; i < keyStates.Length; ++i)
            {
                if (IsHit(i))
                {
                    keyHitPeriod[i]++;
                }
                else
                {
                    keyHitPeriod[i] = 0;
                }
            }

            return 0;
        }

        /// <summary>
        /// キーの押下状態を返す
        /// </summary>
        /// <param name="keyName">キーの名前 KEY_INPUT_XXX</param>
        /// <returns>キーが押されていたらtrue</returns>
        public bool IsHit(int keyName)
        {
            return keyStates[keyName] == 1;
        }

        /// <summary>
        /// キーの押下時間を返す
        /// </summary>
        /// <param name="keyName">キーの名前 KEY_INPUT_XXX</param>
        /// <returns>キーが押されていた時間（フレーム数）</returns>
        public uint GetHitPeriod(int keyName)
        {
            return keyHitPeriod[keyName];
        }
    }
}
