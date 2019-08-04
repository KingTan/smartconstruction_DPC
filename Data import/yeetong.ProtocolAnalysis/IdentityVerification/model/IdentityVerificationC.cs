using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtocolAnalysis.IdentityVerification.model
{
    /// <summary>
    /// 身份验证
    /// </summary>
    public class IdentityVerificationC
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        public string Equipment { get; set; }
        /// <summary>
        /// 命令字
        /// </summary>
        public string Order_flag { get; set; }
        /// <summary>
        /// RTC
        /// </summary>
        public string RTC { get; set; }
        /// <summary>
        /// 身份证 命令0，3
        /// </summary>
        public string Identity_card { get; set; }
        /// <summary>
        /// 状态 命令字为0时
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 结果 应答命令字0，3
        /// </summary>
        public string Result { get; set; }
        /// <summary>
        /// 总包数 
        /// </summary>
        public string Package_tote { get; set; }
        /// <summary>
        /// 当前包 
        /// </summary>
        public string Package_current { get; set; }
        /// <summary>
        /// 特种类型 当前为1
        /// </summary>
        public string Characteristic_type { get; set; }
        /// <summary>
        /// 特种数据
        /// </summary>
        public byte[] Characteristic_data { get; set; }

        //特征数据里的具体东西 身份证号上面有
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 虹膜特征库
        /// </summary>
        public byte[] Iris { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string Creat_time { get; set; }

        public IdentityVerificationC()
        {
            Equipment = "";
            RTC = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Identity_card = "";
            Creat_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Status = "0";
            Result = "1";
            Package_tote = "1";
            Package_current = "1";
            Characteristic_type = "1";
            Characteristic_data = new byte[512];
            Name = "";
            Iris = new byte[512];
        }
    }
}
