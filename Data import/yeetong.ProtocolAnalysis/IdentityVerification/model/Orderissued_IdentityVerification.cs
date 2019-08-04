using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtocolAnalysis.IdentityVerification.model
{
    /// <summary>
    /// 命令下发
    /// </summary>
    public class Orderissued_IdentityVerification
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        public string Equipment { get; set; }
        /// <summary>
        /// 确认标识
        /// </summary>
        public string Confirm_flag { get; set; }
        /// <summary>
        /// 命令字
        /// </summary>
        public string Order_flag { get; set; }
        /// <summary>
        /// ip/dns
        /// </summary>
        public string IP_DNS { get; set; }
        /// <summary>
        /// port 端口
        /// </summary>
        public string Port { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public string Creat_time { get; set; }

        public Orderissued_IdentityVerification()
        {
            Equipment = "";
            Confirm_flag = "0";
            Order_flag = "0";
            Creat_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            IP_DNS = "";
            Port = "";
        }
    }
}
