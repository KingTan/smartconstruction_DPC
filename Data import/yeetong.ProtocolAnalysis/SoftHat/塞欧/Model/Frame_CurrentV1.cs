using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtocolAnalysis.SoftHat
{
    public class Frame_CurrentV1
    {
        private string deviceType = "";
        /// <summary>
        /// 设备类型
        /// </summary>
        public string DeviceType
        {
            get { return deviceType; }
            set { deviceType = value; }
        }

        private string deviceNum = "00000000";
        /// <summary>
        /// 设备编号
        /// </summary>
        public string DeviceNum
        {
            get { return deviceNum; }
            set { deviceNum = value; }
        }

        private string gprsSignal = "";
        /// <summary>
        /// GPRS信号强度
        /// </summary>
        public string GprsSignal
        {
            get { return gprsSignal; }
            set { gprsSignal = value; }
        }
    
        private int peopleNum = 0;
        /// <summary>
        /// 人员数量
        /// </summary>
        public int PeopleNum
        {
            get { return peopleNum; }
            set { peopleNum = value; }
        }

        private string peopleLable = "";
        /// <summary>
        /// 人员标签
        /// </summary>
        public string PeopleLable
        {
            get { return peopleLable; }
            set { peopleLable = value; }
        }

        /// <summary>
        /// 时间
        /// </summary>
        private string rtime = "";
        public string Rtime
        {
            get { return rtime; }
            set { rtime = value; }
        }
    }
}
