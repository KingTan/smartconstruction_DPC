using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Architecture;
using TCPAPI;
using ToolAPI;
/*---------------------------------------------
Copyright (c) 2017 共友科技
版权所有：共友科技
创建人名：赵通
创建描述：协议解析主入口
创建时间：2017.10.11
文件功能描述：协议解析主入口，根据版本号进行分流
修改人名：
修改描述：
修改标识：
修改时间：
---------------------------------------------*/
namespace ProtocolAnalysis
{
    public class ProtocolAnalysisSE_Main
    {
        //用于塔吊和升降机的
        public delegate string OnResolveRecvMessagedelegate(byte[] b, int c, TcpSocketClient client);
        public delegate string OnResoleRecvMessageUdpdelegate(byte[] b, int c, UdpState udp); //用于udp接收
        public static void ProtocolPackageResolver(byte[] b, int c, TcpSocketClient client)
        {
            switch (MainStatic.DeviceType)
            {
                //塔吊
                case 0: ProtocolPackageResolver_TowerCrane(b, c, client); break;
                //升降机
                case 1: ProtocolPackageResolver_Lift(b, c, client); break;
                //卸料
                case 2: ProtocolPackageResolver_Unload(b, c, client); break;
                //雾炮
                case 3: ProtocolPackageResolver_FogGun(b, c, client); break;
                //扬尘
                case 4: ProtocolPackageResolver_RaiseDustNoise(b, c, client); break;
                //大体积混凝土
                case 5: ProtocolPackageResolver_MassConcrete(b, c, client); break;
                //安全帽
                case 6: ProtocolPackageResolver_Softhat(b, c, client); break;
                //红外对射
                case 7: ProtocolPackageResolver_InfraredContrast(b, c, client); break;
                //气体监测
                case 8: ProtocolPackageResolver_GasDetection(b, c, client); break;
                //身份验证相关
                case 10: ProtocolPackageResolver_IdentityVerification(b, c, client); break;
                //电表
                case 12: ProtocolPackResolver_Electric(b, c, client); break;
                //临边防护
                case 13: ProtocolPackResolver_BorderProtection(b, c, client); break;

                default: break;

            }
        }


        #region 塔吊
        public static void ProtocolPackageResolver_TowerCrane(byte[] b, int c, TcpSocketClient client)
        {
            switch (b[0])
            {
                case 0xA5:
                    GoYOTower0xA5(b, c, client);
                    break;
                case 0x7E:
                    GoYOTower0x7E(b, c, client);
                    break;
            }
        }
        #region 0x7E开头的处理
        static OnResolveRecvMessagedelegate OnResolveRecvMessagede_7E0E = GprsResolveDataV0E.OnResolveRecvMessage;
        static OnResolveRecvMessagedelegate OnResolveRecvMessagede_7E01 = GprsResolveDataV01.OnResolveRecvMessage;
        static OnResolveRecvMessagedelegate OnResolveRecvMessagede_7E11 = GprsResolveDataV11.OnResolveRecvMessage;
        private static void GoYOTower0x7E(byte[] b, int c, TcpSocketClient client)
        {
            TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;
            switch (b[2])
            {
                case 0x0E:
                    TcpExtendTemp.TVersion = "7E7E0E";
                    GoYOUnpack(b, c, client, "7E7E0E", "7D7D", OnResolveRecvMessagede_7E0E);
                    break;
                case 0x01:
                    TcpExtendTemp.TVersion = "7E7E0E";
                    GoYOUnpack(b, c, client, "7E7E01", "7D7D", OnResolveRecvMessagede_7E01);
                    break;
                case 0x11:
                    TcpExtendTemp.TVersion = "7E7E0E";
                    GoYOUnpack(b, c, client, "7E7E11", "7D7D", OnResolveRecvMessagede_7E11);
                    break;
                default: break;
            }
        }
        #endregion
        #region 0xA5开头的处理
        static OnResolveRecvMessagedelegate OnResolveRecvMessagedeV021303 = GprsResolveDataV021303.OnResolveRecvMessage;
        private static void GoYOTower0xA5(byte[] b, int c, TcpSocketClient client)
        {
            TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;
            if (b[2] == 0x02 && b[3] == 0x13 && b[4] == 0x03)
            {
                TcpExtendTemp.TVersion = "A55A021303";
                GoYOUnpack(b, c, client, "A55A021303", "EEFF", OnResolveRecvMessagedeV021303);
            }
        }
        #endregion
        #endregion

