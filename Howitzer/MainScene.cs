using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DxLibDLL;
using System.IO;

namespace Howitzer
{
    class MainScene : Scene
    {
        public Mouse Mouse
        {
            get;
            set;
        }

        public Keyboard Keyboard
        {
            get;
            set;
        }

        public Sensor Sensor
        {
            get;
            set;
        }

        public ServoController SerialPort
        {
            get;
            set;
        }

        public DebugWindow DebugWindow
        {
            get;
            set;
        }

        private Bullet bullet = null;
        private double bulletAzimuth;
        private double bulletSpeed = 7.0;
        private PolarPoint bulletTarget = null;
        private double firingAngle;
        private bool bulletHit = false;

        private int fireSound, hitSound, turretMoveSound;
        private int dokuroImage;
        private int colorSoftImage, depthSoftImage;

        private int dokuroX, dokuroY, dokuroWidth2;
        private bool drawDokuro = false;

        private int phase = 0;
        private int waitUntil = 0;

        private bool directShot = true;
        private List<GameObject> gameObjects = new List<GameObject>();


        protected override void _Init(GameSettings settings)
        {
            base._Init(settings);

            Configuration gconf = Configuration.GetGlobal();

            colorSoftImage = DX.MakeXRGB8ColorSoftImage(Sensor.KinectSensor.ColorStream.FrameWidth, Sensor.KinectSensor.ColorStream.FrameHeight);
            depthSoftImage = DX.MakeXRGB8ColorSoftImage(Sensor.KinectSensor.DepthStream.FrameWidth, Sensor.KinectSensor.DepthStream.FrameHeight);

            fireSound = DX.LoadSoundMem(Path.Combine(gconf.DataDirectory.FullName, "fire.wav"));
            hitSound = DX.LoadSoundMem(Path.Combine(gconf.DataDirectory.FullName, "hit.wav"));
            turretMoveSound = DX.LoadSoundMem(Path.Combine(gconf.DataDirectory.FullName, "turret_move.wav"));

            dokuroImage = DX.LoadGraph(Path.Combine(gconf.DataDirectory.FullName, "dokuro.png"));

        }

        protected override void _Update(GameStatus status)
        {
            base._Update(status);
            UpdateBullet(status);

            if (Keyboard.GetHitPeriod(DX.KEY_INPUT_AT) == 1)
            {
                directShot = !directShot;
            }

            foreach (var obj in gameObjects)
            {
                obj.Update(status);
            }

            if (Keyboard.IsHit(DX.KEY_INPUT_ESCAPE))
            {
                PauseScene s = new PauseScene();
                s.Init(GameSettings);
                CallScene(s);
            }
        }

        protected override void _Draw()
        {

            int screenX = GameSettings.WindowWidth;
            int screenY = GameSettings.WindowHeight;

            bool mouseInScreen = 0 <= Mouse.X && Mouse.X < screenX && 0 <= Mouse.Y && Mouse.Y < screenY;


            if (!Keyboard.IsHit(DX.KEY_INPUT_SPACE))
            {
                DrawSensorColorImage();
            }
            else
            {
                DrawSensorDepthImage();
            }


            if (this.Status == States.Resumed)
            {
                DrawHologram();

                DX.SetFontSize(20);

                MoveCamera();

                DrawBullet(bulletHit);
                if (drawDokuro)
                {
                    DX.DrawExtendGraph(dokuroX - dokuroWidth2, dokuroY - dokuroWidth2, dokuroX + dokuroWidth2, dokuroY + dokuroWidth2, dokuroImage, DX.TRUE);
                }

                DebugWindow.Draw();

                if (mouseInScreen)
                {
                    if (Mouse.Middle)
                    {
                        CircleObject obj = new CircleObject();
                        obj.Init(GameSettings);
                        obj.Radius = 10;
                        obj.X = Mouse.X;
                        obj.Y = Mouse.Y;
                        gameObjects.Add(obj);
                    }
                }

                foreach (var obj in gameObjects)
                {
                    obj.Draw();
                }

                if (mouseInScreen)
                {
                    DX.SetMouseDispFlag(DX.FALSE);
                    DX.DrawCircle(Mouse.X, Mouse.Y, 10, DX.GetColor(0, 255, 0), DX.FALSE);
                    DX.DrawCircle(Mouse.X, Mouse.Y, 8, DX.GetColor(0, 255, 0), DX.FALSE);
                }
                else
                {
                    DX.SetMouseDispFlag(DX.TRUE);
                }
            }
            else
            {
                DX.SetMouseDispFlag(DX.TRUE);
            }
        }

