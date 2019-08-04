using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtocolAnalysis.InfraredContrast
{
    public class Frame_Current
    {
        /// <summary>
        /// 设备地址
        /// </summary>
        public string DeviceNo
        {
            get;
            set;
        }
        /// <summary>
        /// 报警状态
        /// 1个字节，代表8个开关的状态 1代表报警0正常
        /// </summary>
        public string Alarmstatus
        {
            get;
            set;
        }
        /// <summary>
        /// 防拆状态
        /// 1个字节，代表8个开关的状态 1代表被拆除0正常
        /// </summary>
        public string DismantleStatus
        {
            get;
            set;
        }
        /// <summary>
        /// 时间
        /// </summary>
        public string ReceiveTime
        {
            get;
            set;
        }

        public Frame_Current()
        {
            DeviceNo = "";
            Alarmstatus = "00000000";
            DismantleStatus = "00000000";
            ReceiveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
