﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Architecture;
using ProtocolAnalysis.TowerCrane;
using TCPAPI;
using ToolAPI;

namespace ProtocolAnalysis
{
    public class ProtocolAnalysisSE_MainUdp
    {
        public delegate string OnResoleRecvMessageUdpdelegate(byte[] b, int c, UdpState udp); //用于udp接收

        public static void ProtocolPackageUdpResolver(byte[] b, UdpState udp)
        {
            //string log= ConvertData.ToHexString(b, 0, b.Length);
           // ToolAPI.XMLOperation.WriteLogXmlNoTail(System.Windows.Forms.Application.StartupPath + @"\OriginalPackage", "", log);
            //烟感及强电
            switch (MainStatic.DeviceType)
            {
                case 0:  ProtocolPackageResolver_Crane(b, udp); break;
                case 9: ProtocolPackageResolver_Smoke(b, udp); break;
                case 11: ProtocolPackageResolver_StrongEMonitor(b, udp); break;
                default: break;
            }
        }
        /// <summary>
        /// 塔机
        /// </summary>
        /// <param name="b"></param>
        /// <param name="client"></param>
        public static void ProtocolPackageResolver_Crane(byte[] b, UdpState client)
        {
            if (b.Length == 54 || b.Length == 29)
            {
                //判断帧类型  Convert.ToString(d, 2)
                string Frame = Convert.ToString(Convert.ToInt16(b[3]), 2).PadLeft(8, '0') + Convert.ToString(Convert.ToInt16(b[4]), 2).PadLeft(8, '0');
                //帧类型（截取）string.Format("{0:x}",Convert.ToInt32(Frame.Substring(0, 5),2)
                string FrameType = string.Format("{0:x}", Convert.ToInt32(Frame.Substring(0, 5), 2));
                if (FrameType.Equals("2"))//实时数据
                {
                    GprsResolveStrongCD.OnResolveRecvCurrentMessage(b, client);
                    ToolAPI.XMLOperation.WriteLogXmlNoTail("OnResolveRecvMessage-->收到数据", "11111111111111111");
                }
                else if (FrameType.Equals("4"))//获取设备号
                {
                    GprsResolveStrongCD.OnResolveRecvEquipmentMessage(b, client);
                    ToolAPI.XMLOperation.WriteLogXmlNoTail("OnResolveRecvMessage-->收到数据", "2222222222222222222");
                }
            }
        }
        /// <summary>
        /// 数组转string
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ToHexString(byte[] bytes) // 0xae00cf => "AE00CF "
        {
            string hexString = string.Empty;
            if (bytes != null)
            {
                StringBuilder strB = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    strB.Append(bytes[i].ToString("X2"));
                }
                hexString = strB.ToString();
            }
            return hexString;
        }
        /// <summary>
        /// 烟感
        /// </summary>
        /// <param name="b"></param>
        /// <param name="client"></param>
        public static void ProtocolPackageResolver_Smoke(byte[] b, UdpState client)
        {
            if (ConvertData.ToHexString(b, 0, 1) == "40")
            {
                GoYOUnpack(b, b.Length, client, "4040", "2323");
            }
            else

            {
                GprsResolveSmoke.OnResolveRecvMessage(b, client);
            }
            
        }
        /// <summary>
        /// 强电监测
        /// </summary>
        /// <param name="b"></param>
        /// <param name="client"></param>
        public static void ProtocolPackageResolver_StrongEMonitor(byte[] b, UdpState client)
        {
            GprsResolveStrongE.OnResolveRecvMessage(b, client);
        }

        /// <summary>
        /// 拆包算法
        /// </summary>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="client"></param>
        /// <param name="startStr"></param>
        /// <param name="endStr"></param>
        private static void GoYOUnpack(byte[] b, int c, UdpState client, string startStr, string endStr)
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
                        ProtocolAnalysis.Smoke.ResolveSmoke.OnResolveRecvMessage(framesByte, client);
                    }
                }
            }
        }
    }
}
