using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtocolAnalysis.Iot_v1.model
{
    /// <summary>
    /// 应答帧
    /// </summary>
    public class Reply_frame
    {
        /// <summary>
        /// 结果码
        /// </summary>
        public int error_code { get; set; }
        /// <summary>
        /// 结果描述
        /// </summary>
        public string result_message { get; set; }
        /// <summary>
        /// 时间
        /// </summary>
        public string time_stamp { get; set; }
        /// <summary>
        /// 数据域
        /// </summary>
        public object data { get; set; }
    }

    public static class Result_code
    {
        public static int success = 200;
        public static string  success_des = "success";
        public static int vendor_code_error = 201;
        public static string vendor_code_error_des = "non-existent vendor_code";
        public static int frame_token_error = 202;
        public static string frame_token_error_des = "non-existent frame_token";
        public static int data_error = 203;
        public static string data_error_des = "data format error";
        public static int unknown_error = 203;
        public static string unknown_error_des = "unknown error";
    }
}
