using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TCPAPI;
/*---------------------------------------------
    Copyright (c) 2017 共友科技
    版权所有：共友科技
    创建人名：赵通
    创建描述：主题类
    创建时间：2017.10.11
    文件功能描述：主题类
    修改人名：
    修改描述：
    修改标识：
    修改时间：
    ---------------------------------------------*/
namespace Architecture
{
    public class Subject
    {
        ///// <summary>
        ///// 数据存储事件
        ///// </summary>
        //public event Action<object> DataStorageRelay = (object obj) => { };

        /// <summary>
        /// 数据解析事件
        /// </summary>
        public event Action<byte[], int, TcpSocketClient> DataAnalysis = (byte[] byteAry, int count, TcpSocketClient socketClient) => { };
        /// <summary>
        /// 命令下发事件
        /// </summary>
        public event Action<IList<TcpSocketClient>> CommandSending = (IList<TcpSocketClient> list) => { };
        /// <summary>
        /// 数据存储转发的触发
        /// </summary>
        /// <param name="obj"></param>
        //public void DataStorageRelay_trigger(object obj)
        //{
        //    DataStorageRelay(obj);
        //}

        /// <summary>
        /// 数据解析事件触发
        /// </summary>
        /// <param name="byteAry"></param>
        public void DataAnalysis_trigger(byte[] byteAry, int count, TcpSocketClient socketClient)
        {
            DataAnalysis(byteAry, count, socketClient);
        }

        /// <summary>
        /// 命令下发的触发
        /// </summary>
        /// <param name="list"></param>
        public void CommandSending_trigger(IList<TcpSocketClient> list)
        {
            CommandSending(list);
        }
    }
}
