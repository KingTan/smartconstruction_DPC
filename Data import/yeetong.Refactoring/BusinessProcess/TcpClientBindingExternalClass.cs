using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPAPI;
/*---------------------------------------------
    Copyright (c) 2017 共友科技
    版权所有：共友科技
    创建人名：赵通
    创建描述：TCP客户端Socket数据绑定类 实现IExternalInterface接口
    创建时间：2017.10.11
    文件功能描述：负责把和socket一一对应的属性绑定，比如：设备编号。或者做一些标志记录
    修改人名：
    修改描述：
    修改标识：
    修改时间：
    ---------------------------------------------*/
namespace Architecture
{
    public class ExternalClass : IExternalInterface
    {
        #region 实现接口
        public Object External { set; get; }
        public event Func<byte[], TcpSocketClient, Object, Object> InitEvent;  
        //为了给你的外部绑定类进行初始化
        public void ExternalInit(byte[] obj, TcpSocketClient tcpClientTemp)
        {
            TcpClientBindingExternalClass TcpExtendTemp = External as TcpClientBindingExternalClass;
            //需要根据版本来确定，所以还是调用具体版本协议吧。在架构中不做处理
            string result = InitEvent(obj, tcpClientTemp, External).ToString();
            if (result != "")
            {
                try
                {
                    TcpExtendTemp.TVersion = result.Split('&')[0];
                    TcpExtendTemp.EquipmentID = result.Split('&')[1];
                }
                catch (Exception)
                {
                    TcpExtendTemp.TVersion = "";
                    TcpExtendTemp.EquipmentID = "";
                }

            }
        }
        #endregion

        public ExternalClass()
        {
            External = new TcpClientBindingExternalClass();
            InitEvent += (byte[] obj, TcpSocketClient client, Object ob) => { return ""; };
        }


    }
    public class TcpClientBindingExternalClass
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public string TVersion { get; set; }
        /// <summary>
        /// 设备编号
        /// </summary>
        public string EquipmentID { get; set; }
        /// <summary>
        /// 新老设备标识
        /// </summary>
        public int EquipmentTag { get; set; }
        public string uuid { get; set; }
        public TcpClientBindingExternalClass()
        {
            TVersion = "";//版本号初始化
            EquipmentID = "";//设备编号初始化
            EquipmentTag = 0;//新老设备标识初始化  1：老设备 2 新设备 1，2可以针对任何新老设备   3是定闹钟的雾泡设备 
            uuid = "";//雾炮喷淋用到
        }
    }
}
