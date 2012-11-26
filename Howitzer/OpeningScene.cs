using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DxLibDLL;
using System.IO;

namespace Howitzer
{
    class OpeningScene : Scene
    {

        public ServoController SerialPort
        {
            get;
            set;
        }

        private class SlideshowInfo
        {
            public int Time;
            public string FileName;
            public string Description;
            public ImageFileObject Image;

            public SlideshowInfo(int time, string filename, string description)
            {
                Time = time;
                FileName = filename;
                Description = description;
                Image = new ImageFileObject(DX.LoadGraph(
                    Path.Combine(Configuration.GetGlobal().DataDirectory.FullName, FileName)));
            }
        }

        private const int
            TEAMLOGO_TIME = 500,
            LOGO_EN_TIME = 3000,
            LOGO_JP_TIME = 3800,
            SLIDESHOW_TIME = 15000,
            FINALE_TIME = SLIDESHOW_TIME + 47000;

        private SlideshowInfo[] SLIDESHOW_INFO =
        {
            new SlideshowInfo(0, "type38_150mm_howitzer.jpg", "三八式十五糎榴弾砲"),
            new SlideshowInfo(7300, "type96_150mm_howitzer.jpg", "九六式十五糎榴弾砲"),
            new SlideshowInfo(15300, "type74_105mm_howitzer_tank.jpg", "74式自走105mmりゅう弾砲"),
            new SlideshowInfo(23300, "type99_155mm_howitzer_tank.jpg", "99式自走155mmりゅう弾砲"),
            new SlideshowInfo(31300, "ac130.jpg", ""),
            new SlideshowInfo(39300, "type99_2.jpg", ""),
            new SlideshowInfo(1000 * 1000, "", "") // 番兵
        };
        private int slideshowPhase = 0;

        private int phase = 0;

        private int startTime = -1;

        private Motion m = new BezierMotion(BezierMotion.Parameter.RapidLv5, BezierMotion.Parameter.SlowLv1);

        private ImageFileObject logo_jp, logo_en, teamlogo;
        private int teamlogoAlpha;
        private int teamlogoSE;
        private int wetop;

        protected override void _Init(GameSettings settings)
        {
            base._Init(settings);

            {
                var o = new ImageFileObject(DX.LoadGraph(
                    Path.Combine(Configuration.GetGlobal().DataDirectory.FullName, "logo_jp.png")));
                o.Init(settings);
                o.Transparent = true;
                logo_jp = o;
            }
            {
                var o = new ImageFileObject(DX.LoadGraph(
                    Path.Combine(Configuration.GetGlobal().DataDirectory.FullName, "logo_en.png")));
                o.Init(settings);
                o.Transparent = true;
                logo_en = o;
            }
            {
                var o = new ImageFileObject(DX.LoadGraph(
                    Path.Combine(Configuration.GetGlobal().DataDirectory.FullName, "teamlogo.png")));
                o.Init(settings);
                o.Transparent = true;
                teamlogo = o;
            }

            teamlogoSE = DX.LoadSoundMem(
                Path.Combine(Configuration.GetGlobal().DataDirectory.FullName, "teamlogo_se.wav"));
            wetop = DX.LoadSoundMem(
                Path.Combine(Configuration.GetGlobal().DataDirectory.FullName, "wetop.mp3"));
        }


