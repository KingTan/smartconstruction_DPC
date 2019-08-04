using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtocolAnalysis.GasDetection
{//IN `craneNo` varchar(20),IN `RangeV` int,IN `LowAlarmV` float,IN `A1AlarmV` float,IN `A2AlarmV` float,IN `GasStrengthADV` int,IN `gasstrengthV` float,IN `AlarmStatusV` int,IN `AlarmStatusFlagV` varchar(1),IN `onlineTimes` datetime
    public class Frame_Current
    {
        /// <summary>
        /// 设备地址 dtu
        /// </summary>
        public string DeviceNo
        {
            get;
            set;
        }
        /// <summary>
        /// 主机地址
        /// </summary>
        public byte Addr
        {
            get;
            set;
        }
        /// <summary>
        /// 量程
        /// </summary>
        public UInt16 Range { get; set; }
        /// <summary>
        /// 低报警点
        /// </summary>
        public float LowAlarm
        {
            get;
            set;
        }
        /// <summary>
        /// A1报警点
        /// </summary>
        public float A1Alarm
        {
            get;
            set;
        }
        /// <summary>
        /// A2报警点
        /// </summary>
        public float A2Alarm
        {
            get;
            set;
        }
        /// <summary>
        /// 实时AD值
        /// </summary>
        public UInt32 GasStrengthAD
        {
            get;
            set;
        }
        /// <summary>
        /// 气体值
        /// </summary>
        public float GasStrength
        {
            get;
            set;
        }
        /// <summary>
        /// 报警状态
        /// </summary>
        public UInt16 AlarmStatus
        {
            get;
            set;
        }
        /// <summary>
        /// 报警状态  二级报警2 一级报警是1 未报警0
        /// </summary>
        public byte AlarmStatusFlag { get; set; }
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
            Range = 0;
            LowAlarm = 0;
            A1Alarm = 0;
            A2Alarm = 0;
            GasStrengthAD = 0;
            AlarmStatus = 0;
            AlarmStatusFlag = 0;
            Addr = 6;
            GasStrength = 0f;
            ReceiveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
