using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Architecture;
using TCPAPI;
using ToolAPI;
using ProtocolAnalysis.Iot_v1.operation;
namespace ProtocolAnalysis
{
    public class ProtocolAnalysisSE_Main
    {
        //用于塔吊和升降机的
        public delegate string OnResolveRecvMessagedelegate(byte[] b, int c, TcpSocketClient client);
        public static void ProtocolPackageResolver(byte[] b, int c, TcpSocketClient client)
        {
            switch (MainStatic.DeviceType)
            {
                //物联网通用 -1
                case -1: Iot_send_frame.OnResolveRecvMessage(b, c, client); break;
                //塔吊
                case 0: ProtocolPackageResolver_TowerCrane(b, c, client); break;
                //升降机
                case 1: ProtocolPackageResolver_Lift(b, c, client); break;
                //卸料
                case 2: ProtocolPackageResolver_Unload(b, c, client); break;
                //扬尘
                case 4: ProtocolPackageResolver_RaiseDustNoise(b, c, client); break;
                default: break;

            }
        }

        #region 塔吊
        public static void ProtocolPackageResolver_TowerCrane(byte[] b, int c, TcpSocketClient client)
        {
            switch (b[0])
            {
                //goyo
                case 0x7E:
                    GoYOTower0x7E(b, c, client);
                    break;
            }
        }
        #region 0x7E开头的处理
        static OnResolveRecvMessagedelegate OnResolveRecvMessagede_7E0E = GprsResolveDataV0E.OnResolveRecvMessage;
        private static void GoYOTower0x7E(byte[] b, int c, TcpSocketClient client)
        {
            TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;
            switch (b[2])
            {
                case 0x0E:
                    TcpExtendTemp.TVersion = "7E7E0E";
                    GoYOUnpack(b, c, client, "7E7E0E", "7D7D", OnResolveRecvMessagede_7E0E);
                    break;
                default: break;
            }
        }
        #endregion
        #endregion

        #region 升降机
        static OnResolveRecvMessagedelegate OnResolveRecvMessagede_7A010400 = GprsResolveDataV010400.OnResolveRecvMessage;
        public static void ProtocolPackageResolver_Lift(byte[] b, int c, TcpSocketClient client)
        {
            TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;
            if (b[2] == 0x01 && b[3] == 0x04 && b[4] == 0x00)
            {
                TcpExtendTemp.TVersion = "010400";
                GoYOUnpack(b, c, client, "7A7A010400", "7B7B", OnResolveRecvMessagede_7A010400);
            }
        }
        #endregion

        #region 卸料
        static OnResolveRecvMessagedelegate OnResolveRecvMessagede_7A02 = GprsResolveDataV102.OnResolveRecvMessage;
        public static void ProtocolPackageResolver_Unload(byte[] b, int c, TcpSocketClient client)
        {
            TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;
            if (b[2] == 0x01 && b[3] == 0x00 && b[4] == 0x02)
            {
                TcpExtendTemp.TVersion = "010002";
                GoYOUnpack(b, c, client, "7A7A010002", "7B7B", OnResolveRecvMessagede_7A02);
            }
        }
        #endregion

       

        #region 扬尘入口
        /// <summary>
        /// 扬尘噪音入口
        /// </summary>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="client"></param>
        public static void ProtocolPackageResolver_RaiseDustNoise(byte[] b, int c, TcpSocketClient client)
        {
            //符合字节流协议处理
            TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;
            // ** ** ** ** 5E ** ** ** 02
            if (c > 8 && b[3] == 0x5E && b[7] == 0x02)//创塔设备
            {
                TcpExtendTemp.TVersion = "ct";
                ProtocolAnalysis_CT.OnResolveRecvMessage(b, c, client);
            }
           
            else if (b[0] == 0x7A && b[1] == 0x7A)
            {
                TcpExtendTemp.TVersion = "goyo";
                if (b[2] == 0x01 && b[3] == 0x00 && b[4] == 0x04)
                {
                    GoYOUnpack(b, c, client, "7A7A010004", "7B7B", ProtocolAnalysis_V1.OnResolveRecvMessage);
                }

            }

        }
        #endregion

        #region 拆包算法
        /// <summary>
        /// 拆包算法
        /// </summary>
        /// <param name="b">tcp接收到的字节流</param>
        /// <param name="c">tcp接收到的字节流长度</param>
        /// <param name="client">TcpSocketClient对象</param>
        /// <param name="startStr">可以进行拆分表示的协议头</param>
        /// <param name="endStr">可以进行拆分表示的协议尾</param>
        /// <param name="OnResolveRecvMessagede">最后调用的解析类方法</param>
        private static void GoYOUnpack(byte[] b, int c, TcpSocketClient client, string startStr, string endStr, OnResolveRecvMessagedelegate OnResolveRecvMessagede)
        {
            //得到帧组集合
            string dataHexString = ConvertData.ToHexString(b, 0, c);
            string[] stringSeparators = new string[] { startStr };
            //判断起始符+版本号进行分割包
            string[] DataHexAry = dataHexString.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < DataHexAry.Length; i++)
            {
                //判断结尾来确定帧是否完整
                if (DataHexAry[i].Length > endStr.Length)
                {
                    string ending = DataHexAry[i].Substring(DataHexAry[i].Length - endStr.Length);
                    if (ending.Equals(endStr))//一个完整的帧
                    {
                        //转换为字节数组
                        string frames = startStr + DataHexAry[i];
                        byte[] framesByte = ConvertData.HexToByte(frames);
                        //FileHelp.FileAppend(string.Format("【{0}】设备连接传入数据：{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), ConvertData.ToHexString(framesByte, 0, framesByte.Length)));
                        //进入对应的解析类
                        OnResolveRecvMessagede(framesByte, framesByte.Length, client);
                    }
                }
            }
        }
        #endregion
    }
}
