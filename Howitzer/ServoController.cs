using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Ports;

namespace Howitzer
{
    class ServoController
    {
        private string portName;
        private SerialPort port;

        public ServoController(string portName)
        {
            this.portName = portName;
            this.port = null;
        }

        /// <summary>
        /// 指定された文字列をシリアルポートへ書き込む
        /// </summary>
        /// <param name="command">書き込む文字列</param>
        /// <returns>正常に書き込めたらtrue</returns>
        public bool Write(string command)
        {
            if (this.port == null || !this.port.IsOpen)
            {
                Connect();
            }

            if (this.port != null && this.port.IsOpen)
            {
                try
                {
                    this.port.Write(command);
                    return true;
                }
                catch (InvalidOperationException)
                {
                    // ポートが開いていない
                }
                catch (TimeoutException)
                {
                    // 書き込みがタイムアウトした
                }
            }

            return false;
        }

        public void Close()
        {
            if (this.port != null && !this.port.IsOpen)
            {
                this.port.Close();
            }
        }

        private void Connect()
        {
            if (this.port == null)
            {
                foreach (var name in SerialPort.GetPortNames())
                {
                    if (name == this.portName)
                    {
                        try
                        {
                            this.port = new SerialPort(this.portName, 9600, Parity.None, 8, StopBits.One);
                            this.port.Open();
                        }
                        catch (IOException)
                        {
                            // ポートが見つからない or 開けない
                            // this.portをnullのままにしておく
                        }
                        break;
                    }
                }
            }
            else if (!this.port.IsOpen)
            {
                try
                {
                    this.port.Open();
                }
                catch (IOException)
                {
                    this.port = null;
                }
            }
        }
    }
}
