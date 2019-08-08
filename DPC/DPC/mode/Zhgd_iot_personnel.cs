using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPC
{
    /// <summary>
    /// 实时数据
    /// </summary>
    public class Zhgd_iot_personnel_records
    {
        /// <summary>
        /// 检索时间 一般为采集时间
        /// </summary>
        public long @timestamp { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public long create_time { get; set; }
        /// <summary>
        /// 项目ID 对内
        /// </summary>
        public string project_id { get; set; }
        /// <summary>
        /// 项目编码 对外
        /// </summary>
        public string project_code { get; set; }
        /// <summary>
        /// 设备类型 参考设备类型字典
        /// </summary>
        public string equipment_type { get; set; }
        /// <summary>
        /// 设备序列码
        /// </summary>
        public string sn { get; set; }
        /// <summary>
        /// 门号
        /// </summary>
        public string gate_no { get; set; }
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

    /// <summary>
    /// 识别方式
    /// </summary>
    public static class Cert_mode
    {
        public static readonly string IC卡 = "01";
        public static readonly string ID卡 = "02";
        public static readonly string 人脸 = "03";
        public static readonly string 指纹 = "04";
        public static readonly string 虹膜 = "05";
    }
    /// <summary>
    /// 进出类型
    /// </summary>
    public static class In_or_out
    {
        public static readonly string 进 = "01";
        public static readonly string 出 = "02";
    }
}
