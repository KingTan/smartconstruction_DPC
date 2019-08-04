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
    public class ProtocolAnalysis_LW
    {
        //static bool istrue = true;
        //起始符+协议版本号+命令字+数据长度+数据载荷+CEC16
        //(字节)2+3+1+2+len+2+2 = 8(头)+LEn(数据载荷)+4(尾)
        //0103603F24A600429C00003EF09F4E3F448D733FC8E6DC3FB675453FEB6D173F800000428200000000000000000000424D999A4193333300000000000000000000000000000000000011061C0C2730313434302D303032382D73636C772D3330333900DEB4
        private static void OnResolveRecvMessage(byte[] b, int c, TcpSocketClient client)
        {
            try
            {
              
                if (b.Length >= 101)
                {
                    DBFrame df = new DBFrame();
                    df.contenthex = ConvertData.ToHexString(b, 0, c);
                    df.version = (client.External.External as TcpClientBindingExternalClass).TVersion;

                    Current_LW modelObject = new Current_LW();
                    //测试值 * 100
                    modelObject.Noise = HexStringToDouble.HexStringToDoubleFun(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", b[3], b[4], b[5], b[6])) * 100;
                    modelObject.Pm10 = HexStringToDouble.HexStringToDoubleFun(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", b[7], b[8], b[9], b[10]));
                    //(测试值 - 0.4) * 20;
                    modelObject.Wind = (float)((HexStringToDouble.HexStringToDoubleFun(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", b[11], b[12], b[13], b[14])) - 0.4d) * 20);
                    modelObject.WindDirection = HexStringToDouble.HexStringToDoubleFun(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", b[15], b[16], b[17], b[18]));
                    modelObject.WindDirection = WindDirectionTo(modelObject.WindDirection);
                    //(测试值 - 0.4) / 0.016 - 40;
                    modelObject.Temperature = (float)((HexStringToDouble.HexStringToDoubleFun(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", b[19], b[20], b[21], b[22])) - 0.4) / 0.016 - 40);
                    //(测试值 - 0.4) / 0.016;
                    modelObject.Humidity = (float)((HexStringToDouble.HexStringToDoubleFun(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", b[23], b[24], b[25], b[26])) - 0.4) / 0.016);
                    //测试值 / 2 * 80 + 30;
                    modelObject.Pressure = (float)(HexStringToDouble.HexStringToDoubleFun(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", b[27], b[28], b[29], b[30])) / 2 * 80 + 30);
                    modelObject.Voltage = HexStringToDouble.HexStringToDoubleFun(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", b[31], b[32], b[33], b[34]));
                    modelObject.Pm2_5 = HexStringToDouble.HexStringToDoubleFun(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", b[35], b[36], b[37], b[38]));
                    modelObject.Longitude = HexStringToDouble.HexStringToDoubleFun(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", b[39], b[40], b[41], b[42]));
                    modelObject.Latitude = HexStringToDouble.HexStringToDoubleFun(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", b[43], b[44], b[45], b[46]));
                    modelObject.BanTemperature = HexStringToDouble.HexStringToDoubleFun(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", b[47], b[48], b[49], b[50]));
                    modelObject.BanHumidity = HexStringToDouble.HexStringToDoubleFun(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", b[51], b[52], b[53], b[54]));
                    modelObject.TSP = HexStringToDouble.HexStringToDoubleFun(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", b[55], b[56], b[57], b[58]));
                    byte[] id = new byte[19];
                    Array.Copy(b, 79, id, 0, 19);
                    modelObject.ID = System.Text.Encoding.ASCII.GetString(id);

                    df.deviceid = modelObject.ID;
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
                }
            }
            catch(Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail(System.Windows.Forms.Application.StartupPath + @"\LW", "瞭望设备实时数据解析异常：", ex.Message);
            }

        }
        /// <summary>
        /// 风速转换
        /// </summary>
        /// <returns></returns>
        private static float WindDirectionTo(float testValue)
        {
            //"北风";  0 "    东风";  90 "     南风";   180 "   西风";   270 "      东北风";  45 "   东南风";   135 "   西南风";   225  "   西北风";   315
            float result = 0f;
            if (testValue <= 0.452)
                result = 0;
            else if (testValue <= 0.771 && testValue > 0.452)
                result = 45;
            else if (testValue <= 0.877 && testValue > 0.771)
                result = 90;
            else if (testValue <= 1.195 && testValue > 0.877)
                result = 135;
            else if (testValue <= 1.301 && testValue > 1.195)
                result = 180;
            else if (testValue <= 1.619 && testValue > 1.301)
                result = 225;
            else if (testValue <= 1.831 && testValue > 1.619)
                result = 270;
            else if (testValue > 1.831)
                result = 315;
            return result; 
        }

        public static void Unpack(byte[] b, int c, TcpSocketClient client)
        {
            //得到帧组集合
            string dataHexString = ConvertData.ToHexString(b, 0, c);
            string[] stringSeparators = new string[] { "010360" };
            //判断起始符+版本号进行分割包
            string[] DataHexAry = dataHexString.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < DataHexAry.Length; i++)
            {
                //转换为字节数组
                string frames = "010360" + DataHexAry[i];
                byte[] framesByte = ConvertData.HexToByte(frames);
                //进入对应的解析类
                OnResolveRecvMessage(framesByte, framesByte.Length, client);
            }
        }
    }

}
