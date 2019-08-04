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
    public class ProtocolAnalysis_ZKZQ
    {
        //FF AA 01 00 64 00 31 35 30 38 31 35 31 32 32 32 34 35 01 2D 32 2E 35 20 20 20 02 35 36 2E 35 20 20 20 03 31 30 31 2E 32 35 20 05 31 2E 35 20 20 20 20 25 31 35 30 20 20 20 20 14 35 35 20 20 20 20 20 15 38 35 20 20 20 20 20 17 37 35 2E 35 20 20 20 18 30 2E 35 20 20 20 20 23 31 32 31 2E 35 35 32 24 33 34 2E 31 32 33 31
        private static void OnResolveRecvMessage(byte[] b, int c, TcpSocketClient client)
        {
            try
            {
                //if (b.Length >= 106)
                //{
                DBFrame df = new DBFrame();
                df.contenthex = ConvertData.ToHexString(b, 0, c);
                df.version = (client.External.External as TcpClientBindingExternalClass).TVersion;

                Current_ZKZQ modelObject = new Current_ZKZQ();
                modelObject.ID = "ZKZQ_" + b[2].ToString();
                try
                {
                    for (int i = 0; i < 11; i++)
                    {
                        byte type = b[18 + i * 8];
                        byte[] tempBA = new byte[7];
                        Array.Copy(b, 19 + i * 8, tempBA, 0, 7);
                        float value = float.Parse(Encoding.ASCII.GetString(tempBA));
                        switch (type)
                        {
                            case 0x01: modelObject.Temperature = value; break;//温度  ℃
                            case 0x02: modelObject.Humidity = value; break;//湿度 %RH
                            case 0x03: modelObject.Pressure = value * 1000; break;//大气压 kPa转成pa
                            case 0x05: modelObject.Wind = value; break;//风速 m/s
                            case 0x25: modelObject.WindDirection = value; break;//风向 °
                            case 0x14: modelObject.Pm2_5 = value; break;//PM2.5 ug/m3
                            case 0x15: modelObject.Pm10 = value; break;//PM10 ug/m3
                            case 0x17: modelObject.Noise = value; break;//噪音 dB
                            case 0x18: modelObject.TSP = value; break;//TSP mg/m3
                            case 0x23: modelObject.Longitude = value; break;//经度
                            case 0x24: modelObject.Latitude = value; break;//纬度
                        }
                    }
                }
                catch (Exception ex)
                {

                }

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

                if (!string.IsNullOrEmpty(MainStatic.DeviceCopy_RaiseDustNoise))
                {
                    if (MainStatic.DeviceCopy_RaiseDustNoise.Contains(df.deviceid + "#"))
                    {
                        try
                        {
                            string[] strary = MainStatic.DeviceCopy_RaiseDustNoise.Split(';');
                           
                            foreach (string dev in strary)
                            {
                                string[] devcopy = dev.Split('#');
                                if (devcopy[0].Equals(df.deviceid))
                                {
                                    modelObject.ID = devcopy[1].ToString();
                                    DBFrame dfcopy = DBFrame.DeepCopy(df);
                                    dfcopy.deviceid = devcopy[1].ToString();
                                    dfcopy.datatype = "current";
                                    dfcopy.contentjson = JsonConvert.SerializeObject(modelObject);
                                    if (dfcopy.contentjson != null && dfcopy.contentjson != "")
                                    {
                                        DB_MysqlRaiseDustNoise.SaveRaiseDustNoise(dfcopy);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }

                    }
                }
                //}
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
            string[] stringSeparators = new string[] { "FFAA" };
            //判断起始符+版本号进行分割包
            string[] DataHexAry = dataHexString.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < DataHexAry.Length; i++)
            {
                //转换为字节数组
                string frames = "FFAA" + DataHexAry[i];
                byte[] framesByte = ConvertData.HexToByte(frames);
                //进入对应的解析类
                OnResolveRecvMessage(framesByte, framesByte.Length, client);
            }
        }
    }

}