        #region 升降机
        static OnResolveRecvMessagedelegate OnResolveRecvMessagede_7A06 = GprsResolveDataV010206.OnResolveRecvMessage;
        static OnResolveRecvMessagedelegate OnResolveRecvMessagede_7A09 = GprsResolveDataV010209.OnResolveRecvMessage;
        static OnResolveRecvMessagedelegate OnResolveRecvMessagede_7A10 = GprsResolveDataV010210.OnResolveRecvMessage;
        static OnResolveRecvMessagedelegate OnResolveRecvMessagede_7A010300 = GprsResolveDataV010300.OnResolveRecvMessage;
        static OnResolveRecvMessagedelegate OnResolveRecvMessagede_7A010400 = GprsResolveDataV010400.OnResolveRecvMessage;
        public static void ProtocolPackageResolver_Lift(byte[] b, int c, TcpSocketClient client)
        {
            TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;
            if (b[2] == 0x01 && b[3] == 0x02 && b[4] == 0x06)
            {
                TcpExtendTemp.TVersion = "010206";
                GoYOUnpack(b, c, client, "7A7A010206", "7B7B", OnResolveRecvMessagede_7A06);
            }
            else if (b[2] == 0x01 && b[3] == 0x02 && b[4] == 0x09) //珠海的升降机
            {
                TcpExtendTemp.TVersion = "010209";
                GoYOUnpack(b, c, client, "7A7A010209", "7B7B", OnResolveRecvMessagede_7A09);
            }
            else if (b[2] == 0x01 && b[3] == 0x02 && b[4] == 0x10)
            {
                TcpExtendTemp.TVersion = "010210";
                GoYOUnpack(b, c, client, "7A7A010210", "7B7B", OnResolveRecvMessagede_7A10);
            }
            else if (b[2] == 0x01 && b[3] == 0x03 && b[4] == 0x00)
            {
                TcpExtendTemp.TVersion = "010300";
                GoYOUnpack(b, c, client, "7A7A010300", "7B7B", OnResolveRecvMessagede_7A010300);
            }
            else if (b[2] == 0x01 && b[3] == 0x04 && b[4] == 0x00)
            {
                TcpExtendTemp.TVersion = "010300";
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

        #region 雾炮
        static OnResolveRecvMessagedelegate OnResolveRecvMessagede_FogGun7A02 = ProtocolAnalysis.FogGun.GprsResolveDataV010002.OnResolveRecvMessage;
        static OnResolveRecvMessagedelegate OnResolveRecvMessagede_FogGun7A03 = ProtocolAnalysis.FogGun.GprsResolveDataV010003.OnResolveRecvMessage;
        static OnResolveRecvMessagedelegate OnResolveRecvMessagede_FogGun7A0200 = ProtocolAnalysis.FogGun.GprsResolveDataV020000.OnResolveRecvMessage;
        public static void ProtocolPackageResolver_FogGun(byte[] b, int c, TcpSocketClient client)
        {
            TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;
            if (b[2] == 0x01 && b[3] == 0x00 && b[4] == 0x02)
            {
                TcpExtendTemp.TVersion = "010002";
                GoYOUnpack(b, c, client, "7A7A010002", "7B7B", OnResolveRecvMessagede_FogGun7A02);
            }
            if (b[2] == 0x01 && b[3] == 0x00 && b[4] == 0x03)
            {
                TcpExtendTemp.TVersion = "010003";
                GoYOUnpack(b, c, client, "7A7A010003", "7B7B", OnResolveRecvMessagede_FogGun7A03);
            }
            if (b[2] == 0x02 && b[3] == 0x00 && b[4] == 0x00)
            {
                TcpExtendTemp.TVersion = "020000";
                GoYOUnpack(b, c, client, "7A7A020000", "7B7B", OnResolveRecvMessagede_FogGun7A0200);
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
            //2323
            if (c > 2 && b[0] == 0x23 && b[1] == 0x23)//蓝丰设备
            {
                string dataTemp = Encoding.ASCII.GetString(b);
                if (dataTemp.Contains("WWYC"))
                {
                    TcpExtendTemp.TVersion = "wwyc";
                    ProtocolAnalysis_WWYC.OnResolveRecvMessage(b, c, client);
                }
                else
                {
                    TcpExtendTemp.TVersion = "lf";
                    ProtocolAnalysis_LF.OnResolveRecvMessage(b, c, client);
                }
            }
            //01 03 60
            //else if (c > 3 && b[0] == 0x01 && b[1] == 0x03 && b[2] == 0x60)//瞭望设备
            //{
            //    TcpExtendTemp.TVersion = "lw";
            //    ProtocolAnalysis_LW.Unpack(b, c, client);
            //}
            //ff aa 
            else if (c > 2 && b[0] == 0xFF && b[1] == 0xAA)//中科正奇设备
            {
                TcpExtendTemp.TVersion = "zkzq";
                ProtocolAnalysis_ZKZQ.Unpack(b, c, client);
            }
            // ** ** ** ** 5E ** ** ** 02
            else if (c > 8 && b[3] == 0x5E && b[7] == 0x02)//创塔设备
            {
                TcpExtendTemp.TVersion = "ct";
                ProtocolAnalysis_CT.OnResolveRecvMessage(b, c, client);
            }
            //威海精讯  fe dc
            else if (c > 2 && b[0] == 0xfe && b[1] == 0xdc)
            {
                //ToolAPI.XMLOperation.WriteLogXmlNoTail("1", "1");
                TcpExtendTemp.TVersion = "whjx";
                ProtocolAnalysis_WHJX.Unpack(b, c, client);
            }
            else if (b[0] == 0x7A && b[1] == 0x7A)
            {
                TcpExtendTemp.TVersion = "goyo";
                if (b[2] == 0x01 && b[3] == 0x00 && b[4] == 0x00)
                {
                    GoYOUnpack(b, c, client, "7A7A010000", "7B7B", ProtocolAnalysis_GOYO.OnResolveRecvMessage);
                }
                else if (b[2] == 0x01 && b[3] == 0x00 && b[4] == 0x03)
                {
                    GoYOUnpack(b, c, client, "7A7A010003", "7B7B", ProtocolAnalysis_New.OnResolveRecvMessage);
                }
                else if (b[2] == 0x01 && b[3] == 0x00 && b[4] == 0x04)
                {
                    GoYOUnpack(b, c, client, "7A7A010004", "7B7B", ProtocolAnalysis_V1.OnResolveRecvMessage);
                }

            }
            else
            {
                TcpExtendTemp.TVersion = "goyo";
                if (b[0] == 0x44 || b[0] == 0x45)
                {
                    byte[] t = new byte[35];
                    for (int i = 0; i < 35; i++)
                    {
                        t[i] = b[i];
                    }

                    Protocolanalysis_XA.OnResolveRecvMessage(t, t.Length, client); 
                }
            }

        }
        #endregion

        #region 大体积混凝土
        public static void ProtocolPackageResolver_MassConcrete(byte[] b, int c, TcpSocketClient client)
        {
            GprsResolveData_Mc.OnResolveRecvMessage(b, c, client);

            GprsResolveData_LRK.OnResolveRecvMessage(b, c, client);
        }

        #endregion


        static OnResolveRecvMessagedelegate OnResolveRecvGprsResolveSoftHat_SO_V1 = GprsResolveSoftHat_SO_V1.OnResolveRecvMessage;
        #region 安全帽
        /// <summary>
        /// 安全帽
        /// </summary>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="client"></param>
        public static void ProtocolPackageResolver_Softhat(byte[] b, int c, TcpSocketClient client)
        {
            //GprsResolveSoftHat.OnResolveRecvMessage(b, c, client);木兰的暂时用不着
            TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;
            if (b[2] == 0x01 && b[3] == 0x00)
            {
                TcpExtendTemp.TVersion = "0100";
                GoYOUnpack(b, c, client, "7A7A0100", "7B7B", OnResolveRecvGprsResolveSoftHat_SO_V1);
                //GprsResolveSoftHat_SO_V1.OnResolveRecvMessage(b,c,client);
            }
            else
            {
                GprsResolveSoftHat_SO.OnResolveRecvMessage(b, c, client);
            }
            
        }
        #endregion

        #region 红外对射
        /// <summary>
        /// 红外对射
        /// </summary>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="client"></param>
        public static void ProtocolPackageResolver_InfraredContrast(byte[] b, int c, TcpSocketClient client)
        {
            GprsResolveInfraredContrast.OnResolveRecvMessage(b, c, client);
        }
        #endregion

        #region 气体监测
        /// <summary>
        /// 气体监测
        /// </summary>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="client"></param>
        public static void ProtocolPackageResolver_GasDetection(byte[] b, int c, TcpSocketClient client)
        {
            string str = ConvertData.ToHexString(b, 0, 2);
            if (str == "FEDC")
            {
                GprsResolveGasDetection.OnResolveRecvMessageAboutCO(b, c, client);
            }
            else
            {
                //甲烷乙炔共用方法
                GprsResolveGasDetection.OnResolveRecvMessage(b, c, client);
            }
        }
        #endregion
        #region 定制身份验证
        /// <summary>
        /// 定制身份验证
        /// </summary>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="client"></param>
        public static void ProtocolPackageResolver_IdentityVerification(byte[] b, int c, TcpSocketClient client)
        {
            TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;
            TcpExtendTemp.TVersion = "7E0100";
            GprsResolve_IdentityVerification.OnResolveRecvMessage(b, c, client);
        }
        #endregion
        #region 电表
        /// <summary>
        /// 电表
        /// </summary>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="client"></param>
        public static void ProtocolPackResolver_Electric(byte[] b, int c, TcpSocketClient client)
        {
            TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;
            TcpExtendTemp.TVersion = "";
            GprsResolveDataV101.ProtocolPackageResolver(b, c, client);
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
        /// <summary>
        ///临边防护
        /// </summary>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="client"></param>
        static OnResolveRecvMessagedelegate OnResolveRecvMessagede_BorderProtectinV100000 = ProtocolAnalysis.BorderProtection.GprsResolveDataV010000.OnResolveRecvMessage;
        public static void ProtocolPackResolver_BorderProtection(byte[] b, int c, TcpSocketClient client)
        {
            TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;
            if (b[2] == 0x01 && b[3] == 0x00 && b[4] == 0x00)
            {
                TcpExtendTemp.TVersion = "010000";
                GoYOUnpack(b, c, client, "7A7A010000", "7B7B", OnResolveRecvMessagede_BorderProtectinV100000);
            }
        }

        #endregion
    }
}
