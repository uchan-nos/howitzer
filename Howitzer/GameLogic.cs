using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DxLibDLL;
using System.IO;
using System.IO.Ports;

namespace Howitzer
{
    class GameLogic
    {
        private Keyboard keyboard = new Keyboard();
        private Mouse mouse = new Mouse();
        private Sensor sensor = new Sensor();
        private DebugWindow debugWindow = new DebugWindow();
        private GameSettings gameSettings = new GameSettings();
        private ServoController serialPort = new ServoController("COM3");

        public Keyboard Keyborad
        {
            get
            {
                return keyboard;
            }
        }

        public Mouse Mouse
        {
            get
            {
                return mouse;
            }
        }

        public Sensor Sensor
        {
            get
            {
                return sensor;
            }
        }

        public DebugWindow DebugWindow
        {
            get
            {
                return debugWindow;
            }
        }

        public GameSettings GameSettings
        {
            get
            {
                return gameSettings;
            }
        }

        private SceneStack sceneStack = new SceneStack();

        /// <summary>
        /// 初期化する
        /// </summary>
        /// <returns>成功したら0</returns>
        public int Init()
        {
            Configuration gconf = Configuration.GetGlobal();

            if (sensor.Init() != 0)
            {
                Error("Failed to initialize Kinect sensor");
                return -1;
            }

            int screenX, screenY, screenBits;
            if (DX.GetScreenState(out screenX, out screenY, out screenBits) != 0)
            {
                Error("Failed to get screen state");
                return -1;
            }
            gameSettings.WindowWidth = screenX;
            gameSettings.WindowHeight = screenY;
            gameSettings.ScreenBits = screenBits;
            gameSettings.GameLogic = this;

            debugWindow.Height = screenY / 2 - 10;
            debugWindow.Width = screenX / 4 - 10;
            debugWindow.Top = screenY - debugWindow.Height - 5;
            debugWindow.Left = 5;

            //serialPort.Open();

            var mainScene = new MainScene();
            mainScene.Mouse = mouse;
            mainScene.Keyboard = keyboard;
            mainScene.Sensor = sensor;
            mainScene.SerialPort = serialPort;
            mainScene.DebugWindow = debugWindow;
            mainScene.Init(gameSettings);
            sceneStack.Push(mainScene);

            return 0;
        }

        public void End()
        {
            serialPort.Close();
        }

        /// <summary>
        /// ゲームロジックを1フレーム分実行する
        /// </summary>
        /// <param name="gameStatus">ゲームの状態</param>
        /// <returns>ゲームループを抜ける場合はtrue</returns>
        public bool Run(GameStatus gameStatus)
        {
            if (UpdateStatus() != 0)
            {
                return true;
            }

            int screenX, screenY, screenBits;
            if (DX.GetScreenState(out screenX, out screenY, out screenBits) != 0)
            {
                Error("Failed to get screen state");
                return true;
            }
            gameSettings.WindowWidth = screenX;
            gameSettings.WindowHeight = screenY;
            gameSettings.ScreenBits = screenBits;

            sceneStack.Update(gameStatus);
            sceneStack.Draw();

            return false;
        }

        private void Error(string msg)
        {
            Console.WriteLine(msg);
        }

        private int UpdateStatus()
        {
            int res;

            res = keyboard.Update();
            if (res != 0)
            {
                Error("Failed to update key status");
                return res;
            }

            res = mouse.Update();
            if (res != 0)
            {
                Error("Failed to update mouse status");
                return res;
            }

            return 0;
        }
    }
}
