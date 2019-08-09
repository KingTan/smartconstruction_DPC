using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProtocolAnalysis.Iot_v1.model
{
    public class Register_reply_frame
    {
        /// <summary>
        /// 帧验证
        /// </summary>
        public string frame_token { get; set; }
    }


}
