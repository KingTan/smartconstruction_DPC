using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtocolAnalysis.MassConcrete
{
    public class Heartbeat
    {
        /// <summary>
        /// 设备id （4位）
        /// </summary>
        public string EquipmentID { set; get; }
        /// <summary>
        /// 时间
        /// </summary>
        public string Time { set; get; }
    }
}
