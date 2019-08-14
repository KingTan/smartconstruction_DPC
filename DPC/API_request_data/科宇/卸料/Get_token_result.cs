using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_request_data.科宇.卸料
{
    public class Get_token_result
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
        public Token data { get; set; }
    }

    public class Token
    {
        /// <summary>
        /// 获取的token
        /// </summary>
        public string token { get; set; }
    }
}
