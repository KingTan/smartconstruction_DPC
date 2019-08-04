using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtocolAnalysis.IdentityVerification.model
{
   public class Current_IdentityVerification
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
       /// 身份证号
       /// </summary>
       public string ID { get; set; }
       /// <summary>
       /// 界面id
       /// </summary>
       public string InterId { get; set; }
       /// <summary>
       /// NC
       /// </summary>
       public string Nc { get; set; }
       /// <summary>
       /// 继电器状态
       /// </summary>
       public string SensorSet { get; set; }
       /// <summary>
       /// 高度
       /// </summary>
       public string Height { get; set; }
    }
}
