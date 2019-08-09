using Architecture;
using Newtonsoft.Json;
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
    /// iot解析协议
    /// </summary>
    public class Iot_send_frame
    {

        public static void OnResolveRecvMessage(byte[] b, int c, TcpSocketClient client)
        {
            try
            {
                string datastring = Encoding.UTF8.GetString(b, 0, c);
                Send_frame sf = JsonConvert.DeserializeObject<Send_frame>(datastring);
                if(sf==null)
                {
                    //数据错误
                    string sendmessage = JsonConvert.SerializeObject(Iot_reply_frame.Get_reply_frame(Result_code.data_error, Result_code.data_error_des));
                    client.SendMessage(sendmessage);
                }
                else if(sf.frame_type== Frame_type.注册帧)
                {
                    string frame_token  = Register_operation.Judge_vendor_code(sf.data);
                    if(string.IsNullOrEmpty(frame_token))
                    {
                        //数据错误
                        string sendmessage = JsonConvert.SerializeObject(Iot_reply_frame.Get_reply_frame(Result_code.vendor_code_error, Result_code.vendor_code_error_des));
                        client.SendMessage(sendmessage);
                        //杀死该socket
                        client.DisSocket();
                    }
                    else
                    {
                        TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;
                        TcpExtendTemp.uuid = frame_token;//frame_token
                        //发送正确的应答
                        string sendmessage = JsonConvert.SerializeObject(Iot_reply_frame.Get_reply_frame( new Register_reply_frame() { frame_token = frame_token }));
                        client.SendMessage(sendmessage);
                    }
                }
                else if (sf.frame_type == Frame_type.心跳帧)
                {
                    TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;
                    TcpExtendTemp.uuid = "";//frame_token
                    if (TcpExtendTemp.uuid == null || TcpExtendTemp.uuid == "" || TcpExtendTemp.uuid != sf.frame_token)
                    {   //帧token错误
                        string sendmessage = JsonConvert.SerializeObject(Iot_reply_frame.Get_reply_frame(Result_code.frame_token_error, Result_code.frame_token_error_des));
                        client.SendMessage(sendmessage);
                        //杀死该socket
                        client.DisSocket();
                    }
                    else
                    {
                        //正确应答
                        string sendmessage = JsonConvert.SerializeObject(Iot_reply_frame.Get_reply_frame(Result_code.success, Result_code.success_des));
                        client.SendMessage(sendmessage);
                    }
                }
                else if (sf.frame_type == Frame_type.数据帧)
                {
                    TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;
                    TcpExtendTemp.uuid = "";//frame_token
                    if (TcpExtendTemp.uuid == null || TcpExtendTemp.uuid == "" || TcpExtendTemp.uuid != sf.frame_token)
                    {   //帧token错误
                        string sendmessage = JsonConvert.SerializeObject(Iot_reply_frame.Get_reply_frame(Result_code.frame_token_error, Result_code.frame_token_error_des));
                        client.SendMessage(sendmessage);
                        //杀死该socket
                        client.DisSocket();
                    }
                    else
                    {
                        //正确应答
                        string sendmessage = JsonConvert.SerializeObject(Iot_reply_frame.Get_reply_frame(Result_code.success, Result_code.success_des));
                        client.SendMessage(sendmessage);
                        //todo解析
                        Data_frame_operation.Data_operation(sf.equipment_type,sf.data);
                    }
                }
                else
                {
                    //数据错误
                    string sendmessage = JsonConvert.SerializeObject(Iot_reply_frame.Get_reply_frame(Result_code.data_error, Result_code.data_error_des));
                    client.SendMessage(sendmessage);
                    //杀死该socket
                    client.DisSocket();
                }
            }
            catch(Exception ex)
            {
                if(client!=null)
                {
                    string sendmessage = JsonConvert.SerializeObject(Iot_reply_frame.Get_reply_frame(Result_code.unknown_error, Result_code.vendor_code_error_des));
                    client.SendMessage(sendmessage);
                    //杀死该socket
                    client.DisSocket();
                }
            }

        }
    }
}
