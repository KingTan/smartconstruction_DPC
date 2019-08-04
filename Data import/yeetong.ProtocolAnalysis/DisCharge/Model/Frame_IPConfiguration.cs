using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtocolAnalysis.DisCharge
{
    public class Frame_IPConfiguration
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        public string DeviceNo
        {
            get;
            set;
        }
        /// <summary>
        /// IP/域名
        /// </summary>
        public string IP
        {
            get;
            set;
        }
        /// <summary>
        /// 端口
        /// </summary>
        public string Port
        {
            get;
            set;
        }
        public Frame_IPConfiguration()
        {
            DeviceNo = "";
            IP = "";
            Port = "";
        }
    }
}
