using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtocolAnalysis.FogGun
{
    public  class Frame_UpdateIpPort
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
        /// IP长度
        /// </summary>
        public byte IPLength
        {
            get;
            set;
        }
        /// <summary>
        /// IP
        /// </summary>
        public string IP
        {
            get;
            set;
        }
        /// <summary>
        /// 端口长度
        /// </summary>
        public byte PortLength
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

        /// <summary>
        /// 状态
        /// </summary>
        public byte State
        {
            get;
            set;
        }
        /// <summary>
        /// 是否下发成功
        /// </summary>
        public bool issuccess
        {
            get;
            set;
        }
        public Frame_UpdateIpPort()
        {
            DeviceNo = "";
            IPLength = 0;
            IP = "";
            PortLength = 0;
            Port = "";
            State = 0;
            issuccess = false;
        }
    }
}
