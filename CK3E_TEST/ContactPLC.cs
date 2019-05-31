/* Module Name: 与PLC交互功能模块
 *
 * Description: 这个模块主要功能有两个：
 *              (1) 接收并解析PLC发来的状态数据；
 *              (2) 发送操作指令给PLC执行。
 */

using System;
using System.IO.Ports;
using System.Threading;

namespace CK3E_TEST
{
    internal class ContactPLC
    {
        private uint _cmd;  //指令
        private uint[] _cmdModel;

        private SerialPort _comm;
        private bool _isCommOK;
        private byte[] _status;
        private int _watchDog;  //串口通信状态看门狗
        private Thread _watchThread; //通信状态检测线程

        /// <summary>
        /// 与PLC交互模块
        /// </summary>
        /// <param name="port">端口号</param>
        public ContactPLC(string port)
        {
            _comm = new SerialPort(port, 9600, Parity.Even, 8, StopBits.One);
            _comm.DataReceived += new SerialDataReceivedEventHandler(_teachcom_DataReceived);
            _watchThread = new Thread(new ThreadStart(WatchThread));
            _watchThread.IsBackground = true;
            _isCommOK = false;
            _watchDog = 0;

            _status = new byte[2];
            _cmdModel = new uint[8] { 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80 };
        }

        /// <summary>
        /// 与PLC通讯状态
        /// </summary>
        public bool IsCommOk
        {
            get { return _isCommOK; }
        }

        /// <summary>
        /// PLC发来的状态
        /// </summary>
        public byte[] Status
        {
            get { return _status; }
        }
        /// <summary>
        /// 断开与PLC连接
        /// </summary>
        public void Close()
        {
            try
            {
                _isCommOK = false;
                _watchThread.Abort();
                _comm.Close();
            }
            catch (Exception)
            { }
        }

        /// <summary>
        /// 连接PLC
        /// </summary>
        public void Open()
        {
            try
            {
                _comm.Open();
                _watchThread.Start();
            }
            catch (Exception)
            { }
        }
        /// <summary>
        /// IPC给PLC发送操作指令
        /// </summary>
        /// <param name="number">指令编号</param>
        /// <param name="on">ON/OFF</param>
        public void Operate(int number, bool on)
        {
            if (on)
            {
                _cmd |= _cmdModel[number - 1];
            }
            else
            {
                _cmd &= ~_cmdModel[number - 1];
            }
        }

        /// <summary>
        /// 串口接收数据事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _teachcom_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                //等待数据接收完整
                Thread.Sleep(10);

                //转码数据
                int n = _comm.BytesToRead;
                byte[] readbuffer = new byte[n];
                _comm.Read(readbuffer, 0, readbuffer.Length);

                //分析数据
                AnalysisData(readbuffer);

                //发送控制指令给PLC
                byte[] sendCmd = new byte[1];
                sendCmd[0] = Convert.ToByte(_cmd);
                _comm.Write(sendCmd, 0, 1);

                //更新通讯状态
                _watchDog = 0;
                _isCommOK = true;
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 分析数据
        /// </summary>
        /// <param name="data">PLC发来的数据</param>
        private void AnalysisData(byte[] data)
        {
            _status = data;
        }

        /// <summary>
        /// 看门狗线程
        /// </summary>
        private void WatchThread()
        {
            while (true)
            {
                //连续10次接收不到数据，则通讯异常
                _watchDog++;
                if (_watchDog >= 10)
                {
                    _isCommOK = false;
                    _watchDog = 10;
                }

                Thread.Sleep(100);
            }
        }
    }
}