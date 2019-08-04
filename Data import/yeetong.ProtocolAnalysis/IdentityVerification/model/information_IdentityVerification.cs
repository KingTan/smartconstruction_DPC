using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtocolAnalysis.IdentityVerification.model
{
    public class information_IdentityVerification
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
        /// 软件版本号
        /// </summary>
        public string SoftVersion { get; set; }
        /// <summary>
        /// 身份认证方式
        /// </summary>
        public string IdentifyWay { get; set; }
        /// <summary>
        /// 身份识别周期
        /// </summary>
        public string IdentifyClcle { get; set; }
        /// <summary>
        /// 高度最低采集点
        /// </summary>
        public string HeightMin { get; set; }
        /// <summary>
        /// 高度最高采集点
        /// </summary>
        public string HeightMax { get; set; }
        /// <summary>
        /// 高度两处采集点距离
        /// </summary>
        public string HeightDistance { get; set; }
        /// <summary>
        /// 数据类型
        /// </summary>
        public string dataType { get; set; }
    }
}
