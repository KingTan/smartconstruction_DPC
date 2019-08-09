using ProtocolAnalysis.Iot_v1.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPAPI;

namespace ProtocolAnalysis.Iot_v1.operation
{
    /// <summary>
    /// 应答包拼接
    /// </summary>
    public class Iot_reply_frame
    {
        public static Reply_frame Get_reply_frame(int code,string code_des)
        {
            Reply_frame reply_Frame = new Reply_frame();
            reply_Frame.error_code = code;
            reply_Frame.result_message = code_des;
            reply_Frame.time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            reply_Frame.data = new Empty_data();
            return reply_Frame;
        }
        public static Reply_frame Get_reply_frame(Register_reply_frame register_Reply_Frame)
        {
            Reply_frame reply_Frame = new Reply_frame();
            reply_Frame.error_code = Result_code.success;
            reply_Frame.result_message = Result_code.success_des;
            reply_Frame.time_stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            reply_Frame.data = register_Reply_Frame;
            return reply_Frame;
        }
    }
}
