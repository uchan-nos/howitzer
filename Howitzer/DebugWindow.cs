using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DxLibDLL;

namespace Howitzer
{
    class DebugWindow
    {
        private Dictionary<string, string> table = new Dictionary<string, string>();

        public int Width
        {
            set;
            get;
        }

        public int Height
        {
            set;
            get;
        }

        public int Top
        {
            set;
            get;
        }

        public int Left
        {
            set;
            get;
        }

        public DebugWindow()
        {
        }

        public void Update(string key, string value)
        {
            table[key] = value;
        }

        public void Draw()
        {
            //DX.COLORDATA colorData;
            //DX.CreateARGB8ColorData(out colorData);
            int mode, param;
            DX.GetDrawBlendMode(out mode, out param);
            DX.SetDrawBlendMode(DX.DX_BLENDMODE_ALPHA, 180);
            //DX.DrawBox(Left, Top, Left + Width, Top + Height, DX.GetColor3(out colorData, 128, 128, 128, 128), DX.TRUE);
            DX.DrawBox(Left, Top, Left + Width, Top + Height, DX.GetColor(0, 0, 0), DX.TRUE);
            DX.SetDrawBlendMode(mode, param);

            if (table.Count > 0)
            {
                int lineHeight = (Height - 10) / table.Count;
                if (lineHeight > 15)
                {
                    lineHeight = 15;
                }

                DX.SetFontSize(lineHeight);

                int widthMax = 0;
                foreach (var item in table)
                {
                    int width = DX.GetDrawStringWidth(item.Key, item.Key.Length);
                    if (width >= 0)
                    {
                        widthMax = Math.Max(widthMax, width);
                    }
                }

                int i = 0;
                foreach (var item in table)
                {
                    DX.DrawString(Left + 5, Top + 5 + i * lineHeight, item.Key, 0xffffff);
                    DX.DrawString(Left + 5 + widthMax + 5, Top + 5 + i * lineHeight, item.Value, 0xffffff);
                    ++i;
                }
            }
        }
    }
}
