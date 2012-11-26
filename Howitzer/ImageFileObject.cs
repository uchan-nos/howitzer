using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DxLibDLL;

namespace Howitzer
{
    class ImageFileObject : GameObject
    {
        /// <summary>
        /// オブジェクトの横位置
        /// </summary>
        public int X
        {
            get;
            set;
        }

        /// <summary>
        /// オブジェクトの縦位置
        /// </summary>
        public int Y
        {
            get;
            set;
        }

        public bool Transparent
        {
            get;
            set;
        }

        private int width;
        public int Width
        {
            get { return width; }
        }

        private int height;
        public int Height
        {
            get { return height; }
        }

        public int ImageHandle
        {
            get;
            private set;
        }

        public ImageFileObject(int imageHandle)
        {
            ImageHandle = imageHandle;
            DX.GetGraphSize(ImageHandle, out width, out height);
        }

        protected override void _Draw()
        {
            base._Draw();
            DX.DrawGraph(X, Y, ImageHandle, Transparent ? DX.TRUE : DX.FALSE);
        }
    }
}
