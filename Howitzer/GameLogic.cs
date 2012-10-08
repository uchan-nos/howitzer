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
        private Bullet bullet = null;
        private double bulletAzimuth;
        private PolarPoint bulletTarget = null;
        private double bulletSpeed = 7.0;

        private Vector3D<double> howitzerPosition; // 砲の位置（左手座標、画面奥がZ）

        private DebugWindow debugWindow = new DebugWindow();
        private bool directShot = true;

        private int screenX, screenY, screenBits; // 画面解像度（動的に変化してはいけない）

        private int colorSoftImage, depthSoftImage;

        private int fireSound, hitSound, turretMoveSound;
        private int dokuroImage;

        private int dokuroX, dokuroY, dokuroWidth2;

        private SerialPort serialPort = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One);

        private int phase = 0;
        private int waitUntil = 0;

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

            if (DX.GetScreenState(out this.screenX, out this.screenY, out this.screenBits) != 0)
            {
                Error("Failed to get screen state");
                return -1;
            }

            colorSoftImage = DX.MakeXRGB8ColorSoftImage(sensor.KinectSensor.ColorStream.FrameWidth, sensor.KinectSensor.ColorStream.FrameHeight);
            depthSoftImage = DX.MakeXRGB8ColorSoftImage(sensor.KinectSensor.DepthStream.FrameWidth, sensor.KinectSensor.DepthStream.FrameHeight);

            debugWindow.Height = screenY / 2 - 10;
            debugWindow.Width = screenX / 4 - 10;
            debugWindow.Top = screenY - debugWindow.Height - 5;
            debugWindow.Left = 5;

            fireSound = DX.LoadSoundMem(Path.Combine(gconf.DataDirectory.FullName, "fire.wav"));
            hitSound = DX.LoadSoundMem(Path.Combine(gconf.DataDirectory.FullName, "hit.wav"));
            turretMoveSound = DX.LoadSoundMem(Path.Combine(gconf.DataDirectory.FullName, "turret_move.wav"));

            dokuroImage = DX.LoadGraph(Path.Combine(gconf.DataDirectory.FullName, "dokuro.png"));

            serialPort.Open();

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

            bool mouseInScreen = 0 <= mouse.X && mouse.X < screenX && 0 <= mouse.Y && mouse.Y < screenY;

            if (keyboard.GetHitPeriod(DX.KEY_INPUT_AT) == 1)
            {
                directShot = !directShot;
            }

            if (!keyboard.IsHit(DX.KEY_INPUT_SPACE))
            {
                DrawSensorColorImage();
            }
            else
            {
                DrawSensorDepthImage();
            }
            DrawHologram();

            DX.SetFontSize(20);

            UpdateBullet(gameStatus);

            MoveCamera();

            debugWindow.Draw();
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

        private void DrawSensorColorImage()
        {
            unsafe
            {
                int* p = (int*)DX.GetImageAddressSoftImage(colorSoftImage);

                using (var pixels = sensor.GetColorPixels())
                {
                    System.Runtime.InteropServices.Marshal.Copy(pixels.Data, 0, new IntPtr(p), pixels.Width * pixels.Height * 4);
                }
            }
            int tempGraph = DX.CreateGraphFromSoftImage(colorSoftImage);
            DX.DrawExtendGraph(0, 0, screenX, screenY, tempGraph, DX.FALSE);
            DX.DeleteGraph(tempGraph);
        }

        private void DrawSensorDepthImage()
        {
            unsafe
            {
                int* p = (int*)DX.GetImageAddressSoftImage(depthSoftImage);

                using (var pixels = sensor.GetDepthPixels())
                {
                    fixed (short* src0 = &pixels.Data[0])
                    {
                        short* src = src0;

                        for (int y = 0; y < pixels.Height; ++y)
                        {
                            for (int x = 0; x < pixels.Width; ++x)
                            {
                                int c = *src >> 8;
                                //*p = DX.GetColor(c, c, c);
                                *p = (c << 16) | (c << 8) | c;
                                ++p;
                                ++src;
                            }
                        }
                    }

                }
            }
            int tempGraph = DX.CreateGraphFromSoftImage(depthSoftImage);
            DX.DrawExtendGraph(0, 0, screenX, screenY, tempGraph, DX.FALSE);
            DX.DeleteGraph(tempGraph);
        }

        private void DrawHologram()
        {
            int mode, param;
            DX.GetDrawBlendMode(out mode, out param);
            DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, 200);

            if (DX.ChangeFont("メイリオ") != 0)
            {
                Error("Failed to change font");
            }
            DX.SetFontSize(30);
            DX.DrawString(10, 10, string.Format("弾速 {0:F1} m/s", bulletSpeed), DX.GetColor(0, 255, 0), DX.GetColor(0, 0, 255));

            DX.DrawString(10, 40, string.Format("{0},{1}", mouse.X, mouse.Y), DX.GetColor(0, 255, 0));

            DX.DrawLine(0, screenY / 2, screenX, screenY / 2, DX.GetColor(0, 255, 0));
            if (bullet != null)
            {
                DX.DrawCircle((int)(bullet.Horizontal.Position * 100), screenY / 2 - (int)(bullet.Vertical.Position * 100), 3, DX.GetColor(0, 255, 0), DX.TRUE);
            }

            DX.SetDrawBlendMode(mode, param);
        }

        private int GetDepthAt(int x, int y)
        {
            using (var depthPixels = sensor.GetDepthPixels())
            {
                int x_ = x * depthPixels.Width / screenX;
                int y_ = y * depthPixels.Height / screenY;
                return depthPixels.Data[y_ * depthPixels.Width + x_] >> 3;
            }
        }

        private PolarPoint GetPolarPointAt(int x, int y, int depth)
        {
            double azimuth = Math.Atan2(x, this.sensor.KinectSensor.DepthStream.NominalFocalLengthInPixels);
            double elevation = Math.Atan2(y, this.sensor.KinectSensor.DepthStream.NominalFocalLengthInPixels);
            elevation += this.sensor.KinectSensor.ElevationAngle * Math.PI / 180;

            return new PolarPoint(depth, elevation, azimuth);
        }

        private void UpdateBullet(GameStatus gameStatus)
        {
            switch (phase)
            {
                case 0:
                    if (MoveHowitzer(gameStatus))
                    {
                        phase++;
                    }
                    break;
                case 1:
                    if (WaitHowitzer(gameStatus))
                    {
                        phase++;
                    }
                    break;
                case 2:
                    if (FireBullet(gameStatus))
                    {
                        phase++;
                    }
                    break;
                case 3:
                    if (MoveBullet(gameStatus))
                    {
                        phase++;
                    }
                    break;
                case 4:
                    if (WaitDokuro(gameStatus))
                    {
                        phase++;
                    }
                    break;
                default:
                    phase = 0;
                    break;
            }
        }

        private bool MoveHowitzer(GameStatus gameStatus)
        {
            bool mouseInScreen = 0 <= mouse.X && mouse.X < screenX && 0 <= mouse.Y && mouse.Y < screenY;

            if (mouseInScreen && mouse.Left)
            {
                int depthAtMouse = GetDepthAt(mouse.X, mouse.Y);
                int mouseXCenter = mouse.X - screenX / 2;
                int mouseYCenter = screenY / 2 - mouse.Y;
                PolarPoint pp = GetPolarPointAt(mouseXCenter, mouseYCenter, depthAtMouse);

                bulletTarget = pp;

                double srv1Deg = pp.Elevation * 180 / Math.PI;
                double srv2Deg = pp.Azimuth * 180 / Math.PI;
                serialPort.Write(string.Format("srv1:{0}\n", (int)Math.Round(1500 + srv1Deg * 10))); // 1500 usec中心
                serialPort.Write(string.Format("srv2:{0}\n", (int)Math.Round(1500 + srv2Deg * 10))); // 1500 usec中心
                // http://homepage3.nifty.com/rio_i/lab/avr/09pwm.html

                waitUntil = gameStatus.CurrentTimeInMillis + 7000;
                DX.PlaySoundMem(turretMoveSound, DX.DX_PLAYTYPE_BACK);
                //waitUntil = gameStatus.CurrentTimeInMillis;

                return true;
            }

            return false;
        }

        private bool WaitHowitzer(GameStatus gameStatus)
        {
            if (waitUntil < gameStatus.CurrentTimeInMillis)
            {
                return true;
            }

            return false;
        }

        private bool FireBullet(GameStatus gameStatus)
        {
            if (bulletTarget.Length > 0)
            {
                double v0 = bulletSpeed;
                double g = 9.8;

                double[] angles = Util.CalcFiringAngle(bulletTarget.Length / 1000.0, bulletTarget.Elevation, v0, g);
                double firingAngle = angles[directShot ? 0 : 1];
                double sin = Math.Sin(firingAngle), cos = Math.Cos(firingAngle);

                debugWindow.Update("12:FANG", (firingAngle * 180 / Math.PI).ToString("F02"));

                bullet = new Bullet();
                bullet.Horizontal.Acceleration = 0;
                bullet.Horizontal.Velocity = cos * v0;
                bullet.Horizontal.Position = 0.05;
                bullet.Vertical.Acceleration = -g;
                bullet.Vertical.Velocity = sin * v0;
                bullet.Vertical.Position = 0;

                /*
                Console.WriteLine(string.Format("bullet allocated, Vertical a={0:F2} v={1:F2} p={2:F2}",
                    bullet.Vertical.Acceleration, bullet.Vertical.Velocity, bullet.Vertical.Position));
                 * */

                bulletAzimuth = bulletTarget.Azimuth;

                //Console.WriteLine(string.Format("{0:F2},{1:F2}", angles[0] * 180 / Math.PI, angles[1] * 180 / Math.PI));

                DX.PlaySoundMem(fireSound, DX.DX_PLAYTYPE_BACK);
            }

            return true;
        }

        private bool MoveBullet(GameStatus gameStatus)
        {
            if (bullet != null)
            {
                int timeInMillis = gameStatus.CurrentTimeInMillis - gameStatus.PreviousTimeInMillis;

                bullet.Update(timeInMillis);
                debugWindow.Update("10:BL_V", bullet.Vertical.Position.ToString("F2"));
                debugWindow.Update("11:BL_H", bullet.Horizontal.Position.ToString("F2"));

                /*
                Console.WriteLine(string.Format("bullet updated, Vertical a={0:F2} v={1:F2} p={2:F2}",
                    bullet.Vertical.Acceleration, bullet.Vertical.Velocity, bullet.Vertical.Position));
                Console.WriteLine(string.Format("                Horizontal a={0:F2} v={1:F2} p={2:F2}",
                    bullet.Horizontal.Acceleration, bullet.Horizontal.Velocity, bullet.Horizontal.Position));
                 * */


                if (bulletTarget.Length / 1000 * Math.Cos(bulletTarget.Elevation) < bullet.Horizontal.Position
                    && bulletTarget.Length / 1000 * Math.Sin(bulletTarget.Elevation) > bullet.Vertical.Position)
                {
                    DrawBullet(true);
                    bullet = null;
                    DX.PlaySoundMem(hitSound, DX.DX_PLAYTYPE_BACK);

                    waitUntil = gameStatus.CurrentTimeInMillis + 1000;

                    return true;
                }
                {
                    DrawBullet(false);
                }

                return false;
            }
            return true;
        }

        private void DrawBullet(bool hit)
        {
            if (bullet.Horizontal.Position > 0)
            {
                float focalLength = sensor.KinectSensor.DepthStream.NominalFocalLengthInPixels;

                //double projYPos = bullet.Vertical.Position / bullet.Horizontal.Position * focalLength;
                //double projXPos = Math.Tan(bulletAzimuth) * sensor.KinectSensor.DepthStream.NominalFocalLengthInPixels;

                double rHorizon = bullet.Horizontal.Position;
                double projYPos = focalLength * Math.Tan(Math.Atan2(bullet.Vertical.Position, bullet.Horizontal.Position) - sensor.KinectSensor.ElevationAngle / 180.0 * Math.PI);
                double projXPos = rHorizon * Math.Sin(bulletAzimuth) * focalLength / (rHorizon * Math.Cos(bulletAzimuth) + 0.05);

                debugWindow.Update("13:PROJX", projXPos.ToString("F2"));

                double r = Math.Sqrt(Math.Pow(bullet.Vertical.Position, 2) + Math.Pow(bullet.Horizontal.Position, 2));

                int drawYPos = (int)(screenY / 2 - projYPos);
                int drawXPos = (int)(screenX / 2 + projXPos);

                //Console.WriteLine(string.Format("projXPos={0:F2}, projYPos={1:F2}", projXPos, projYPos));

                double drawScale = focalLength / bullet.Horizontal.Position;
                if (hit)
                {
                    int width2 = (int) (0.1 * drawScale / 2);
                    dokuroX = drawXPos;
                    dokuroY = drawYPos;
                    dokuroWidth2 = width2;
                }
                else
                {
                    DX.DrawCircle(drawXPos, drawYPos, (int)(0.02 * drawScale), DX.GetColor(255, 0, 0), DX.TRUE);
                }
            }
        }

        private bool WaitDokuro(GameStatus gameStatus)
        {
            if (waitUntil < gameStatus.CurrentTimeInMillis)
            {
                return true;
            }

            DX.DrawExtendGraph(dokuroX - dokuroWidth2, dokuroY - dokuroWidth2, dokuroX + dokuroWidth2, dokuroY + dokuroWidth2, dokuroImage, DX.TRUE);

            return false;
        }

        private void MoveCamera()
        {
            bool up = keyboard.IsHit(DX.KEY_INPUT_UP);
            bool down = keyboard.IsHit(DX.KEY_INPUT_DOWN);

            try
            {
                if (up && !down)
                {
                    sensor.KinectSensor.ElevationAngle += 5;
                }
                else if (!up && down)
                {
                    sensor.KinectSensor.ElevationAngle -= 5;
                }
                else if (up && down)
                {
                    sensor.KinectSensor.ElevationAngle = 0;
                }
            }
            catch (InvalidOperationException)
            {
            }
        }
    }
}
