/* 这个模块主要是为了实现与CK3E的交互，包括：
*  (1) 连接/断开指定CK3E；
*  (2) 下载控制指令给CK3E；
*  (3) 下载运动控制程序给CK3E；
*  (4) 中断运动控制程序执行。
*/

using System;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;

namespace CK3E_TEST
{
    public class ContactCk3e
    {
        public LogRecord logRecord;
        private string Host;
        private string User;
        private string Pwd;
        private string Port;

        private bool _communication;
        private bool _downloading;
        private bool _downloadSuccess;
        private int _totalLines;    //总的程序行数
        private int _nowLine;   //当前下载的程序行序号
        private bool _openPmacSuccess;
        private string[] _program;  //程序

        [DllImport("libPowerPMACControl.dll", EntryPoint = "?PowerPMACcontrol_connect@PowerPMACcontrol@PowerPMACcontrol_ns@@QAEHPBD000_N@Z")]
        public static extern int PowerPMACcontrol_connect([MarshalAs(UnmanagedType.LPStr)]string host,
            [MarshalAs(UnmanagedType.LPStr)]string user, [MarshalAs(UnmanagedType.LPStr)]string pwd,
            [MarshalAs(UnmanagedType.LPStr)]string port = "22", bool nominus2 = false);

        [DllImport("libPowerPMACControl.dll", EntryPoint = "?PowerPMACcontrol_isConnected@PowerPMACcontrol@PowerPMACcontrol_ns@@QAE_NH@Z")]
        public static extern bool PowerPMACcontrol_isConnected(int timeout);

        [DllImport("libPowerPMACControl.dll", EntryPoint = "?PowerPMACcontrol_disconnect@PowerPMACcontrol@PowerPMACcontrol_ns@@QAEHXZ")]
        public static extern int PowerPMACcontrol_disconnect();

        [DllImport("libPowerPMACControl.dll",
            EntryPoint = "?PowerPMACcontrol_sendCommand@PowerPMACcontrol@PowerPMACcontrol_ns@@QAEHV?$basic_string@DU?$char_traits@D@std@@V?$allocator@D@2@@std@@AAV34@@Z")]
        public static extern int PowerPMACcontrol_sendCommand(string command, out string reply);

        public ContactCk3e()
        {
            _openPmacSuccess = false;
            _downloadSuccess = true;
            _downloading = false;

            Host = "192.168.0.200";
            User = "root";
            Pwd = "deltatau";
            Port = "22";

            _communication = PowerPMACcontrol_isConnected(5000);
        }

        public delegate void LogRecord(string message, string type);

        public bool Communication
        {
            get { return _communication; }
            set { _communication = value; }
        }

        // 正在下载运动控制程序
        public bool Downloading
        {
            get { return _downloading; }
        }

        // 控制程序下载完成
        public bool DownloadSuccess
        {
            get { return _downloadSuccess; }
            set { _downloadSuccess = value; }
        }

        // 打开成功
        public bool OpenSuccess
        {
            get { return _openPmacSuccess; }
        }

        /// <summary>
        /// 中断运动控制程序下载
        /// </summary>
        public void AbortDownload()
        {
            _downloadSuccess = true;
            _downloading = false;

            string ans = null;
            PowerPMACcontrol_sendCommand("A", out ans);
            PowerPMACcontrol_sendCommand("close", out ans);
            PowerPMACcontrol_sendCommand("delete rotary", out ans);
        }