        protected override void _Resume()
        {
            base._Resume();

            Console.WriteLine("MainScene resume");
        }

        protected override void _Pause()
        {
            base._Pause();

            Console.WriteLine("MainScene stop");
        }

        private void Error(string msg)
        {
            Console.WriteLine(msg);
        }

        private void DrawSensorColorImage()
        {
            unsafe
            {
                int* p = (int*)DX.GetImageAddressSoftImage(colorSoftImage);

                using (var pixels = Sensor.GetColorPixels())
                {
                    System.Runtime.InteropServices.Marshal.Copy(pixels.Data, 0, new IntPtr(p), pixels.Width * pixels.Height * 4);
                }
            }
            int tempGraph = DX.CreateGraphFromSoftImage(colorSoftImage);
            DX.DrawExtendGraph(0, 0, GameSettings.WindowWidth, GameSettings.WindowHeight, tempGraph, DX.FALSE);
            DX.DeleteGraph(tempGraph);
        }

        private void DrawSensorDepthImage()
        {
            unsafe
            {
                int* p = (int*)DX.GetImageAddressSoftImage(depthSoftImage);

                using (var pixels = Sensor.GetDepthPixels())
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
            DX.DrawExtendGraph(0, 0, GameSettings.WindowWidth, GameSettings.WindowHeight, tempGraph, DX.FALSE);
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

            DX.DrawString(10, 40, string.Format("{0},{1}", Mouse.X, Mouse.Y), DX.GetColor(0, 255, 0));

            DX.DrawLine(0, GameSettings.WindowHeight / 2, GameSettings.WindowWidth, GameSettings.WindowHeight / 2, DX.GetColor(0, 255, 0));
            if (bullet != null)
            {
                DX.DrawCircle((int)(bullet.Horizontal.Position * 100), GameSettings.WindowHeight / 2 - (int)(bullet.Vertical.Position * 100), 3, DX.GetColor(0, 255, 0), DX.TRUE);
            }

            DX.SetDrawBlendMode(mode, param);
        }

        private int GetDepthAt(int x, int y)
        {
            using (var depthPixels = Sensor.GetDepthPixels())
            {
                int x_ = x * depthPixels.Width / GameSettings.WindowWidth;
                int y_ = y * depthPixels.Height / GameSettings.WindowHeight;
                return depthPixels.Data[y_ * depthPixels.Width + x_] >> 3;
            }
        }

        private PolarPoint GetPolarPointAt(int x, int y, int depth)
        {
            double azimuth = Math.Atan2(x, this.Sensor.KinectSensor.DepthStream.NominalFocalLengthInPixels);
            double elevation = Math.Atan2(y, this.Sensor.KinectSensor.DepthStream.NominalFocalLengthInPixels);
            elevation += this.Sensor.KinectSensor.ElevationAngle * Math.PI / 180;

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
            bool mouseInScreen = 0 <= Mouse.X && Mouse.X < GameSettings.WindowWidth && 0 <= Mouse.Y && Mouse.Y < GameSettings.WindowHeight;

            if (mouseInScreen && Mouse.Left)
            {
                int depthAtMouse = GetDepthAt(Mouse.X, Mouse.Y);
                int mouseXCenter = Mouse.X - GameSettings.WindowWidth / 2;
                int mouseYCenter = GameSettings.WindowHeight / 2 - Mouse.Y;
                PolarPoint pp = GetPolarPointAt(mouseXCenter, mouseYCenter, depthAtMouse);

                double diffBulletAzimuth = pp.Azimuth -
                    (bulletTarget != null ? bulletTarget.Azimuth : 0);

                bulletTarget = pp;

                double v0 = bulletSpeed;
                double g = 9.8;
                double[] angles = Util.CalcFiringAngle(bulletTarget.Length / 1000.0, bulletTarget.Elevation, v0, g);
                this.firingAngle = angles[directShot ? 0 : 1];

                double srv1Deg = firingAngle * 180 / Math.PI;
                double srv2Deg = pp.Azimuth * 180 / Math.PI;

                int tmp = (int)Math.Round(1500 + srv1Deg * 7);
                SerialPort.Write(string.Format("srv1:{0}\n", tmp)); // 1500 usec中心
                Console.WriteLine(string.Format("{0}, {1}", srv1Deg, tmp));
                SerialPort.Write(string.Format("srv2:{0}\n", (int)Math.Round(1500 + srv2Deg * 10))); // 1500 usec中心
                // http://homepage3.nifty.com/rio_i/lab/avr/09pwm.html

                waitUntil = gameStatus.CurrentTimeInMillis + (int)(7000 * Math.Abs(diffBulletAzimuth) / Math.PI) + 1000;
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

                double sin = Math.Sin(firingAngle), cos = Math.Cos(firingAngle);

                DebugWindow.Update("12:FANG", (firingAngle * 180 / Math.PI).ToString("F02"));

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

                DX.StopSoundMem(turretMoveSound);
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
                DebugWindow.Update("10:BL_V", bullet.Vertical.Position.ToString("F2"));
                DebugWindow.Update("11:BL_H", bullet.Horizontal.Position.ToString("F2"));

                /*
                Console.WriteLine(string.Format("bullet updated, Vertical a={0:F2} v={1:F2} p={2:F2}",
                    bullet.Vertical.Acceleration, bullet.Vertical.Velocity, bullet.Vertical.Position));
                Console.WriteLine(string.Format("                Horizontal a={0:F2} v={1:F2} p={2:F2}",
                    bullet.Horizontal.Acceleration, bullet.Horizontal.Velocity, bullet.Horizontal.Position));
                 * */


                if (bulletTarget.Length / 1000 * Math.Cos(bulletTarget.Elevation) < bullet.Horizontal.Position
                    && bulletTarget.Length / 1000 * Math.Sin(bulletTarget.Elevation) > bullet.Vertical.Position)
                {
                    bulletHit = true;

                    DX.PlaySoundMem(hitSound, DX.DX_PLAYTYPE_BACK);

                    waitUntil = gameStatus.CurrentTimeInMillis + 1000;

                    return true;
                }
                {
                    bulletHit = false;
                    //DrawBullet(false);
                }

                return false;
            }
            return true;
        }

        private void DrawBullet(bool hit)
        {
            if (bullet != null && bullet.Horizontal.Position > 0)
            {
                float focalLength = Sensor.KinectSensor.DepthStream.NominalFocalLengthInPixels;

                //double projYPos = bullet.Vertical.Position / bullet.Horizontal.Position * focalLength;
                //double projXPos = Math.Tan(bulletAzimuth) * sensor.KinectSensor.DepthStream.NominalFocalLengthInPixels;

                double rHorizon = bullet.Horizontal.Position;
                double projYPos = focalLength * Math.Tan(Math.Atan2(bullet.Vertical.Position, bullet.Horizontal.Position) - Sensor.KinectSensor.ElevationAngle / 180.0 * Math.PI);
                double projXPos = rHorizon * Math.Sin(bulletAzimuth) * focalLength / (rHorizon * Math.Cos(bulletAzimuth) + 0.05);

                DebugWindow.Update("13:PROJX", projXPos.ToString("F2"));

                double r = Math.Sqrt(Math.Pow(bullet.Vertical.Position, 2) + Math.Pow(bullet.Horizontal.Position, 2));

                int drawYPos = (int)(GameSettings.WindowHeight / 2 - projYPos);
                int drawXPos = (int)(GameSettings.WindowWidth / 2 + projXPos);

                //Console.WriteLine(string.Format("projXPos={0:F2}, projYPos={1:F2}", projXPos, projYPos));

                double drawScale = focalLength / bullet.Horizontal.Position;
                if (hit)
                {
                    int width2 = (int)(0.1 * drawScale / 2);
                    dokuroX = drawXPos;
                    dokuroY = drawYPos;
                    dokuroWidth2 = width2;

                    bullet = null;
                    bulletHit = false;
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
                drawDokuro = false;
                return true;
            }

            drawDokuro = true;

            return false;
        }

        private void MoveCamera()
        {
            bool up = Keyboard.IsHit(DX.KEY_INPUT_UP);
            bool down = Keyboard.IsHit(DX.KEY_INPUT_DOWN);

            try
            {
                if (up && !down)
                {
                    Sensor.KinectSensor.ElevationAngle += 5;
                }
                else if (!up && down)
                {
                    Sensor.KinectSensor.ElevationAngle -= 5;
                }
                else if (up && down)
                {
                    Sensor.KinectSensor.ElevationAngle = 0;
                }
            }
            catch (InvalidOperationException)
            {
            }
        }
    }
}