        protected override void _Update(GameStatus status)
        {
            base._Update(status);

            if (startTime == -1)
            {
                startTime = status.CurrentTimeInMillis;
            }
            int durationFromStart = status.CurrentTimeInMillis - startTime;

            if (durationFromStart >= 1000 && GameSettings.GameLogic.Keyborad.IsHit(DX.KEY_INPUT_RETURN))
            {
                MainScene s = new MainScene();
                s.Init(GameSettings);
                s.SerialPort = SerialPort;
                GotoScene(s);

                DX.StopSoundMem(wetop);
            }

            float xf;
            switch (phase)
            {
                case 0:
                    if (durationFromStart >= TEAMLOGO_TIME) {
                        phase++;
                        DX.PlaySoundMem(teamlogoSE, DX.DX_PLAYTYPE_BACK);
                    }
                    break;
                case 1:
                    if (durationFromStart >= TEAMLOGO_TIME + 1300) { phase++; }
                    break;
                case 2:
                    if (durationFromStart >= TEAMLOGO_TIME + 2000) { phase++; }
                    break;
                case 3:
                    if (durationFromStart >= LOGO_EN_TIME) {
                        phase++;
                        DX.PlaySoundMem(wetop, DX.DX_PLAYTYPE_BACK);
                    }
                    break;
                case 4:
                    if (durationFromStart >= LOGO_JP_TIME) { phase++; }
                    break;
                case 5:
                    if (durationFromStart >= SLIDESHOW_TIME) { phase++; }
                    break;
                case 6:
                    if (durationFromStart >= FINALE_TIME) { phase++; }
                    break;
            }

            switch (phase)
            {
                case 1:
                    xf = m.Calculate((durationFromStart - TEAMLOGO_TIME) / 700.0f).X;
                    teamlogoAlpha = (int)(xf * 255);
                    teamlogo.X = GameSettings.WindowWidth / 2 - teamlogo.Width / 2;
                    teamlogo.Y = GameSettings.WindowHeight / 2 - teamlogo.Height / 2;
                    break;
                case 2:
                    xf = m.Calculate((durationFromStart - TEAMLOGO_TIME - 1300) / 700.0f).X;
                    teamlogoAlpha = (int)((1 - xf) * 255);
                    teamlogo.X = GameSettings.WindowWidth / 2 - teamlogo.Width / 2;
                    teamlogo.Y = GameSettings.WindowHeight / 2 - teamlogo.Height / 2;
                    break;
                case 4:
                case 5:
                    xf = m.Calculate((durationFromStart - LOGO_EN_TIME) / 1000.0f).X;
                    logo_en.X = (int)(xf * ((GameSettings.WindowWidth / 2 - logo_en.Width / 2) + logo_en.Width)) - logo_en.Width;
                    logo_en.Y = GameSettings.WindowHeight / 2 - logo_en.Height;
                    xf = m.Calculate((durationFromStart - LOGO_JP_TIME) / 1000.0f).X;
                    logo_jp.X = (int)(xf * ((GameSettings.WindowWidth / 2 - logo_jp.Width / 2) + logo_jp.Width)) - logo_jp.Width;
                    logo_jp.Y = GameSettings.WindowHeight / 2;
                    break;
                case 6:
                    {
                        int timeForSlideshow = durationFromStart - SLIDESHOW_TIME;
                        if (timeForSlideshow >= SLIDESHOW_INFO[slideshowPhase + 1].Time)
                        {
                            slideshowPhase++;
                        }
                        ImageFileObject img = SLIDESHOW_INFO[slideshowPhase].Image;
                        img.Init(GameSettings);
                        img.X = (GameSettings.WindowWidth - img.Width) / 2;
                        img.Y = (GameSettings.WindowHeight - img.Height) / 2;
                    }
                    break;
                case 7:
                    teamlogo.Draw();
                    break;
            }
        }

        protected override void _Draw()
        {
            base._Draw();

            Console.WriteLine(phase);
            switch (phase)
            {
                case 1:
                case 2:
                    DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, teamlogoAlpha);
                    teamlogo.Draw();
                    break;
                case 4:
                    DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, 255);
                    logo_en.Draw();
                    break;
                case 5:
                    logo_en.Draw();
                    logo_jp.Draw();
                    if (logo_jp.X > 0)
                    {
                        Console.WriteLine("debug");
                    }
                    break;
                case 6:
                    DX.DrawExtendGraph(0, 0, GameSettings.WindowWidth, GameSettings.WindowHeight, SLIDESHOW_INFO[slideshowPhase].Image.ImageHandle, DX.TRUE);
                    break;
            }
        }

        protected override void _Dispose()
        {
            base._Dispose();

            DX.DeleteGraph(logo_jp.ImageHandle);
            DX.DeleteGraph(logo_en.ImageHandle);
            DX.DeleteGraph(teamlogo.ImageHandle);
            DX.DeleteSoundMem(teamlogoSE);

            for (int i = 0; i < SLIDESHOW_INFO.Length; ++i)
            {
                if (SLIDESHOW_INFO[i].Image != null)
                {
                    DX.DeleteGraph(SLIDESHOW_INFO[i].Image.ImageHandle);
                }
            }
        }
    }
}
