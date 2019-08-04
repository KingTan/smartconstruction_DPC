using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Threading;
using TCPAPI;
using Architecture;
using ToolAPI;
using ProtocolAnalysis.TowerCrane.OE;
using ProtocolAnalysis.Tool;
using ProtocolAnalysis.TowerCrane;
using Newtonsoft.Json;
using ProtocolAnalysis.IdentityVerification.model;
using ProtocolAnalysis.IdentityVerification;//存放对象的
namespace ProtocolAnalysis
{
    public static class GprsResolve_IdentityVerification
    {
        #region 解析入口
        /// <summary>
        /// 解析、存储、显示数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static string OnResolveRecvMessage(byte[] b, int c, TcpSocketClient client)
        {
            try
            {
                if (b.Length < 14)
                    return "";
                if (b[0] != 0x7E || b[1] != 0x01)
                    return "";
                DBFrame df = new DBFrame();
                df.contenthex = ConvertData.ToHexString(b, 0, c);
                df.version = (client.External.External as TcpClientBindingExternalClass).TVersion;
                //ToolAPI.XMLOperation.WriteLogXmlNoTail(System.Windows.Forms.Application.StartupPath + @"\Identify", "设备数据原包", df.contenthex);
                byte[] sn = new byte[8];   //设备编号
                Array.Copy(b, 5, sn, 0, 8);
                TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;
                TcpExtendTemp.EquipmentID = Encoding.ASCII.GetString(sn);
                df.deviceid = TcpExtendTemp.EquipmentID;

                byte typ = b[13];
                if (typ == 0x00)//心跳
                {
                    byte[] rb = OnResolveHeabert(b, c, ref df);
                    if (rb != null)
                        client.SendBuffer(rb);
                }
                else if (typ == 0x01) //实时数据
                {
                    byte[] rb = OnResolveCurrent(b, c, ref df);
                    //if (rb != null)
                        //client.SendBuffer(rb);
                }
                else if (typ == 0x02)//身份验证
                {
                    byte[] rb = OnResolveIdentityVerification(b, c, ref df, client);
                    if (rb != null)
                        client.SendBuffer(rb);
                }
                else if (typ == 0x03)//命令下发
                {
                    byte[] rb = OnResolveOrderissued(b, c, ref df);
                    if (rb != null)
                        client.SendBuffer(rb);
                }
                else if (typ == 0x04) //信息上传
                {
                    byte[] rb = OnResolveUploadInfo(b, c, ref df);
                    if (rb != null)
                        client.SendBuffer(rb);
                }
                if (TcpExtendTemp.EquipmentID == null || TcpExtendTemp.EquipmentID.Equals(""))
                {
                    TcpExtendTemp.EquipmentID = df.deviceid;
                }
                return "";
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        #endregion

        #region 具体命令解析
        /// <summary>
        /// 实时数据
        /// </summary>
        /// <param name="b"></param>
        /// <param name="bCount"></param>
        /// <param name="df"></param>
        /// <returns></returns>
        private static byte[] OnResolveCurrent(byte[] b, int bCount, ref DBFrame df)
        {
            try
            {
                Current_IdentityVerification data = new Current_IdentityVerification();
                data.Equipment = df.deviceid;
                try
                {
                    string tStr = ConvertData.ToHexString(b, 16, 6);
                    data.RTC = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
                }
                catch (Exception ex)
                {
                    data.RTC = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
                UShortValue s = new UShortValue();
                byte[] sn = new byte[18];   //身份证
                Array.Copy(b, 22, sn, 0, 18);
                data.ID = Encoding.ASCII.GetString(sn).Replace("\0", "");
                data.InterId = b[40].ToString();
                data.Nc = b[41].ToString();
                s.bValue1 = b[42];
                s.bValue2 = b[43];
                data.SensorSet = Convert.ToString(s.sValue, 2).PadLeft(16, '0');
                s.bValue1 = b[44];
                s.bValue2 = b[45];
                data.Height = s.sValue.ToString();
                byte[] rb = Byte_Current(data.Equipment, data.RTC);
                DB_MysqlIdentityVerification.SaveCurrent(data);
                return rb;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 信息上传
        /// </summary>
        /// <param name="b"></param>
        /// <param name="bCount"></param>
        /// <param name="df"></param>
        /// <returns></returns>
        private static byte[] OnResolveUploadInfo(byte[] b, int bCount, ref DBFrame df)
        {
            information_IdentityVerification data = new information_IdentityVerification();
            data.Equipment = df.deviceid;
            if (b[16] == 0x00)//命令字是0x00的
            {
                data.dataType = b[17].ToString();
                try
                {
                    string tStr = ConvertData.ToHexString(b, 18, 6);
                    data.RTC = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
                }
                catch (Exception ex)
                {
                    data.RTC = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
                UShortValue s = new UShortValue();
                byte[] sn = new byte[20];   //软件版本
                Array.Copy(b, 24, sn, 0, 20);
                data.SoftVersion = Encoding.ASCII.GetString(sn).Replace("\0", "");
                data.IdentifyWay = b[44].ToString();
                data.IdentifyClcle = b[45].ToString();
                s.bValue1 = b[46];
                s.bValue2 = b[47];
                data.HeightMin = s.sValue.ToString();
                s.bValue1 = b[48];
                s.bValue2 = b[49];
                data.HeightMax = s.sValue.ToString();
                s.bValue1=b[50];
                s.bValue2 = b[51];
                data.HeightDistance = s.sValue.ToString();
                byte[] rb = Byte_Information(data.Equipment,b);
                DB_MysqlIdentityVerification.SaveInformation(data);
                return rb;
            }
            return null;
        }
        #region 心跳
        /// <summary>
        /// 心跳
        /// </summary>
        /// <param name="b"></param>
        /// 发送：7e 01 00 00 00 31 32 33 34 35 36 37 38 00 18 00 18 06 22 17 21 10 31 32 33 34 35 36 37 38 31 32 33 34 35 36 37 38 31 32 32 37 7D 7D
        /// 应答：7e 01 00 00 00 31 32 33 34 35 36 37 38 00 07 00 01 18 06 22 17 39 20 a3 00 7d 7d 
        private static byte[] OnResolveHeabert(byte[] b, int bCount, ref DBFrame df)
        {
            Heartbeat_IdentityVerification data = new Heartbeat_IdentityVerification();
            data.Equipment = df.deviceid;
            //时间
            try
            {
                string tStr = ConvertData.ToHexString(b, 16, 6);
                data.RTC = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                data.RTC = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            byte[] sn = new byte[18];   //身份證
            Array.Copy(b, 22, sn, 0, 18);
            data.Identity_card = Encoding.ASCII.GetString(sn).Replace("\0", "");
            //拼接应答
            byte[] rb = new byte[27];
            DateTime dt = DateTime.Now;
            rb[0] = 0x7E;
            rb[1] = 0x01;
            rb[2] = 0x00;
            rb[3] = 0x00; rb[4] = 0x00;//保留
            byte[] devTemp = Encoding.ASCII.GetBytes(data.Equipment);
            for (int i = 0; i < 8; i++)
            {
                rb[5 + i] = devTemp[i];
            }
            rb[13] = 0x00;//功能码
            rb[14] = 0x07;//应用数据区数据长度 
            rb[15] = 0x00;
            rb[16] = 0x01;//1：校准时间
            rb[17] = byte.Parse(dt.Year.ToString().Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            rb[18] = byte.Parse(dt.Month.ToString(), System.Globalization.NumberStyles.HexNumber);
            rb[19] = byte.Parse(dt.Day.ToString(), System.Globalization.NumberStyles.HexNumber);
            rb[20] = byte.Parse(dt.Hour.ToString(), System.Globalization.NumberStyles.HexNumber);
            rb[21] = byte.Parse(dt.Minute.ToString(), System.Globalization.NumberStyles.HexNumber);
            rb[22] = byte.Parse(dt.Second.ToString(), System.Globalization.NumberStyles.HexNumber);
            byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(rb, 2, 21));
            rb[23] = crc16[0];
            rb[24] = crc16[1];
            rb[25] = 0x7D;
            rb[26] = 0x7D;
            //存入数据库
            DB_MysqlIdentityVerification.SaveHeartbeat(data);
            return rb;
        }
        #endregion

        #region 身份验证
        /// <summary>
        /// 身份验证
        /// </summary>
        /// <param name="b"></param>
        /// 发送：命令字0  7e 01 00 00 00 31 32 33 34 35 36 37 38 02 1A 00 00 18 06 25 08 48 10 31 32 33 34 35 36 37 38 31 32 33 34 35 36 37 38 31 32 00 0A 12 7D 7D
        /// 应答：命令字0  7e 01 00 00 00 31 32 33 34 35 36 37 38 02 02 00 00 01 a2 38 7d 7d
        /// 发送：命令字1  7e 01 00 00 00 31 32 33 34 35 36 37 38 02 27 00 01 03 01 00 31 32 33 34 35 36 37 38 31 32 33 34 35 36 37 38 31 32 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 DF EC 7D 7D  第一个包
        ///       应答：   7e 01 00 00 00 31 32 33 34 35 36 37 38 02 03 01 01 24 f9 7d 7d
        ///                7e 01 00 00 00 31 32 33 34 35 36 37 38 02 05 00 01 03 02 00 00 05 27 7D 7D  第二个包
        ///       应答：   7e 01 00 00 00 31 32 33 34 35 36 37 38 02 03 02 01 77 ac 7d 7d
        ///                7e 01 00 00 00 31 32 33 34 35 36 37 38 02 05 00 01 03 03 00 00 35 10 7D 7D  第三个包
        ///       应答：   7e 01 00 00 00 31 32 33 34 35 36 37 38 02 03 03 01 46 9f 7d 7d
        /// 发送：命令字2  7e 01 00 00 00 31 32 33 34 35 36 37 38 02 04 00 02 03 01 01 6D 34 7D 7D 第一个包
        ///                7e 01 00 00 00 31 32 33 34 35 36 37 38 02 04 00 02 03 02 01 3E 61 7D 7D 第二个包
        ///                7e 01 00 00 00 31 32 33 34 35 36 37 38 02 04 00 02 03 03 01 0F 52 7D 7D 第三个包
        /// 发送：命令字3  7e 01 00 00 00 31 32 33 34 35 36 37 38 02 14 00 03 31 32 33 34 35 36 37 38 31 32 33 34 35 36 37 38 31 32 01 71 D5 7D 7D  删除成功
        ///                7e 01 00 00 00 31 32 33 34 35 36 37 38 02 14 00 03 31 32 33 34 35 36 37 38 31 32 33 34 35 36 37 38 31 32 00 65 A2 7D 7D  删除失败
        private static byte[] OnResolveIdentityVerification(byte[] b, int bCount, ref DBFrame df, TcpSocketClient client)
        {
            IdentityVerificationC data = new IdentityVerificationC();
            data.Equipment = df.deviceid;
            data.Order_flag = b[16].ToString();
            List<byte> rb = new List<byte>();
            byte[] rbtemp = new byte[14];   //设备编号
            Array.Copy(b, 0, rbtemp, 0, 14);
            rb.AddRange(rbtemp);
            if (data.Order_flag == "0")//登录身份信息
            {

                byte[] sn = new byte[18];   //设备编号
                Array.Copy(b, 17, sn, 0, 18);
                data.Identity_card = Encoding.ASCII.GetString(sn).Replace("\0", "");

                try
                {
                    string tStr = ConvertData.ToHexString(b, 35, 6);
                    data.RTC = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
                }
                catch (Exception ex)
                {

                    data.RTC = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }

                data.Status = b[41].ToString();
                //存入数据库
                DB_MysqlIdentityVerification.SaveIdentityVerification(data);
                //配置应答
                rb.Add(0x02); rb.Add(0x00); rb.Add(b[16]); rb.Add(0x01);
                return PackageFrameTail(rb);
            }
            else if (data.Order_flag == "1")
            {
                data.Package_tote = b[17].ToString();
                data.Package_current = b[18].ToString();
                data.Characteristic_type = b[19].ToString();
                //特征数据
                data.Characteristic_data = new byte[bCount - 24];//只有特征数据
                Array.Copy(b, 20, data.Characteristic_data, 0, bCount - 24);
                //if(data.Package_current=="1")
                //{
                byte[] sn = new byte[18];   //身份证号
                Array.Copy(data.Characteristic_data, 0, sn, 0, 18);
                data.Identity_card = Encoding.ASCII.GetString(sn).Replace("\0", "");

               
                byte[] na = new byte[16];
                Array.Copy(data.Characteristic_data, 18, na, 0, 16);
                data.Name = Encoding.Unicode.GetString(na);

               

                byte[] i = new byte[data.Characteristic_data.Length - 34];//虹膜
                Array.Copy(data.Characteristic_data, 34, i, 0, i.Length);
                data.Iris = i;
                //}


                //存入数据库
                DB_MysqlIdentityVerification.SaveIdentityVerification(data);
                rb.Add(4); rb.Add(0x00);//长度
                rb.Add(b[16]); rb.Add(b[17]); rb.Add(b[18]); rb.Add(0x01);
                return PackageFrameTail(rb);
            }
            else if (data.Order_flag == "2")
            {
                data.Package_tote = b[17].ToString();
                data.Package_current = b[18].ToString();
                data.Result = b[19].ToString();
                //存入数据库
                data.Identity_card = CommandIssued_IdentityVerification.IrisissuedDic[data.Equipment]["identity_card"].ToString();
                if (data.Package_current == "1")
                {
                    byte[] message = GprsResolve_IdentityVerification.Byte_Iris("2", CommandIssued_IdentityVerification.IrisissuedDic[data.Equipment]);//得到拼接包
                    if (message != null)
                    {
                        client.SendBuffer(message);
                    }
                }
                else
                {
                    try
                    {
                        CommandIssued_IdentityVerification.IrisissuedDic.Remove(data.Equipment);
                    }
                    catch (Exception ex) { }
                }
                DB_MysqlIdentityVerification.SaveIdentityVerification(data);

            }
            else if(data.Order_flag=="3")
            {

                byte[] sn = new byte[18];   //身份证号
                Array.Copy(b, 17, sn, 0, 18);
                data.Identity_card = Encoding.ASCII.GetString(sn).Replace("\0", "");

                data.Result = b[35].ToString();
                //存入数据库
                DB_MysqlIdentityVerification.SaveIdentityVerification(data);
            }
            //7e 01 00 00 00 31 32 33 34 35 36 37 38 02 14 00 04 31 32 33 34 35 36 37 38 31 32 33 34 35 36 37 38 31 32 01 68 5D 7D 7D
            //应答：7E 01 00 00 00 31 32 33 34 35 36 37 38 02 23 00 31 32 33 34 35 36 37 38 31 32 33 34 35 36 37 38 31 32 01 D5 C5 C8 FD 00 00 00 00 00 00 00 00 00 00 00 00 14 4F 7D 7D
            else if (data.Order_flag == "4") //新增命令字4
            {
                byte[] sn = new byte[18];   //身份证号
                Array.Copy(b, 17, sn, 0, 18);
                data.Identity_card = Encoding.ASCII.GetString(sn).Replace("\0", "");
                string name = DB_MysqlIdentityVerification.GetNameToId(data.Identity_card);
                rb.Add(0x24); rb.Add(0x00);//长度
                rb.Add(0x04);
                for (int i = 17; i < 36; i++)
                {
                    rb.Add(b[i]);
                }
                byte[] na = Encoding.GetEncoding("GBK").GetBytes(name);
                int j =0;// na.Length;
                for (int i = 0; i < 16; i++)
                {
                    if (j < na.Length)
                        rb.Add(na[j]);
                    else
                        rb.Add(0x00);
                    j++;
                }
                byte[] sendRev = PackageFrameTail(rb);
                return sendRev;
            }
            return null;
        }
        /// <summary>
        /// 拼接下发特征码
        /// </summary>
        /// <param name="flag"></param>
        /// <param name="dr"></param>
        /// <returns></returns>
        /// 第一包：7e 01 00 00 00 31 32 33 34 35 36 37 38 02 27 00 02 03 01 00 31 32 33 34 35 36 37 38 31 32 33 34 35 36 37 38 31 32 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 21 87 7d 7d
        /// 第二包：7e 01 00 00 00 31 32 33 34 35 36 37 38 02 05 00 02 03 02 00 00 d7 c9 7d 7d
        /// 第三包：7e 01 00 00 00 31 32 33 34 35 36 37 38 02 05 00 02 03 03 00 00 e7 fe 7d 7d
        public static byte[] Byte_Iris(string flag, DataRow dr)
        {
            try
            {
                //设备编号
                string equipment = dr["equipment"].ToString().Trim();
                string Characteristic_data = "";
                if (flag == "1")
                    Characteristic_data = dr["characteristic_data0"].ToString().Trim();
                else if (flag == "2")
                    Characteristic_data = dr["characteristic_data1"].ToString().Trim();
                else
                    Characteristic_data = dr["characteristic_data2"].ToString().Trim();
                byte[] framesByte = ConvertData.HexToByte(Characteristic_data);

                List<byte> rb = new List<byte>();
                //头
                rb.Add(0x7E); rb.Add(0x01);
                //版本号
                rb.Add(0x00);
                //保留
                rb.Add(0x00); rb.Add(0x00);
                //设备编号
                rb.AddRange(Encoding.ASCII.GetBytes(equipment));
                //功能码
                rb.Add(0x02);
                //数据长度
                short count = (short)(framesByte.Length + 4);
                byte[] lengthTemp = ToolAPI.ValueTypeToByteArray.GetBytes_LittleEndian(count);
                rb.AddRange(lengthTemp);//这个长度最后确定
                //数据区
                //命令字
                rb.Add(0x02);
                //总包数
                rb.Add(0x02);
                //当前包 
                rb.Add(byte.Parse(flag));
                //特征类型
                rb.Add(0x00);
                //特征库
                rb.AddRange(framesByte);
                byte[] sendRev = PackageFrameTail(rb);
                return sendRev;
            }
            catch (Exception)
            {
                return null;
            }

        }
        /// <summary>
        /// 获取实时数据的应答
        /// </summary>
        /// <param name="equipment"></param>
        /// <param name="rtc"></param>
        /// <returns></returns>
        public static byte[] Byte_Current(string equipment,string rtc)
        {
            try
            {
                byte[] rb = new byte[26];
                rb[0] = 0x7E;
                rb[1] = 0x01;
                rb[2] = 0x00;
                rb[3] = 0x00; rb[4] = 0x00;//保留
                byte[] devTemp = Encoding.ASCII.GetBytes(equipment);
                for (int i = 0; i < 8; i++)
                {
                    rb[5 + i] = devTemp[i];
                }
                rb[13] = 0x01;//功能码
                rb[14] = 0x06;
                rb[15] = 0x00; //长度
                DateTime dt = DateTime.Parse(rtc);
                rb[16] = byte.Parse(dt.Year.ToString().Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                rb[17] = byte.Parse(dt.Month.ToString(), System.Globalization.NumberStyles.HexNumber);
                rb[18] = byte.Parse(dt.Day.ToString(), System.Globalization.NumberStyles.HexNumber);
                rb[19] = byte.Parse(dt.Hour.ToString(), System.Globalization.NumberStyles.HexNumber);
                rb[20] = byte.Parse(dt.Minute.ToString(), System.Globalization.NumberStyles.HexNumber);
                rb[21] = byte.Parse(dt.Second.ToString(), System.Globalization.NumberStyles.HexNumber);
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(rb, 2, 20));
                rb[22] = crc16[0];
                rb[23] = crc16[1];
                rb[24] = 0x7D;
                rb[25] = 0x7D;
                return rb;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 信息上传应答
        /// </summary>
        /// <param name="equipment"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static byte[] Byte_Information(string equipment,byte[] b)
        {
            try
            {
                byte[] rb = new byte[21];
                rb[0] = 0x7E;
                rb[1] = 0x01;
                rb[2] = 0x00;
                rb[3] = 0x00; rb[4] = 0x00;//保留
                byte[] devTemp = Encoding.ASCII.GetBytes(equipment);
                for (int i = 0; i < 8; i++)
                {
                    rb[5 + i] = devTemp[i];
                }
                rb[13] = 0x04;//功能码
                rb[14] = 0x01;
                rb[15] = 0x00; //长度
                rb[16] = b[16];
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(rb, 2, 15));
                rb[17] = crc16[0];
                rb[18] = crc16[1];
                rb[19] = 0x7D;
                rb[20] = 0x7D;
                return rb;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 拼接删除一个用户虹膜
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        /// 7e 01 00 00 00 31 32 33 34 35 36 37 38 02 13 00 03 31 32 33 34 35 36 37 38 31 32 33 34 35 36 37 38 31 32 4f 5c 7d 7d
        public static byte[] Byte_Irisdelete(DataRow dr)
        {
            try
            {
                //设备编号
                string equipment = dr["equipment"].ToString().Trim();
                string identity_card = dr["identity_card"].ToString().Trim();

                List<byte> rb = new List<byte>();
                //头
                rb.Add(0x7E); rb.Add(0x01);
                //版本号
                rb.Add(0x00);
                //保留
                rb.Add(0x00); rb.Add(0x00);
                //设备编号
                rb.AddRange(Encoding.ASCII.GetBytes(equipment));
                //功能码
                rb.Add(0x02);
                //数据长度
                rb.Add(19); rb.Add(0x00);//这个长度最后确定
                //数据区
                //命令字
                rb.Add(0x03);
                //ID
                rb.AddRange(Encoding.ASCII.GetBytes(identity_card));
                byte[] sendRev = PackageFrameTail(rb);
                return sendRev;
            }
            catch (Exception)
            {
                return null;
            }

        }
        #endregion

        #region 命令下发
        /// <summary>
        /// 命令下发
        /// </summary>
        /// <param name="b"></param>
        /// 设备的首次应答 7e 01 00 00 00 31 32 33 34 35 36 37 38 03 1e 00 00 00 0d 31 39 32 2e 31 36 38 2e 36 36 2e 33 38 00 00 00 00 00 00 00 00 04 35 30 30 30 00 4e 99 7d 7d
        /// 设备的再次确认 7e 01 00 00 00 31 32 33 34 35 36 37 38 03 1e 00 01 01 0d 31 39 32 2e 31 36 38 2e 36 36 2e 33 38 00 00 00 00 00 00 00 00 04 35 30 30 30 00 51 90 7d 7d
        private static byte[] OnResolveOrderissued(byte[] b, int bCount, ref DBFrame df)
        {
            Orderissued_IdentityVerification data = new Orderissued_IdentityVerification();
            data.Equipment = df.deviceid;
            data.Confirm_flag = b[16].ToString();
            data.Order_flag = b[17].ToString();
            byte[] ip = new byte[b[18]];   //ip
            Array.Copy(b, 19, ip, 0, b[18]);
            data.IP_DNS = Encoding.ASCII.GetString(ip);
            byte[] port = new byte[b[40]];   //端口
            Array.Copy(b, 41, port, 0, b[40]);
            data.Port = Encoding.ASCII.GetString(port);

            //存入数据库
            DB_MysqlIdentityVerification.SaveOrder(data);
            return null;
        }
        /// <summary>
        /// 拼接下发ip
        /// </summary>
        /// <param name="dr"></param>
        /// <returns></returns>
        /// ip下发通知包 7e 01 00 00 00 31 32 33 34 35 36 37 38 03 1e 00 00 00 0d 31 39 32 2e 31 36 38 2e 36 36 2e 33 38 00 00 00 00 00 00 00 00 04 35 30 30 30 00 4e 99 7d 7d
        /// ip下发确认包 7e 01 00 00 00 31 32 33 34 35 36 37 38 03 1e 00 01 00 0d 31 39 32 2e 31 36 38 2e 36 36 2e 33 38 00 00 00 00 00 00 00 00 04 35 30 30 30 00 51 90 7d 7d
        public static byte[] Byte_IP(DataRow dr)
        {
            try
            {
                //设备编号
                string equipment = dr["equipment"].ToString().Trim();
                //ip
                string ip = dr["equipment_addr"].ToString().Trim();
                //端口
                string port = dr["equipment_port"].ToString().Trim();
                //下发和确认
                string confirm_flag = dr["addr_confirm_flag"].ToString().Trim();

                List<byte> rb = new List<byte>();
                //头
                rb.Add(0x7E); rb.Add(0x01);
                //版本号
                rb.Add(0x00);
                //保留
                rb.Add(0x00); rb.Add(0x00);
                //设备编号
                rb.AddRange(Encoding.ASCII.GetBytes(equipment));
                //功能码
                rb.Add(0x03);
                //数据长度
                rb.Add(30); rb.Add(0x00);
                //数据区
                //确认标识
                rb.Add(byte.Parse(confirm_flag));
                //命令字
                rb.Add(0x00);
                rb.Add((byte)ip.Length);//ip长度
                byte[] byteArray = System.Text.Encoding.Default.GetBytes(ip);
                rb.AddRange(byteArray);//ip得ASCII码
                for (int i = 0; i < 21 - byteArray.Length; i++)
                    rb.Add(0x00);
                rb.Add((byte)port.Length);//port长度
                byteArray = System.Text.Encoding.Default.GetBytes(port);
                rb.AddRange(byteArray);//port得ASCII码
                for (int i = 0; i < 5 - byteArray.Length; i++)
                    rb.Add(0x00);
                byte[] sendRev = PackageFrameTail(rb);
                return sendRev;
            }
            catch (Exception)
            {
                return null;
            }

        }
        #endregion

        #endregion

        #region
        /// <summary>
        /// 封装帧尾
        /// </summary>
        /// <param name="byteTemp"></param>
        /// <returns></returns>
        public static byte[] PackageFrameTail(List<byte> rb)
        {
            //校验和
            byte[] byteTemp = new byte[rb.Count];
            rb.CopyTo(byteTemp);
            byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(byteTemp, 2, rb.Count - 2));
            rb.Add(crc16[0]);
            rb.Add(crc16[1]);
            //结束符
            rb.Add(0x7D);
            rb.Add(0x7D);
            byteTemp = new byte[rb.Count];
            rb.CopyTo(byteTemp);
            return byteTemp;
        }
        #endregion
    }

}
