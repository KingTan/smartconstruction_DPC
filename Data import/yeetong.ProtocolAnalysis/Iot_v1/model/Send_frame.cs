using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtocolAnalysis.Iot_v1.model
{
    /// <summary>
    /// 物联网设备发送帧
    /// </summary>
    public class Send_frame
    {
        /// <summary>
        /// 帧类型
        /// </summary>
        public string frame_type { get; set; }
        /// <summary>
        /// 设备类型
        /// </summary>
        public string equipment_type { get; set; }
        /// <summary>
        /// 时间
        /// </summary>
        public string time_stamp { get; set; }
        /// <summary>
        /// 帧验证
        /// </summary>
        public string frame_token { get; set; }
        /// <summary>
        /// 数据域
        /// </summary>
        public object data { get; set; }
    }

    /// <summary>
    /// 设备类型静态类
    /// </summary>
    public static class Equipment_type
    {
        public static readonly string 塔机 = "tower";
        public static readonly string 升降机 = "lift";
        public static readonly string 卸料平台 = "discharge";
        public static readonly string 雾泡喷淋 = "Fog_gun";
        public static readonly string 扬尘噪音 = "dust_noise";
        public static readonly string 人员管理 = "personnel";
    }

    /// <summary>
    /// 帧类型类型静态类
    /// </summary>
    public static class Frame_type
    {
        public static readonly string 注册帧 = "register";
        public static readonly string 心跳帧 = "heart_beat";
        public static readonly string 数据帧 = "real_time_data";
    }
}
