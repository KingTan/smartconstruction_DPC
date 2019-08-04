using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtocolAnalysis
{
   
    [Serializable]
    public class BorderProtection_Heartbeat
    {
        /// <summary>
        /// 设备号
        /// </summary>
        public string sn { get; set; }
        /// <summary>
        /// RTC
        /// </summary>
        public string Rtc { get; set; }
    }
    
}
