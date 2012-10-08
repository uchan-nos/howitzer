using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Kinect;

namespace Howitzer
{
    class Sensor
    {
        public class Pixels<T> : IDisposable
        {
            public T[] Data
            {
                private set;
                get;
            }

            private object sync;

            /// <summary>
            /// 横幅（ピクセル）
            /// </summary>
            public int Width
            {
                private set;
                get;
            }

            /// <summary>
            /// 高さ（ピクセル）
            /// </summary>
            public int Height
            {
                private set;
                get;
            }

            public Pixels(T[] data, object sync, int width, int height)
            {
                this.Data = data;
                this.sync = sync;
                this.Width = width;
                this.Height = height;
            }

            public void Dispose()
            {
                Monitor.Exit(sync);
            }
        }

        private KinectSensor sensor;
        private byte[] colorPixels, colorPixelsRead; // colorPixelsReadはユーザーが読み込むための配列
        private short[] depthPixels, depthPixelsRead; // depthPixelsReadはユーザーが読み込むための配列

        private readonly object colorPixelsSync = new object(), depthPixelsSync = new object(); // 同期用オブジェクト

        public KinectSensor KinectSensor
        {
            get
            {
                return sensor;
            }
        }

        /// <summary>
        /// キネクトセンサーの初期化を行う
        /// </summary>
        /// <returns>成功したら0</returns>
        public int Init()
        {
            this.sensor = null;

            // 最初に見つかったキネクトを使う
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (this.sensor == null)
            {
                return -1;
            }

            this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];
            this.depthPixels = new short[this.sensor.DepthStream.FramePixelDataLength];

            this.colorPixelsRead = new byte[this.colorPixels.Length];
            this.depthPixelsRead = new short[this.depthPixels.Length];

            this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            this.sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

            this.sensor.ColorFrameReady += this.OnColorFrameReady;
            this.sensor.DepthFrameReady += this.OnDepthFrameReady;

            this.sensor.Start();

            return 0;
        }

        private void OnColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            /*
             * FrameReady系イベントはUIスレッドで呼ばれる
             * http://akatukisiden.wordpress.com/2012/02/03/kinect-for-windows-sdk-c-frameready-events-running-thread/
             */
            using (var frame = e.OpenColorImageFrame())
            {
                frame.CopyPixelDataTo(colorPixels);

                lock (colorPixelsSync)
                {
                    var temp = colorPixels;
                    colorPixels = colorPixelsRead;
                    colorPixelsRead = temp;
                }
            }
        }

        private void OnDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (var frame = e.OpenDepthImageFrame())
            {
                frame.CopyPixelDataTo(depthPixels);

                lock (depthPixelsSync)
                {
                    var temp = depthPixels;
                    depthPixels = depthPixelsRead;
                    depthPixelsRead = temp;
                }
            }
        }

        /// <summary>
        /// センサーにより最近撮影されたRGB画像のピクセルデータを返す
        /// </summary>
        /// <remarks>返されるデータはモニタによるロックがかけられているため、使用後はできるだけ早くDisposeメソッドを呼び出してください</remarks>
        /// <returns>RGB画像のピクセルデータ</returns>
        public Pixels<byte> GetColorPixels()
        {
            Monitor.Enter(colorPixelsSync);
            return new Pixels<byte>(colorPixelsRead, colorPixelsSync, sensor.ColorStream.FrameWidth, sensor.ColorStream.FrameHeight);
        }

        /// <summary>
        /// センサーにより最近撮影された深度画像のピクセルデータを返す
        /// </summary>
        /// <remarks>返されるデータはモニタによるロックがかけられているため、使用後はできるだけ早くDisposeメソッドを呼び出してください</remarks>
        /// <returns>深度画像のピクセルデータ</returns>
        public Pixels<short> GetDepthPixels()
        {
            Monitor.Enter(depthPixelsSync);
            return new Pixels<short>(depthPixelsRead, depthPixelsSync, sensor.DepthStream.FrameWidth, sensor.DepthStream.FrameHeight);
        }
    }
}
