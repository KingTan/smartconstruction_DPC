using Architecture;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TCPAPI;
using ToolAPI;

namespace ProtocolAnalysis
{
    public class GprsResolveDataV101
    {
        public static void ProtocolPackageResolver(byte[] b, int c, TcpSocketClient client)
        {
            DBFrame df = new DBFrame();
            df.contenthex = ConvertData.ToHexString(b, 0, c);

            df.version = (client.External.External as TcpClientBindingExternalClass).TVersion;
            TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;
            if (c == 8 && b[0] == 0x65 && b[1] == 0x77)//心跳 网关号包含ew的8位
            {
                byte[] sn = new byte[8];   //设备编号
                Array.Copy(b, 0, sn, 0, 8);
                string Gateway_SN = Encoding.ASCII.GetString(sn);
                if (TcpExtendTemp.EquipmentID == null || TcpExtendTemp.EquipmentID.Equals(""))
                {
                    TcpExtendTemp.EquipmentID = Gateway_SN;//网关号存储
                }
            }
            if (b[0] == 0xfe && b[1] == 0xfe && b[2] == 0xfe && b[3] == 0x68)
            {
                ReceiveNogateway_Current(b, client, ref df);//以后主要就用这个方法了
            }
            else if (b[0] == 0xfe && b[1] == 0x68)
            {
                ReceiveNogateway_CurrentThree(b, client, ref df);
            }
            //else
            //    Receivegateway_Current(b, client, ref df);

            //存入数据库
            if (df.contentjson != null && df.contentjson != "")
            {
                df.version = "1.0";
                DB_MysqlElectric.SaveElectricCurrent(df);
            }
        }
        /// <summary>
        /// 不带网关标识的数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="client"></param>
        /// <param name="df"></param>
        static public void ReceiveNogateway_Current(byte[] b, TcpSocketClient client, ref DBFrame df)
        {
            Frame_eCurrent data = new Frame_eCurrent();
            try
            {
                data.Gateway_SN = (client.External.External as TcpClientBindingExternalClass).EquipmentID;
                data.equipmentNo = string.Format("{0}{1}{2}{3}{4}{5}", b[9].ToString("X2"), b[8].ToString("X2"), b[7].ToString("X2"), b[6].ToString("X2"), b[5].ToString("X2"), b[4].ToString("X2"));
                if (b[11].ToString("X2") == "81" && ((byte.Parse(b[12].ToString())) >= 2))
                {
                    string IdentificationCode = string.Format("{0}{1}", (b[14] - 51).ToString("X2"), (b[13] - 51).ToString("X2")); //读电表电能的标识
                    if (IdentificationCode == "9010") //9010代表读取电能的标识
                    {
                        data.energy = string.Format("{0}{1}{2}.{3}", (b[18] - 51).ToString("X2"), (b[17] - 51).ToString("X2"), (b[16] - 51).ToString("X2"), (b[15] - 51).ToString("X2"));
                        data.energy = double.Parse(data.energy).ToString("0.00");
                        df.deviceid = data.equipmentNo;
                        df.datatype = "current";
                        df.contentjson = JsonConvert.SerializeObject(data);
                    }
                    else if (IdentificationCode == "C028") //读闸的状态
                    {
                        string binary = Convert.ToString((b[15] - 51), 2);
                        string status = binary.Substring(binary.Length - 1); //闸状态
                                                                             //更新数据库中的电表闸状态根据设备号
                        DB_MysqlElectric.UpdateElectricStatus(data.equipmentNo, status);
                    }

                }
                else if (b[11].ToString("X2") == "84") //拉闸合闸的应答
                {
                    //根据设备号更新拉闸合闸的应答，只要来应答就更新状态为1
                    DB_MysqlElectric.UpdateResponseStatus(data.equipmentNo);
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// 三项电表数据解析
        /// </summary>
        /// <param name="data"></param>
        /// <param name="client"></param>
        /// <param name="df"></param>
        static public void ReceiveNogateway_CurrentThree(byte[] b, TcpSocketClient client, ref DBFrame df)
        {
            Frame_eCurrent data = new Frame_eCurrent();
            try
            {
                data.Gateway_SN = (client.External.External as TcpClientBindingExternalClass).EquipmentID;
                data.equipmentNo = string.Format("{0}{1}{2}{3}{4}{5}", b[7].ToString("X2"), b[6].ToString("X2"), b[5].ToString("X2"), b[4].ToString("X2"), b[3].ToString("X2"), b[2].ToString("X2"));
                if (b[9].ToString("X2") == "81" && ((byte.Parse(b[10].ToString())) >= 2))
                {
                    string IdentificationCode = string.Format("{0}{1}", (b[12] - 51).ToString("X2"), (b[11] - 51).ToString("X2")); //读电表电能的标识
                    if (IdentificationCode == "9010") //9010代表读取电能的标识
                    {
                        data.energy = string.Format("{0}{1}{2}.{3}", (b[16] - 51).ToString("X2"), (b[15] - 51).ToString("X2"), (b[14] - 51).ToString("X2"), (b[13] - 51).ToString("X2"));
                        data.energy = double.Parse(data.energy).ToString("0.00");
                        df.deviceid = data.equipmentNo;
                        df.datatype = "current";
                        df.contentjson = JsonConvert.SerializeObject(data);
                    }


                }
                else if (b[11].ToString("X2") == "84") //拉闸合闸的应答 761.74
                {
                    //根据设备号更新拉闸合闸的应答，只要来应答就更新状态为1
                    DB_MysqlElectric.UpdateResponseStatus(data.equipmentNo);
                }
            }
            catch
            {

            }
        }
        /// <summary>
        /// 带网关标识的数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="client"></param>
        /// <param name="df"></param>
        static public void Receivegateway_Current(byte[] b, TcpSocketClient client, ref DBFrame df)
        {
            Frame_eCurrent data = new Frame_eCurrent();
            try
            {
                data.Gateway_SN = ((UInt16)(b[0] * 256 + b[1])).ToString();
                if (b[4] == 0xfe && b[5] == 0xfe && b[6] == 0xfe && b[7] == 0x68)
                {
                    data.equipmentNo = string.Format("{0}{1}{2}{3}{4}{5}", b[13].ToString("X2"), b[12].ToString("X2"), b[11].ToString("X2"), b[10].ToString("X2"), b[9].ToString("X2"), b[8].ToString("X2"));
                    if (b[15].ToString("X2") == "81" && ((byte.Parse(b[16].ToString())) >= 2))
                    {
                        string IdentificationCode = string.Format("{0}{1}", (b[18] - 51).ToString("X2"), (b[17] - 51).ToString("X2"));
                        if (IdentificationCode == "9010")//读电表正向电功
                        {
                            data.energy = string.Format("{0}{1}{2}.{3}", (b[22] - 51).ToString("X2"), (b[21] - 51).ToString("X2"), (b[20] - 51).ToString("X2"), (b[19] - 51).ToString("X2"));
                            data.energy = double.Parse(data.energy).ToString("0.00");
                            //数据库存储实时数据的json
                            df.deviceid = data.equipmentNo;
                            df.datatype = "current";
                            df.contentjson = JsonConvert.SerializeObject(data);
                        }
                        else if (IdentificationCode == "C028") //读闸状态
                        {
                            string binary = Convert.ToString((b[19] - 51), 2);
                            string status = binary.Substring(binary.Length - 1);//0表示合闸  1表示关闸
                                                                                //直接更新数据库中的状态
                            DB_MysqlElectric.UpdateElectricStatus(data.equipmentNo, status);
                        }
                    }
                }
                else if (b[15].ToString("X2") == "84") //合闸和拉闸的应答
                {
                    //根据设备号更新拉闸合闸的应答，只要来应答就更新状态为1
                    DB_MysqlElectric.UpdateResponseStatus(data.equipmentNo);
                }
            }
            catch
            {

            }
        }
    }
}
