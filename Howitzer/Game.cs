using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DxLibDLL;

namespace Howitzer
{
    class Game
    {
        private GameStatus gameStatus = null;
        private GameLogic gameLogic = null;

        /// <summary>
        /// ゲームループを実行する
        /// </summary>
        public void Run()
        {
            if (Init() != 0)
            {
                Error("Failed to initialize");
                return;
            }

            this.gameStatus = new GameStatus();
            this.gameLogic = new GameLogic();

            this.gameStatus.PreviousTimeInMillis = this.gameStatus.CurrentTimeInMillis = 0;
            if (this.gameLogic.Init() != 0)
            {
                Error("Failed to initialize game logic");
                return;
            }

            int count = 0;
            while (LoopFunction() == 0)
            {
                count++;

                // 時刻更新
                this.gameStatus.PreviousTimeInMillis = this.gameStatus.CurrentTimeInMillis;
                this.gameStatus.CurrentTimeInMillis = DX.GetNowCount();

                int begin = DX.GetNowCount();
                if (gameLogic.Run(gameStatus))
                {
                    break;
                }
            }

            gameLogic.End();
        }

        /// <summary>
        /// DXライブラリを初期化し、解像度などを設定する。
        /// 他のメソッドを使う前に呼び出す必要がある。
        /// </summary>
        /// <returns>成功したら0、失敗したら-1</returns>
        private int Init()
        {
            if (DX.ChangeWindowMode(DX.TRUE) != DX.DX_CHANGESCREEN_OK)
            {
                Error("Failed to set to window mode");
                return -1;
            }

            if (DX.DxLib_Init() != 0)
            {
                Error("Failed to initialize DX Library");
                return -1;
            }

            if (DX.SetGraphMode(640, 480, 32) != DX.DX_CHANGESCREEN_OK)
            {
                Error("This graphic card must support 640x480 32bits color");
                return -1;
            }

            if (DX.SetDrawScreen(DX.DX_SCREEN_BACK) != 0)
            {
                Error("SetDrawScreen() != 0");
                return -1;
            }

            return 0;
        }

        private int LoopFunction()
        {
            if (DX.ScreenFlip() == 0 && DX.ProcessMessage() == 0 && DX.ClearDrawScreen() == 0)
            {
                return 0;
            }
            return -1;
        }
        
        private void Error(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
