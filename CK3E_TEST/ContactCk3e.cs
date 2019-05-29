/* 这个模块主要是为了实现与CK3E的交互，包括：
*  (1) 连接/断开指定CK3E；
*  (2) 下载控制指令给CK3E；
*  (3) 下载运动控制程序给CK3E；
*  (4) 中断运动控制程序执行。
*/

using ODT.PowerPmacComLib;
using System;
using System.Threading;
using System.IO;

namespace CK3E_TEST
{
    public class ContactCk3e
    {
        private ISyncGpasciiCommunicationInterface _communication;
        private int _deviceNumber;
        private bool _downloading;
        private bool _downloadSuccess;
        private int _totalLines;    //总的程序行数
        private int _nowLine;   //当前下载的程序行序号
        private bool _openPmacSuccess;
        private string[] _program;  //程序
        private deviceProperties PMAC;

        public ContactCk3e(int deviceNumber)
        {
            _deviceNumber = deviceNumber;
            _openPmacSuccess = false;
            _downloadSuccess = true;
            _downloading = false;

            PMAC = new deviceProperties();
            PMAC.PortNumber = 22;
            PMAC.User = "root";
            PMAC.Password = "deltatau";
            PMAC.Protocol = CommunicationGlobals.ConnectionTypes.SSH;
            if (_deviceNumber == 0)
            {
                PMAC.IPAddress = "192.168.0.200";
            }
            else if (_deviceNumber == 1)
            {
                PMAC.IPAddress = "192.168.0.201";
            }
            _communication = Connect.CreateSyncGpascii(PMAC.Protocol, null);
        }

        public ISyncGpasciiCommunicationInterface Communication
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
            _communication.GetResponse("A", out ans);
            _communication.GetResponse("close", out ans);
            _communication.GetResponse("delete rotary", out ans);
        }

        /// <summary>
        /// 关闭与PMAC设备的连接
        /// </summary>
        public bool Close()
        {
            bool success = false;
            if (_communication.GpAsciiConnected && _openPmacSuccess)
            {
                success = _communication.DisconnectGpascii();
            }
            return success;
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
            if (!_communication.GpAsciiConnected)
            {
                //_communication = Connect.CreateSyncGpascii(PMAC.Protocol, null);
                _openPmacSuccess = _communication.ConnectGpAscii(PMAC.IPAddress, PMAC.PortNumber, PMAC.User, PMAC.Password);
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
            string ans = null;
            if (_communication.GpAsciiConnected && _openPmacSuccess)
            {
                string temp = null;
                Status communicationStatus = _communication.GetResponse(cmd, out temp);
                ans = communicationStatus == Status.Ok ? temp : "\0";
            }
            return ans;
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
                    _communication.GetResponse(_program[_nowLine++], out ans);
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
                    _communication.GetResponse(_program[_nowLine++], out ans);

                    i++;
                }

                //运动控制程序下载完成
                _downloadSuccess = true;
            }
        }

        /// <summary>
        /// 执行运动控制程序下载
        /// </summary>
        /// <param name = "oNull" ></ param >
        private void TdDowning(object oNull)
        {
            _downloadSuccess = false;
            string ans = null;

            //下载一次运动控制程序
            _downloading = true;
            DownloadOnce(20);
            _communication.GetResponse("close", out ans);
            _downloading = false;
            bool checkrotbuffer = true;
            int index = 1;

            //如果控制程序大于20行，用旋转缓冲区方式下载
            while (!_downloadSuccess)
            {
                _communication.GetResponse("M4000", out ans);

                int tempint = int.Parse(ans.Substring(6, ans.Length - 7));
                if (tempint > 10 * index)
                {
                    checkrotbuffer = true;
                }

                if (tempint > 10 * index && checkrotbuffer)
                {
                    checkrotbuffer = false;
                    index += 2;
                    //剩余可执行程序小于20行时，下载一次运动控制程序
                    _downloading = true;
                    _communication.GetResponse("open rotary", out ans);
                    DownloadOnce(20);
                    _communication.GetResponse("close", out ans);
                    _downloading = false;
                }
                else
                {
                    //_communication.GetResponse("close", out ans);
                }

                //两秒检查一次是否需要补充运动控制程序
                Thread.Sleep(2000);
            }
        }

        /// <summary>
        /// 读取运动程序文件内容
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>每行运动程序按序转存为字符串数组中的一个元素</returns>
        public string[] ReadFile(string path)
        {
            string[] temp = File.ReadAllLines(path);
            return temp;
        }
    }
}