        /// <summary>
        /// 关闭与PMAC设备的连接
        /// </summary>
        public bool Close()
        {
            int _close = 1;
            if (_communication && _openPmacSuccess)
            {
                _close = PowerPMACcontrol_disconnect();
            }

            if (_close == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 下载运动控制程序
        /// </summary>
        /// <param name="program">运动控制程序</param>
        /// <param name="errorMsg">错误消息</param>
        /// <returns>下载结果</returns>
        public bool Download(string[] program, out string errorMsg)
        {
            try
            {
                if (!_downloadSuccess)
                {
                    errorMsg = "有未执行完成的程序！";
                    return false;
                }
                else
                {
                    _program = program;
                    _nowLine = 0;
                    _totalLines = 0;
                    foreach (string s in _program)
                    {
                        _totalLines++;
                    }

                    //从线程池启动一个线程，下载运动控制程序
                    ThreadPool.QueueUserWorkItem(TdDowning);
                    errorMsg = null;
                    return true;
                }
            }
            catch (Exception)
            {
                errorMsg = "程序下载出错！";
                return false;
            }
        }

        /// <summary>
        /// 连接PMAC设备
        /// </summary>
        /// <returns></returns>
        public bool Open()
        {
            int _open = 0;
            if (!_communication)
            {
                _open = PowerPMACcontrol_connect(Host, User, Pwd, Port, true);
            }

            if (_open != 0)
            {
                _openPmacSuccess = true;
            }
            return _openPmacSuccess;
        }

        /// <summary>
        /// 发送一条UMAC控制指令
        /// </summary>
        /// <param name="cmd">PMAC控制指令</param>
        /// <returns>指令的执行结果</returns>
        public string Send(string cmd)
        {
            int ans = 1;
            string temp = null;
            if (_communication && _openPmacSuccess)
            {
                ans = PowerPMACcontrol_sendCommand(cmd, out temp);
            }

            if (ans == 0)
            {
                return temp;
            }
            else
            {
                return "\0";
            }
        }

        /// <summary>
        /// 执行一次运动控制程序下载
        /// </summary>
        /// <param name="maxLines">指定一次下载的行数</param>
        private void DownloadOnce(int maxLines)
        {
            string ans = null;

            if (_totalLines > maxLines)
            {
                //程序总行数大于maxLines行时候，先下载maxLines行
                for (int i = 0; i < maxLines; i++)
                {
                    string pro = _program[_nowLine];
                    _totalLines--;
                    PowerPMACcontrol_sendCommand(_program[_nowLine++], out ans);

                    string msg = i.ToString();
                    logRecord("ROW_" + msg + pro + ans, "DOWNLOAD PROGRAM");
                }
            }
            else
            {
                //程序总行数不足maxLines行时候，则一次性全部下载
                int i = 0;
                while (_totalLines > 0)
                {
                    string pro = _program[_nowLine];
                    _totalLines--;
                    PowerPMACcontrol_sendCommand(_program[_nowLine++], out ans);

                    string msg = i.ToString();
                    logRecord("ROW_" + msg + pro + ans, "DOWNLOAD PROGRAM");
                    i++;
                }

                //运动控制程序下载完成
                _downloadSuccess = true;
            }
        }

        /// <summary>
        /// 执行运动控制程序下载
        /// </summary>
        /// <param name="oNull"></param>
        private void TdDowning(object oNull)
        {
            _downloadSuccess = false;
            string ans = null;

            //下载一次运动控制程序
            _downloading = true;
            DownloadOnce(500);
            PowerPMACcontrol_sendCommand("close", out ans);
            _downloading = false;
            bool checkrotbuffer = true;
            int index = 1;

            //如果控制程序大于500行，用旋转缓冲区方式下载
            while (!_downloadSuccess)
            {
                //_communication.GetResponse("M4000", out ans);
                //int tempint = int.Parse(ans);
                int tempint = _program.Length;
                if (tempint > 250 * index)
                {
                    checkrotbuffer = true;
                }
                if (tempint > 250 * index && checkrotbuffer)
                {
                    checkrotbuffer = false;
                    index += 2;
                    //剩余可执行程序小于200行时，下载一次运动控制程序
                    _downloading = true;
                    //_communication.GetResponse("open rotary", out ans);
                    DownloadOnce(500);
                    PowerPMACcontrol_sendCommand("close", out ans);
                    _downloading = false;
                }
                if (tempint < 250 * index)
                {
                    DownloadOnce(tempint);
                    _downloadSuccess = true;
                    _downloading = false;
                }

                //两秒检查一次是否需要补充运动控制程序
                Thread.Sleep(2000);
            }
        }
    }
}