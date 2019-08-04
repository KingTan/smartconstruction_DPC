using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtocolAnalysis.FogGun
{
    public  class Frame_TimingConfig
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
        /// 定时功能的开关
        /// </summary>
        public string TimingSwitch
        {
            get;
            set;
        }
        /// <summary>
        /// 定时启动的时间
        /// </summary>
        public string Time
        {
            get;
            set;
        }
        /// <summary>
        /// 持续时间
        /// </summary>
        public string Timeout
        {
            get;
            set;
        }
        /// <summary>
        /// 循环周属性
        /// ZT增加
        /// 2016/11/15
        /// </summary>
        public string Week
        {
            get;
            set;
        }

        public Frame_TimingConfig()
        {
            DeviceNo = "";
            TimingSwitch = "";
            Time = "";
            Timeout = "";
            Week = "";
        }
    }
}
