using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using TCPAPI;
using ToolAPI;
using System.Xml.Linq;
using System.Windows.Forms;
using System.Net.Sockets;
/*---------------------------------------------
    Copyright (c) 2017 共友科技
    版权所有：共友科技
    创建人名：赵通
    创建描述：业务类
    创建时间：2017.10.11
    文件功能描述：主要负责TCP监听，本地数据库存储，命令下发
    修改人名：
    修改描述：
    修改标识：
    修改时间：
    ---------------------------------------------*/
namespace Architecture
{
    public class Process 
    {
        TCPOperation TCPOperation;
        private Thread CommandIssuedThread = null, TCPServerControlT = null;
        Subject Subject;
        public Process(Subject SubjectTemp)
        {
            Subject = SubjectTemp;
            //开启监听
            InitTcpSocketServer();
            //数据下发线程
            CommandIssuedThread = new Thread(CommandIssuedHandler) { Priority = ThreadPriority.Highest, IsBackground =true};
            CommandIssuedThread.Start();
            //监听socket的线程，查看是否存在线程假死
            TCPServerControlT = new Thread(TCPServerControl) { IsBackground = true };
            TCPServerControlT.Start();
        }

        #region 命令下发
        public void CommandIssuedHandler()
        {
            while (true)
            {
                try
                {
                    Subject.CommandSending_trigger(TCPOperation.tcpSocket.SocketList);
                }
                catch { }
                Thread.Sleep(3000);//3秒循环一次
            }
        }
        #endregion

        #region socket服务的开启
        /// <summary>
        /// 初始化Socket服务
        /// </summary>
        /// <param name="tcp"></param>
        void InitTcpSocketServer()
        {
            TCPOperation = new TCPOperation(Subject);
            TCPOperation.External = new ExternalClass();
            TCPOperation.OpenListener(int.Parse(MainStatic.Port), 10000);
        }
        public void App_Close()
        {
            try
            {
                TCPOperation.CloseListener();
                if (CommandIssuedThread != null && CommandIssuedThread.IsAlive)
                {
                    CommandIssuedThread.Abort();
                    CommandIssuedThread = null;
                }
                if (TCPServerControlT != null && TCPServerControlT.IsAlive)
                {
                    TCPServerControlT.Abort();
                    TCPServerControlT = null;
                }
            }
            catch(Exception ex)
            { }
        }
        #endregion

        #region socket监听自测试 防止监听假死
        void TCPServerControl()
        {
            while (true)
            {
                try
                {
                    try
                    {
                        System.Net.IPHostEntry oIPHost = System.Net.Dns.GetHostByName(Environment.MachineName);
                        if (oIPHost.AddressList.Length > 0)
                        {
                            string IPAddress = oIPHost.AddressList[0].ToString();
                            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            client.Connect(IPAddress, int.Parse(MainStatic.Port));
                            client.Close();
                            ToolAPI.XMLOperation.WriteLogXmlNoTail("测试TCP监听正常", MainStatic.Port);
                        }
                    }
                    catch (Exception ex)
                    {
                        ToolAPI.XMLOperation.WriteLogXmlNoTail("测试TCP监听异常", ex.Message);
                        try
                        {
                            TCPOperation.CloseListener();
                            TCPOperation = null;
                        }
                        catch (Exception ee)
                        {
                            ToolAPI.XMLOperation.WriteLogXmlNoTail("测试TCP监听服务停止异常", ee.Message);
                        }
                        try
                        {
                            InitTcpSocketServer();
                            ToolAPI.XMLOperation.WriteLogXmlNoTail("测试TCP监听再次被启动", "");
                        }
                        catch (Exception ef)
                        {
                            ToolAPI.XMLOperation.WriteLogXmlNoTail("测试TCP监听再次启动异常", ef.Message);
                        }
                    }
                }
                catch (Exception et)
                {
                    ToolAPI.XMLOperation.WriteLogXmlNoTail("测试TCP监听线程异常", et.Message);
                }
                Thread.Sleep(30000);
            }
        }
        #endregion
    }
}
