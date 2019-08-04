using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using TCPAPI;
using System.Reflection;
using ToolAPI;
using Newtonsoft.Json;
using Architecture;
using ProtocolAnalysis.RaiseDustNoise;
namespace ProtocolAnalysis
{
    public class ProtocolAnalysis_WHJX
    {
        //
        private static void OnResolveRecvMessage(byte[] b, int c, TcpSocketClient client)
        {
            try
            {
                //ToolAPI.XMLOperation.WriteLogXmlNoTail("2", "2");
                //if (b.Length >= 106)
                //{
              DBFrame df = new DBFrame();
                df.contenthex = ConvertData.ToHexString(b, 0, c);
                df.version = (client.External.External as TcpClientBindingExternalClass).TVersion;

                Current_WHJX modelObject = new Current_WHJX();
                byte version = b[2];//版本
                modelObject.ID = "WHJX_" + ConvertData.ToHexString(b, 3, 6);  //设备ID
                int session = ToolAPI.ByteArrayToValueType.GetInt32_BigEndian(b,9);//session
                byte command = b[13];//命令字节
                short length = ToolAPI.ByteArrayToValueType.GetInt16_BigEndian(b, 14);//长度

                modelObject.Pm2_5 = ToolAPI.ByteArrayToValueType.GetInt32_BigEndian(b, 16);
                modelObject.Pm10 = ToolAPI.ByteArrayToValueType.GetInt32_BigEndian(b, 20);
                modelObject.Noise = ToolAPI.ByteArrayToValueType.GetInt32_BigEndian(b, 24)/10f;
                modelObject.Temperature = ToolAPI.ByteArrayToValueType.GetInt32_BigEndian(b, 28) / 10f;
                modelObject.Humidity = ToolAPI.ByteArrayToValueType.GetInt32_BigEndian(b, 32) / 10f;
                modelObject.WindDirection = WindDirectionChange(ToolAPI.ByteArrayToValueType.GetInt32_BigEndian(b, 36));
                modelObject.Wind = ToolAPI.ByteArrayToValueType.GetInt32_BigEndian(b, 40)/10f;
                modelObject.TSP = ToolAPI.ByteArrayToValueType.GetInt32_BigEndian(b, 44)/1000f;
                modelObject.Pressure = ToolAPI.ByteArrayToValueType.GetInt32_BigEndian(b, 48) / 1000f; 

                df.deviceid = modelObject.ID;//缺少ID
                df.datatype = "current";
                df.contentjson = JsonConvert.SerializeObject(modelObject);
                FogGun.Linkage_dust linkage_dust = new FogGun.Linkage_dust();
                linkage_dust.Equipment = modelObject.ID;
                linkage_dust.PM25 = modelObject.Pm2_5;
                linkage_dust.PM10 = modelObject.Pm10;
                FogGun.Linkage.Dust_data_Process(linkage_dust);
                TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;
                if (TcpExtendTemp.EquipmentID == null || TcpExtendTemp.EquipmentID.Equals(""))
                {
                    TcpExtendTemp.EquipmentID = df.deviceid;
                }
                //存入数据库
                if (df.contentjson != null && df.contentjson != "")
                {
                    DB_MysqlRaiseDustNoise.SaveRaiseDustNoise(df);
                }
                //}
                //ToolAPI.XMLOperation.WriteLogXmlNoTail("3", "3");
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail(System.Windows.Forms.Application.StartupPath + @"\LW", "瞭望设备实时数据解析异常：", ex.Message);
            }

        }
        public static void Unpack(byte[] b, int c, TcpSocketClient client)
        {
            //得到帧组集合
            string dataHexString = ConvertData.ToHexString(b, 0, c);
            string[] stringSeparators = new string[] { "FEDC" };
            //判断起始符+版本号进行分割包
            string[] DataHexAry = dataHexString.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < DataHexAry.Length; i++)
            {
                //转换为字节数组
                string frames = "FEDC" + DataHexAry[i];
                byte[] framesByte = ConvertData.HexToByte(frames);
                //进入对应的解析类
                OnResolveRecvMessage(framesByte, framesByte.Length, client);
            }
        }

        static float WindDirectionChange(int value)
        {
            switch(value)
            {
                case 0x000f: return 0;
                case 0x0001: return 45;
                case 0x0003: return 90;
                case 0x0005: return 135;
                case 0x0007: return 180;
                case 0x0009: return 225;
                case 0x000b: return 270;
                case 0x000d: return 315;
                case 0x0000: return 22.5f;
                case 0x0002: return 67.5f;
                case 0x0004: return 112.5f;
                case 0x0006: return 157.5f;
                case 0x0008: return 202.5f;
                case 0x000a: return 247.5f;
                case 0x000c: return 295.5f;
                case 0x000e: return 337.5f;
                default:return 0;
            }
        }
    }

}
