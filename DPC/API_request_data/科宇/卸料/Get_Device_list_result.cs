using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_request_data.科宇.卸料
{
    public class Get_Device_list_result
    {
        /// <summary>
        /// 状态信息
        /// </summary>
        public string message { get; set; }
        /// <summary>
        /// 状态码
        /// </summary>
        public string code { get; set; }
        /// <summary>
        /// 访问状态码 status>=1为访问成功，status<1为访问失败
        /// </summary>
        public string status { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        public Unload_device[] data { get; set; }
    }

    public class Unload_device
    {
        /// <summary>
        /// 设备id
        /// </summary>
        public string unload_id { get; set; }
        /// <summary>
        /// 设备名称
        /// </summary>
        public string unload_name { get; set; }
        /// <summary>
        /// 设备地址
        /// </summary>
        public string unload_address { get; set; }
        /// <summary>
        /// 激活状态 1已激活；0未激活；2设备被删除
        /// </summary>
        public string activated { get; set; }
        /// <summary>
        /// 设备IMEI码
        /// </summary>
        public string verifyCode { get; set; }

    }
}
