using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtocolAnalysis.DisCharge
{
    public class Frame_ParameterConfig
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
        /// 参数修改时间
        /// </summary>
        public DateTime? ParameterUpdateTime
        {
            get;
            set;
        }
        /// <summary>
        /// 安装时间
        /// </summary>
        public DateTime? InstallDate
        {
            get;
            set;
        }
        /// <summary>
        /// 额定载荷(1)
        /// </summary>
        public ushort LoadRating
        {
            get;
            set;
        }
        /// <summary>
        /// 预警系数(1) %
        /// </summary>
        public byte EarlyAlarmCoefficient
        {
            get;
            set;
        }
        /// <summary>
        /// 报警系数(1) %
        /// </summary>
        public byte AlarmCoefficient
        {
            get;
            set;
        }
        /// <summary>
        /// 空载AD1
        /// </summary>
        public short EmptyAD1
        {
            get;
            set;
        }
        /// <summary>
        /// 空载AD2
        /// </summary>
        public short EmptyAD2
        {
            get;
            set;
        }
        /// <summary>
        /// 空载AD3
        /// </summary>
        public short EmptyAD3
        {
            get;
            set;
        }
        /// <summary>
        /// 空载AD4
        /// </summary>
        public short EmptyAD4
        {
            get;
            set;
        }
        /// <summary>
        /// 空载AD
        /// </summary>
        public short EmptyAD
        {
            get;
            set;
        }
        /// <summary>
        /// 标准重物AD1
        /// </summary>
        public short StandardLoadAD1
        {
            get;
            set;
        }
        /// <summary>
        /// 标准重物AD2
        /// </summary>
        public short StandardLoadAD2
        {
            get;
            set;
        }
        /// <summary>
        /// 标准重物AD3
        /// </summary>
        public short StandardLoadAD3
        {
            get;
            set;
        }
        /// <summary>
        /// 标准重物AD4
        /// </summary>
        public short StandardLoadAD4
        {
            get;
            set;
        }
        /// <summary>
        /// 标准重物AD
        /// </summary>
        public short StandardLoadAD
        {
            get;
            set;
        }
        /// <summary>
        /// 标准重物
        /// </summary>
        public short StandardLoad
        {
            get;
            set;
        }
        /// <summary>
        /// 倾角预警值(1) 0.1度
        /// </summary>
        public short AngleEarlyAlarm
        {
            get;
            set;
        }
        /// <summary>
        /// 倾角报警值(1) 0.1度
        /// </summary>
        public short AngleAlarm
        {
            get;
            set;
        }
        /// <summary>
        /// 软件版本号
        /// </summary>
        public string SoftVersion
        {
            get;
            set;
        }

        public Frame_ParameterConfig()
        {
            DeviceNo = "";
            ParameterUpdateTime = null;
            InstallDate = null;
            LoadRating = 0;
            EarlyAlarmCoefficient = 0;
            AlarmCoefficient = 0;
            EmptyAD = 0;
            StandardLoadAD = 0;
            StandardLoad = 0;

            EmptyAD1 = 0;
            EmptyAD2 = 0;
            EmptyAD3 = 0;
            EmptyAD4 = 0;

            StandardLoadAD1 = 0;
            StandardLoadAD2 = 0;
            StandardLoadAD3 = 0;
            StandardLoadAD4 = 0;

            AngleEarlyAlarm = 0;
            AngleAlarm = 0;
            SoftVersion = "";
        }
    }
}
