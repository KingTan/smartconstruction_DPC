using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtocolAnalysis.IdentityVerification.model
{
    /// <summary>
    /// 心跳
    /// </summary>
    public class Heartbeat_IdentityVerification
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        public string Equipment { get; set; }
        /// <summary>
        /// RTC
        /// </summary>
        public string RTC { get; set; }
        /// <summary>
        /// 身份证
        /// </summary>
        public string Identity_card { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public string Creat_time { get; set; }

        public Heartbeat_IdentityVerification()
        {
            Equipment = "";
            RTC = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Identity_card = "";
            Creat_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
