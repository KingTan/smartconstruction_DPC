using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TCPAPI;
using ToolAPI;
/*---------------------------------------------
    Copyright (c) 2017 共友科技
    版权所有：共友科技
    创建人名：赵通
    创建描述：TCP客户端Socket接收事件类 实现抽像类AbstractBLL
    创建时间：2017.10.11
    文件功能描述：实现tcp数据接收事件。负责把数据接入进来
    修改人名：
    修改描述：
    修改标识：
    修改时间：
    ---------------------------------------------*/
namespace Architecture
{
    public class TCPOperation : AbstractBLL
    {
        Subject Subject;
        public TCPOperation(Subject sub)
        {
            Subject = sub;
        }

        /// <summary>
        /// 接收数据事件处理方法
        /// </summary>
        public override void tcp_OnSocketResolveRecvEvent(byte[] b, int c, TcpSocketClient client)
        {
            try
            {
                //写入文件做监控
                string Sn = "";
                if (client.External != null)
                    Sn = client.External.External.ToString();
                if (c > 0)
                {
                    ToolAPI.XMLOperation.WriteLogXmlNoTail(Application.StartupPath + "\\OriginalPackage", Sn, ConvertData.ToHexString(b, 0, c));
                    #region 根据协议进行解包
                    //需要外部调用
                    Subject.DataAnalysis_trigger(b, c, client);
                    #endregion
                }
                else
                {
                    ToolAPI.XMLOperation.WriteLogXmlNoTail(Application.StartupPath + "\\OriginalPackage", Sn, "无接收设备数据信息");
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("tcp_OnSocketResolveRecvEvent异常", ex.Message);
            }
        }
        /// <summary>
        /// 绑定SOCKET事件处理方法 在解析里用吧，不需要多次解析了
        /// </summary>
        /// <param name="b"></param>
        /// <param name="client"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override object tcp_InitSocketBindingEvent(byte[] b, TcpSocketClient client, object obj)
        {
            try
            {
                ////需要外部调用
                //return Subject.SocketBinding_trigger(b);
                return "";
            }
            catch
            { return ""; }
        }
    }
}
