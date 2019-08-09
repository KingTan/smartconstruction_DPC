using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtocolAnalysis.Iot_v1.model
{
    /// <summary>
    /// 人员考勤数据接受类
    /// </summary>
    public class Personnel_send_frame
    {
        /// <summary>
        /// 项目编码 对外
        /// </summary>
        public string project_code { get; set; }
        /// <summary>
        /// 设备序列码
        /// </summary>
        public string sn { get; set; }
        /// <summary>
        /// 门号
        /// </summary>
        public string gate_no { get; set; }
        /// <summary>
        /// 检索时间 一般为采集时间
        /// </summary>
        public long @timestamp { get; set; }
        /// <summary>
        /// 通道号
        /// </summary>
        public string channel_no { get; set; }
        /// <summary>
        /// 识别方式
        /// </summary>
        public string cert_mode { get; set; }
        /// <summary>
        /// 进出类型
        /// </summary>
        public string in_or_out { get; set; }
        /// <summary>
        /// 人员code
        /// </summary>
        public string personal_id_code { get; set; }
        /// <summary>
        /// 特征
        /// </summary>
        public string features_code { get; set; }
    }
}
