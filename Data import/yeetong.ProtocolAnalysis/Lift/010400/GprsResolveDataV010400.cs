using Architecture;
using ProtocolAnalysis.Lift;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TCPAPI;
using ToolAPI;

namespace ProtocolAnalysis
{
    public class GprsResolveDataV010400
    {
        /// <summary>
        /// 解析、存储、显示数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static string OnResolveRecvMessage(byte[] b, int c, TcpSocketClient client)
        {
            if (b.Length < 6)
                return "";
            DBFrame df = new DBFrame();
            df.contenthex = ConvertData.ToHexString(b, 0, c);
            df.version =(client.External.External as TcpClientBindingExternalClass).TVersion;
            TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;

            byte typ = b[5];
            //心跳
            if (typ == 0x00)
            {
                byte[] rb = OnResolveHeabert(b, c, client, ref df);
                if (rb != null)
                    client.SendBuffer(rb);
            }
            //实时数据 
            if (typ == 0x01)
            {
                OnResolveRealData(b, c, ref df);
            }
            //身份验证
            if (typ == 0x02)
            {
                AuthenticationDispose(b, c, client);
            }
            //离线数据
            if (typ == 0x03)
            {
                OnResolveRealData(b, c, ref df);
                //离线数据应答
                byte[] rb = new byte[12];
                //针头
                rb[0] = 0x7A;
                rb[1] = 0x7A;
                //协议版本号
                rb[2] = 0x01;
                rb[3] = 0x04;
                rb[4] = 0x00;
                //命令字
                rb[5] = 0x03;
                //数据长度
                rb[6] = 0x00;
                rb[7] = 0x00;
                //数据区
                rb[8] = 0x00;
                //校验和
                rb[9] = 0x00;
                rb[10] = 0x00;
                //结束
                rb[11] = 0x7B;
                rb[12] = 0x7B;
                client.SendBuffer(rb);
            }
            //参数上传（一样）
            if (typ == 0x04)
            {
                byte[] rb = OnResolveParamDataUpload(b, c, ref df);
                if (rb != null)
                    client.SendBuffer(rb);
            }
            /*需要重做*/
            if (typ == 0x05) //IP地址配置
            {
                try
                {
                    DB_MysqlLift.UpdateDataConfig(TcpExtendTemp.EquipmentID,2,true);
                    DB_MysqlLift.UpdateIPCommandIssued(TcpExtendTemp.EquipmentID, 2);

                }
                catch (Exception ex)
                {
                    ToolAPI.XMLOperation.WriteLogXmlNoTail("GprsResolveDataV010206IP写入数据库标识异常", ex.Message);
                }
            }
            /*少个继电器状态*/
            if (typ == 0x06) //限位控制信息
            {
                try
                {
                    DB_MysqlLift.UpdatecontrolDataCongfig(TcpExtendTemp.EquipmentID);
                }
                catch (Exception ex)
                {
                    ToolAPI.XMLOperation.WriteLogXmlNoTail("GprsResolveDataV010206限位控制写入数据库标识异常", ex.Message);
                }
            }
            /*重新整理*/
            if (typ == 0x07) // 设备运行时间
            {
                byte[] rb = OnResolveRunTime(b, c, ref df);
                if (rb != null)
                    client.SendBuffer(rb);

            }
            /*再检查一下*/
            if (typ == 0x08) //时间校准
            {
                byte[] rb = OnResolveTime(b, c);
                if (rb != null)
                    client.SendBuffer(rb);
            }
            if (typ == 0x09) //违规运行通知
            {
                OnViolation(b, c, client, ref df);
            }

            if (TcpExtendTemp.EquipmentID == null || TcpExtendTemp.EquipmentID.Equals(""))
            {
                TcpExtendTemp.EquipmentID = df.deviceid;
            }
            //存入数据库
            if (df.contentjson != null && df.contentjson != "")
            {
                DB_MysqlLift.SaveLift(df);
            }
            return null;
        }

        #region 心跳
        /// <summary>
        /// 心跳接收数据
        /// </summary>
        /// <param name="b"></param>
        public static byte[] OnResolveHeabert(byte[] b, int bCount, TcpSocketClient client, ref DBFrame df)//1.2.1
        {

            if (bCount != 0x1A)
                return null;
            if (BitConverter.ToUInt16(b, 22) != ConvertData.CRC16(b, 8, 14))//检验和
                return null;
            Lift_Heartbeat data = new Lift_Heartbeat();
            string tStr = ConvertData.ToHexString(b, 0, 2);//获取帧头
            if (tStr != "7A7A")
                return null;
            byte[] t = new byte[8];
            for (int i = 8, j = 0; i < 16; i++, j++)
            {
                t[j] = b[i];
            }
            data.SN = Encoding.ASCII.GetString(t);
            tStr = ConvertData.ToHexString(b, 16, 6);//获取时间
            DateTime getdate = new DateTime();
            byte[] bytes = new byte[19];
            try
            {
                getdate = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                bytes[0] = 0x7A;
                bytes[1] = 0x7A;
                //协议版本
                bytes[2] = 0x01;
                bytes[3] = 0x04;
                bytes[4] = 0x00;
                //命令字
                bytes[5] = 0x00;

                //数据长度
                bytes[6] = 0x07;
                bytes[7] = 0x00;
                //////////时间校准标示////
                bytes[8] = 0x01;

                DateTime nowTime = DateTime.Now;
                //时间
                bytes[9] = byte.Parse(nowTime.Year.ToString().Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                bytes[10] = byte.Parse(nowTime.Month.ToString(), System.Globalization.NumberStyles.HexNumber);
                bytes[11] = byte.Parse(nowTime.Day.ToString(), System.Globalization.NumberStyles.HexNumber);
                bytes[12] = byte.Parse(nowTime.Hour.ToString(), System.Globalization.NumberStyles.HexNumber);
                bytes[13] = byte.Parse(nowTime.Minute.ToString(), System.Globalization.NumberStyles.HexNumber);
                bytes[14] = byte.Parse(nowTime.Second.ToString(), System.Globalization.NumberStyles.HexNumber);
                //校验和
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(bytes, 8, 7));
                bytes[15] = crc16[0];//0x00;//算校验和
                bytes[16] = crc16[1];
                //结束
                bytes[17] = 0x7B;
                bytes[18] = 0x7B;

                //包装
                df.deviceid = data.SN;
                df.datatype = "heartbeat";
                df.contentjson = JsonConvert.SerializeObject(data);
                return bytes;
            }
            
            DateTime now = System.DateTime.Now;
            double compare = (now - getdate).TotalMinutes;
            
            if (compare > 1 || compare < 0)  //需要
            {
                bytes = new byte[19];
                //数据长度
                bytes[6] = 0x07;
                bytes[7] = 0x00;
                //////////时间校准标示////
                bytes[8] = 0x01;
                //时间
                bytes[9] = byte.Parse(now.Year.ToString().Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                bytes[10] = byte.Parse(now.Month.ToString(), System.Globalization.NumberStyles.HexNumber);
                bytes[11] = byte.Parse(now.Day.ToString(), System.Globalization.NumberStyles.HexNumber);
                bytes[12] = byte.Parse(now.Hour.ToString(), System.Globalization.NumberStyles.HexNumber);
                bytes[13] = byte.Parse(now.Minute.ToString(), System.Globalization.NumberStyles.HexNumber);
                bytes[14] = byte.Parse(now.Second.ToString(), System.Globalization.NumberStyles.HexNumber);
                //校验和
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(bytes, 8, 7));
                bytes[15] = crc16[0];//0x00;//算校验和
                bytes[16] = crc16[1];
                //结束
                bytes[17] = 0x7B;
                bytes[18] = 0x7B;
                data.OnlineTime = now.ToString("yyyy-MM-dd HH:mm:ss");

                ToolAPI.XMLOperation.WriteLogXmlNoTail("时间校验V1.4.0", "包内解析时间："+ getdate+"当前时间："+now);
            }
            else   //不需要
            {
                bytes = new byte[13];

                //长度
                bytes[6] = 0x01;
                bytes[7] = 0x00;
                //数据区
                bytes[8] = 0x00;
                //校验和
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(bytes, 8, 1));
                bytes[9] = crc16[0];//0x00;//算校验和
                bytes[10] = crc16[1];
                //结束
                bytes[11] = 0x7B;
                bytes[12] = 0x7B;
                data.OnlineTime = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
            }

            bytes[0] = 0x7A;
            bytes[1] = 0x7A;
            //协议版本
            bytes[2] = 0x01;
            bytes[3] = 0x04;
            bytes[4] = 0x00;
            //命令字
            bytes[5] = 0x00;


            //包装
            df.deviceid = data.SN;
            df.datatype = "heartbeat";
            df.contentjson = JsonConvert.SerializeObject(data);
            return bytes;
        }
        #endregion
        #region 实时数据
        /// <summary>
        /// 实时数据
        /// </summary>
        /// <param name="b"></param>
        /// 7A7A0103000144003132333435363738000000000000000000000000000102000841000000000000000000000000000000000F0000000000000000000000000000000000000000000000000006637B7B7A7A010300000E003132333435363738000102000841DC7F7B7B
        public static Byte[] OnResolveRealData(byte[] b, int bCount, ref DBFrame df)
        {
            string tStr = ConvertData.ToHexString(b, 0, 2);
            if (tStr != "7A7A")
                return null;
            Lift_Current data = new Lift_Current();
            //设备编号
            byte[] t = new byte[8];
            for (int i = 8, j = 0; i < 16; i++, j++)
            {
                t[j] = b[i];
            }
            data.SN = Encoding.ASCII.GetString(t);
            string str1 = b[19].ToString("X2") + b[18].ToString("X2") + b[17].ToString("X2") + b[16].ToString("X2");
            data.CycleId = int.Parse(str1, System.Globalization.NumberStyles.HexNumber).ToString();   //工作循环序列号
            //司机工号
            for (int i = 20, j = 0; i < 28; i++, j++)
            {
                t[j] = b[i];
            }
            data.CardNo = Encoding.ASCII.GetString(t);
            //日期
            tStr = ConvertData.ToHexString(b, 28, 6);
            try
            {
                data.Rtime = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");

            }
            catch
            {
                data.Rtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            data.Rtc = data.Rtime;  //RTC
            //重量 吨
            UShortValue s = new UShortValue();
            string str = b[35].ToString("X2") + b[34].ToString("X2");
            int result = int.Parse(str, System.Globalization.NumberStyles.HexNumber);
            data.Weight = (result / 1000.00).ToString("0.00");//设备发送单位：吨
            //当前额定载荷
            str = b[37].ToString("X2") + b[36].ToString("X2");
            result = int.Parse(str, System.Globalization.NumberStyles.HexNumber);
            data.RatedWeight = (result / 1000.00).ToString("0.00");//设备发送单位：吨
            //高度 m
            s.bValue1 = b[38];
            s.bValue2 = b[39];
            data.Height = (s.sValue / 10.00).ToString("0.00");//设备发送单位：分米
            //速度 m/秒
            s.bValue1 = b[40];
            s.bValue2 = b[41];
            data.Speed = (s.sValue / 10.00).ToString("0.00");//设备发送单位：分米//设备发送单位：分米/秒
            //楼层
            s.bValue1 = b[42];
            data.Floors = s.sValue.ToString("0");
            //人数
            s.bValue1 = b[43];
            data.personNum = s.sValue.ToString("0");
            //传感器状态
            s.bValue1 = b[44];
            s.bValue2 = b[45];
            data.SensorSet = Convert.ToString(s.sValue, 2).PadLeft(16, '0');
            //报警码
            s.bValue1 = b[46];
            s.bValue2 = b[47];
            //2字节： 0-重量预警 1-重量报警 2-顶层预警 3-顶层报警 4-蹲底 5-门打开 6-风速预警 7-风速报警 9-人数报警 10-防坠器报警
            //data.AlarmType = Convert.ToString(s.sValue, 2).PadLeft(16, '0');//2字节：0-顶层预警 1-底层预警 2-顶层报警 3-底层报警 4-重量预警 5-重量报警 6-偏载 7-风速预警 8-风速报警 9-门未关好
            string alarm = Convert.ToString(s.sValue, 2).PadLeft(16, '0');
            data.AlarmType = "00000000000000" + alarm.Substring(alarm.Length - 2, 2);
            try
            {
                data.AlarmCode_ZH = data.AlarmType[data.AlarmType.Length - 3].ToString() + data.AlarmType[data.AlarmType.Length - 4].ToString() + data.AlarmType[data.AlarmType.Length - 5].ToString();
            }
            catch (Exception) { }

            data.Type = (data.AlarmType.IndexOf("1") > -1) ? "2" : "0";//报警标识
            //继电器状态
            s.bValue1 = b[48];
            data.PowerStatu = s.sValue.ToString("0");
            //GPRS信号强度
            data.GprsSignal = ((sbyte)b[49]).ToString();
            ///门状态
            data.CurScreen = (b[50]+Convert.ToByte(64)).ToString();//b[50].ToString();/// 加192 Convert.ToInt32("11000000",2)
            
            //风力等级
            data.WindLevel = b[51].ToString();
            //风速
            s.bValue1 = b[52];
            s.bValue2 = b[53];
            data.Wind = (s.sValue / 100.00).ToString("0.00");//设备发送单位：cm/s 除以100得出m/s
            //倾角X
            short X = Convert.ToInt16(("0x" + b[55].ToString("X2") + b[54].ToString("X2")), 16);
            data.AngleX = (X / 100.00).ToString("0.00");  //倾角X 协议要求除以100
            //倾角Y
            short Y = Convert.ToInt16(("0x" + b[57].ToString("X2") + b[56].ToString("X2")), 16);
            data.AngleY = (Y / 100.00).ToString("0.00");  //倾角Y 协议要求除以100
            tStr = ConvertData.ToHexString(b, 78, 2);
            //身份证号处理
            byte[] d = new byte[18];
            for (int i = 58, j = 0; i < 76; i++, j++)
            {
                d[j] = b[i];
            }
            data.CardId = Encoding.ASCII.GetString(d).Replace("\0","");
            if (tStr != "7B7B")
                return null;

            //包装
            df.deviceid = data.SN;
            df.datatype = "current";
            df.contentjson = JsonConvert.SerializeObject(data);
            return null;
        }
        #endregion
        #region 身份验证
        /// <summary>
        /// 身份验证
        /// </summary>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="client"></param>
        private static void AuthenticationDispose(byte[] b, int c, TcpSocketClient client)
        {
            List<byte> rb = new List<byte>();

            try
            {
                switch (b[17])
                {
                    case 1:
                        byte[] byteTemp = new byte[26];
                        Array.Copy(b, byteTemp, 26);
                        rb.AddRange(byteTemp);
                        rb.Add(1);
                        //长度
                        rb[6] = 19; rb[7] = 0;
                        break;
                    case 2:
                        byte[] byteTemp2 = new byte[20];
                        Array.Copy(b, byteTemp2, 20);
                        rb.AddRange(byteTemp2);
                        rb.Add(1);
                        //长度
                        rb[6] = 13; rb[7] = 0;
                        break;
                    case 6://ZT 20170322
                        byte[] byteTemp6 = new byte[18];
                        Array.Copy(b, byteTemp6, 18);
                        rb.AddRange(byteTemp6);
                        //长度
                        rb[6] = 10; rb[7] = 0;
                        break;
                    default: break;
                }
                byte[] byteT = new byte[rb.Count];
                rb.CopyTo(byteT);
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(byteT, 8, rb.Count - 8));
                rb.Add(crc16[0]);
                rb.Add(crc16[1]);
                //结束符
                rb.Add(0x7B);
                rb.Add(0x7B);
                byteT = new byte[rb.Count];
                rb.CopyTo(byteT);
                client.SendBuffer(byteT);
                if (b[17] == 2)
                {
                    rb.Clear();
                    byte[] byteTemp2 = new byte[18];
                    Array.Copy(b, byteTemp2, 18);
                    rb.AddRange(byteTemp2);
                    rb.Add(1);//验证结果
                    //子命令字
                    rb[17] = 3;//验证结果下发
                    //长度
                    rb[6] = 81; rb[7] = 0;


                    byte[] sn = new byte[8];   //司机卡号
                    for (int l = 20, j = 0; l < 28; l++, j++)
                    {
                        sn[j] = b[l];
                    }
                    //工号
                    rb.AddRange(sn);
                    //卡号
                    rb.AddRange(sn);

                    for (int i = 0; i < 54; i++) rb.Add(0xff);

                    byteT = new byte[rb.Count];
                    rb.CopyTo(byteT);
                    crc16 = BitConverter.GetBytes(ConvertData.CRC16(byteT, 8, rb.Count - 8));
                    rb.Add(crc16[0]);
                    rb.Add(crc16[1]);
                    //结束符
                    rb.Add(0x7B);
                    rb.Add(0x7B);
                    byteT = new byte[rb.Count];
                    rb.CopyTo(byteT);
                    client.SendBuffer(byteT);
                }
                //记录数据库
                Lift_Authentication AuthenticationTemp = AuthenticationAnalysis(b, c);
                DB_MysqlLift.Pro_Authentication(AuthenticationTemp);
            }
            catch (Exception) { }
        }
        /// <summary>
        /// 协议解析
        /// </summary>
        /// <returns></returns>
        private static Lift_Authentication AuthenticationAnalysis(byte[] b, int bCount)
        {
            try
            {
                #region 解析
                Lift_Authentication au = new Lift_Authentication();
                byte[] sn = new byte[8];   //设备编号
                for (int l = 8, j = 0; l < 16; l++, j++)
                {
                    sn[j] = b[l];
                }
                //设备号ASCII
                au.SN = Encoding.ASCII.GetString(sn);
                if (b[17] == 0 || b[17] == 1)//身份验证子命令+上下班状态子命令
                {
                    if (b[16] == 6)//人脸识别
                    {
                        au.isFace = true;
                        sn = new byte[8];   //司机卡号
                        for (int l = 18, j = 0; l < 26; l++, j++)
                        {
                            sn[j] = b[l];
                        }
                        //司机卡号ASCII
                        au.empNo = Encoding.ASCII.GetString(sn);
                        DataTable dt = DB_MysqlLift.GetDriverInfoByEmpNo(au.SN, au.empNo);  //获取司机相关信息
                        if (dt.Rows.Count > 0)
                        {
                            au.empNo = dt.Rows[0]["empNo"].ToString().Trim();  //工号
                            au.cardNo = dt.Rows[0]["cardNo"].ToString().Trim();    //卡号
                            au.code = dt.Rows[0]["code"].ToString().Trim();    //身份证号
                            au.telephone = dt.Rows[0]["tel"].ToString().Trim(); //电话
                            au.name = dt.Rows[0]["name"].ToString().Trim();   //姓名
                            au.job = dt.Rows[0]["cardType"].ToString();     //职位
                        }
                    }
                    else
                    {
                        au.isFace = false;
                        sn = new byte[8];   //司机卡号
                        for (int l = 18, j = 0; l < 26; l++, j++)
                        {
                            sn[j] = b[l];
                        }
                        //司机卡号ASCII
                        au.KardID = Encoding.ASCII.GetString(sn);
                        #region
                        DataTable dt = DB_MysqlLift.GetIdentifyInfo(au.SN, au.KardID);  //获取司机相关信息
                        if (dt.Rows.Count > 0)
                        {
                            au.empNo = dt.Rows[0]["empNo"].ToString().Trim();  //工号
                            au.cardNo = au.KardID;    //卡号
                            au.code = dt.Rows[0]["code"].ToString().Trim();    //身份证号
                            au.telephone = dt.Rows[0]["telephone"].ToString().Trim(); //电话
                            au.name = dt.Rows[0]["name"].ToString().Trim();   //姓名
                            au.job = dt.Rows[0]["job"].ToString();     //职位
                        }
                        #endregion
                        try
                        {
                            if (b[17] == 1)
                                au.Status = b[26];
                        }
                        catch (Exception)
                        { }
                    }
                }
                else if (b[17] == 2)
                {
                    if (b.Length >= 38)//获取身份证号
                    {
                        au.isFace = true;
                        byte[] IDCard = new byte[18];
                        for (int l = 20, j = 0; l < 38; l++, j++)
                        {
                            IDCard[j] = b[l];
                        }
                        au.code = Encoding.ASCII.GetString(IDCard);
                        //访问数据库
                        DataTable dt = DB_MysqlLift.GetDriverInfoByIDCard(au.SN, au.code);  //获取司机相关信息
                        if (dt.Rows.Count > 0)
                        {
                            au.empNo = dt.Rows[0]["empNo"].ToString().Trim();  //工号
                            au.cardNo = dt.Rows[0]["cardNo"].ToString().Trim();    //卡号
                            //au.code = dt.Rows[0]["code"].ToString().Trim();    //身份证号
                            au.telephone = dt.Rows[0]["tel"].ToString().Trim(); //电话
                            au.name = dt.Rows[0]["name"].ToString().Trim();   //姓名
                            au.job = dt.Rows[0]["cardType"].ToString();     //职位
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                return au;
                #endregion
            }
            catch (Exception)
            {
                return null;
            }
        }
        #endregion
        #region 参数上传
        /// <summary>
        /// 参数上传
        /// </summary>
        /// <param name="b"></param>
        /// <param name="c"></param>
        private static byte[] OnResolveParamDataUpload(byte[] b, int bCount, ref DBFrame df)
        {
            #region 解析
            string tStr = ConvertData.ToHexString(b, 0, 2);
            if (tStr != "7A7A")
                return null;
            Lift_param data = new Lift_param();

            //设备编号
            byte[] t = new byte[8];
            for (int i = 8, j = 0; i < 16; i++, j++)
            {
                t[j] = b[i];
            }
            data.SN = Encoding.ASCII.GetString(t);
            UShortValue s = new UShortValue();
            ShortValue sS = new ShortValue();

            //召唤时间
            tStr = ConvertData.ToHexString(b, 16, 6);
            try
            {
                data.PubTime = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch
            {
                data.PubTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }

            //时间
            data.ParamUpdateTime = data.PubTime;
            //功能配置
            s.bValue1 = b[22];
            s.bValue2 = b[23];
            string ss = Convert.ToString(s.sValue, 2).PadLeft(16, '0');
            data.FunctionConfig = Convert.ToString(s.sValue, 2).PadLeft(16, '0');

            //身份认证方式
            data.IdentificationWay = Convert.ToInt32(b[24]).ToString();
            //安装时间
            tStr = ConvertData.ToHexString(b, 25, 3);
            try
            {
                data.InstalTime = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch
            {
                data.InstalTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            //空钩采样值1 有符号
            sS.bValue1 = b[28];
            sS.bValue2 = b[29];
            data.EmptyhookAD1 = sS.sValue.ToString("0.00");
            //空钩采样值2
            sS.bValue1 = b[30];
            sS.bValue2 = b[31];
            data.EmptyhookAD2 = sS.sValue.ToString("0.00");
            // 标准重量AD采样值1
            sS.bValue1 = b[32];
            sS.bValue2 = b[33];
            data.StandardWeightAD1 = sS.sValue.ToString("0.00");
            //标准重物采样值2
            sS.bValue1 = b[34];
            sS.bValue2 = b[35];
            data.StandardWeightAD2 = sS.sValue.ToString("0.00");
            // 空钩采样值
            sS.bValue1 = b[36];
            sS.bValue2 = b[37];
            data.EmptyhookAD = sS.sValue.ToString("0.00");
            // 标准重量采样值
            s.bValue1 = b[38];
            s.bValue2 = b[39];
            data.StandardWeightAD = s.sValue.ToString("0.00");
            //标准重物重量
            s.bValue1 = b[40];
            s.bValue2 = b[41];
            data.StandardHeavyWeight = s.sValue.ToString("0.00");
            //额定载荷
            s.bValue1 = b[42];
            s.bValue2 = b[43];
            data.RatedLoad = s.sValue.ToString("0.00");
            //预警系数
            data.CvWeightAlert = Convert.ToInt32(b[44]).ToString();
            //报警系数
            data.CvWeightAlarm = Convert.ToInt32(b[45]).ToString();
            //时段 1 起始时间
            data.Period1StartTime = Convert.ToInt32(b[46]).ToString();
            //时段 1 结束时间
            data.Period1EndTime = Convert.ToInt32(b[47]).ToString();
            //时段 2 起始时间
            data.Period2StartTime = Convert.ToInt32(b[48]).ToString();

            //时段 2 结束时间
            data.Period2EndTime = Convert.ToInt32(b[49]).ToString();
            //时段 3 起始时间
            data.Period3StartTime = Convert.ToInt32(b[50]).ToString();
            //时段 3 结束时间
            data.Period3EndTime = Convert.ToInt32(b[51]).ToString();
            //时段 4 起始时间
            data.Period4StartTime = Convert.ToInt32(b[52]).ToString();
            //时段4 结束时间
            data.Period4EndTime = Convert.ToInt32(b[53]).ToString();
            //时段 1 额定载荷
            s.bValue1 = b[54];
            s.bValue2 = b[55];
            data.Period1RatedLoad = s.sValue.ToString("0.00");
            //时段 2 额定载荷
            s.bValue1 = b[56];
            s.bValue2 = b[57];
            data.Period2RatedLoad = s.sValue.ToString("0.00");
            //时段 3 额定载荷
            s.bValue1 = b[58];
            s.bValue2 = b[59];
            data.Period3RatedLoad = s.sValue.ToString("0.00");
            //时段 4 额定载荷
            s.bValue1 = b[60];
            s.bValue2 = b[61];
            data.Period4RatedLoad = s.sValue.ToString("0.00");
            IntValue iv = new IntValue();
            //1楼楼层采样值
            iv.bValue1 = b[62];
            iv.bValue2 = b[63];
            iv.bValue3 = b[64];
            iv.bValue4 = b[65];
            data.OneFloorAD = iv.iValue.ToString("0.00");
            //最高楼层采样值
            iv.bValue1 = b[66];
            iv.bValue2 = b[67];
            iv.bValue3 = b[68];
            iv.bValue4 = b[69];
            data.HighFloorAD = iv.iValue.ToString("0.00");
            // 层高1
            s.bValue1 = b[70];
            s.bValue2 = b[71];
            data.HeightF1 = s.sValue.ToString("0.00");
            // 层高2
            s.bValue1 = b[72];
            s.bValue2 = b[73];
            data.HeightF2 = s.sValue.ToString("0.00");
            // 层高3
            s.bValue1 = b[74];
            s.bValue2 = b[75];
            data.HeightF3 = s.sValue.ToString("0.00");
            // 层高4
            s.bValue1 = b[76];
            s.bValue2 = b[77];
            data.HeightF4 = s.sValue.ToString("0.00");
            //F1 楼值
            data.ValueF1 = Convert.ToInt32(b[78]).ToString();
            //F2 楼值
            data.ValueF2 = Convert.ToInt32(b[79]).ToString();
            //F3 楼值
            data.ValueF3 = Convert.ToInt32(b[80]).ToString();
            //F4 楼值
            data.ValueF4 = Convert.ToInt32(b[81]).ToString();
            //最大楼层
            data.TotalFloors = Convert.ToInt32(b[82]).ToString();
            //最大高度
            s.bValue1 = b[83];
            s.bValue2 = b[84];
            data.TotalHeight = (s.sValue / 10.0).ToString("0.00");
            //司机身份对比周期
            iv.bValue1 = b[85];
            iv.bValue2 = b[86];
            iv.bValue3 = b[87];
            iv.bValue4 = b[88];
            data.DriverContrastCycle = (iv.iValue / 3600).ToString("0.00");  //接受的秒除以3600得到时，平台以小时显示
            //监理身份对比周期
            iv.bValue1 = b[89];
            iv.bValue2 = b[90];
            iv.bValue3 = b[91];
            iv.bValue4 = b[92];
            data.SuperContrastCycle = (iv.iValue / 3600).ToString("0.00");  //接受的秒除以3600得到时，平台以小时显示
            //限载人数
            data.LimitPersonNum = Convert.ToInt32(b[93]).ToString();
            //纬度 100000
            iv.bValue1 = b[94];
            iv.bValue2 = b[95];
            iv.bValue3 = b[96];
            iv.bValue4 = b[97];
            data.TakeSampleValue2 = (iv.iValue / 100000.0).ToString("0.00");
            //北纬 or 南纬
            data.NorthOrSouthX = Encoding.ASCII.GetString(b, 98, 1);
            //经度
            iv.bValue1 = b[99];
            iv.bValue2 = b[100];
            iv.bValue3 = b[101];
            iv.bValue4 = b[102];
            data.TakeSampleValue2 = (iv.iValue / 100000.0).ToString("0.00");
            //东经 or 西经
            data.NorthOrSouthX = Encoding.ASCII.GetString(b, 103, 1);
            //风速预警
            data.WindAlert = Convert.ToInt32(b[104]).ToString();
            //风速报警
            data.WindAlarm = Convert.ToInt32(b[105]).ToString();
            //boot loader版本号
            string version = "V " + b[106].ToString() + "." + b[107].ToString() + "." + b[108].ToString();
            data.BootLoaderVersion = version;//ConvertData.ToHexString(b, 104, 3);
            //软件版本号
            data.SoftVersion = System.Text.Encoding.ASCII.GetString(b, 109, b[6] - 101);
            data.PubTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            #endregion

            //包装
            df.deviceid = data.SN;
            df.datatype = "parameterUpload";
            df.contentjson = JsonConvert.SerializeObject(data);

            //应答数据
            byte[] rb = new byte[13];
            //针头
            rb[0] = 0x7A;
            rb[1] = 0x7A;
            //协议版本号
            rb[2] = 0x01;
            rb[3] = 0x04;
            rb[4] = 0x00;
            //命令字
            rb[5] = 0x04;
            //数据长度
            rb[6] = 0x00;
            rb[7] = 0x00;
            //数据区
            rb[8] = 0x00;
            //校验和
            rb[9] = 0x00;
            rb[10] = 0x00;
            //结束
            rb[11] = 0x7B;
            rb[12] = 0x7B;
            return rb;
        }
        #endregion
        #region 设备运行时间
        /// <summary>
        /// 设备运行时间
        /// </summary>
        /// <param name="b"></param>
        /// <param name="bCount"></param>
        /// <returns></returns>
        private static byte[] OnResolveRunTime(byte[] b, int bCount, ref DBFrame df)//1.2.1
        {
            Lift_RuntimeEp data = new Lift_RuntimeEp();
            string tStr = ConvertData.ToHexString(b, 0, 2);//获取帧头
            if (tStr != "7A7A")
                return null;
            byte[] sn = new byte[8];   //设备编号
            Array.Copy(b, 8, sn, 0, 8);
            data.EquipmentID = Encoding.ASCII.GetString(sn);  //获取设备编号ASCII
            //总运行时间
            data.TotalRuntime = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", b[19], b[18], b[17], b[16]), 16);
            //本机开机运行时间
            data.StartingUpRuntime = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", b[23], b[22], b[21], b[20]), 16);

            //包装
            df.deviceid = data.EquipmentID;
            df.datatype = "runtimeEp";
            df.contentjson = JsonConvert.SerializeObject(data);

            byte[] rb = new byte[13];
            //针头
            rb[0] = 0x7A;
            rb[1] = 0x7A;
            //协议版本号
            rb[2] = 0x01;
            rb[3] = 0x04;
            rb[4] = 0x00;
            //命令字
            rb[5] = 0x07;
            //数据长度
            rb[6] = 0x00;
            rb[7] = 0x00;
            //数据区
            rb[8] = 0x00;
            //校验和
            rb[9] = 0x00;
            rb[10] = 0x00;
            //结束
            rb[11] = 0x7B;
            rb[12] = 0x7B;
            return rb;
        }
        #endregion
        #region 时间校准
        //数据接收
        private static byte[] OnResolveTime(byte[] b, int bCount)
        {
            if (bCount != 0x1A)
                return null;
            if (BitConverter.ToUInt16(b, 22) != ConvertData.CRC16(b, 8, 14))//检验和
                return null;
            string tStr = ConvertData.ToHexString(b, 0, 2);//获取帧头
            if (tStr != "7A7A")
                return null;
            tStr = ConvertData.ToHexString(b, 16, 6);//获取时间

            DateTime getdate = new DateTime();
            byte[] bytes = new byte[19];
            try
            {
                getdate = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                bytes[0] = 0x7A;
                bytes[1] = 0x7A;
                //协议版本
                bytes[2] = 0x01;
                bytes[3] = 0x04;
                bytes[4] = 0x00;
                //命令字
                bytes[5] = 0x00;

                //数据长度
                bytes[6] = 0x07;
                bytes[7] = 0x00;
                //////////时间校准标示////
                bytes[8] = 0x01;

                DateTime nowTime = DateTime.Now;
                //时间
                bytes[9] = byte.Parse(nowTime.Year.ToString().Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                bytes[10] = byte.Parse(nowTime.Month.ToString(), System.Globalization.NumberStyles.HexNumber);
                bytes[11] = byte.Parse(nowTime.Day.ToString(), System.Globalization.NumberStyles.HexNumber);
                bytes[12] = byte.Parse(nowTime.Hour.ToString(), System.Globalization.NumberStyles.HexNumber);
                bytes[13] = byte.Parse(nowTime.Minute.ToString(), System.Globalization.NumberStyles.HexNumber);
                bytes[14] = byte.Parse(nowTime.Second.ToString(), System.Globalization.NumberStyles.HexNumber);
                //校验和
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(bytes, 8, 7));
                bytes[15] = crc16[0];//0x00;//算校验和
                bytes[16] = crc16[1];
                //结束
                bytes[17] = 0x7B;
                bytes[18] = 0x7B;

                return bytes;
            }
             
            DateTime now = System.DateTime.Now;
            double compare = (now - getdate).TotalMinutes;
            if (compare > 1 || compare < 0)  //需要
            {
                bytes = new byte[19];
                //数据长度
                bytes[6] = 0x07;
                bytes[7] = 0x00;
                //////////时间校准标示////
                bytes[8] = 0x01;
                //时间
                bytes[9] = byte.Parse(now.Year.ToString().Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                bytes[10] = byte.Parse(now.Month.ToString(), System.Globalization.NumberStyles.HexNumber);
                bytes[11] = byte.Parse(now.Day.ToString(), System.Globalization.NumberStyles.HexNumber);
                bytes[12] = byte.Parse(now.Hour.ToString(), System.Globalization.NumberStyles.HexNumber);
                bytes[13] = byte.Parse(now.Minute.ToString(), System.Globalization.NumberStyles.HexNumber);
                bytes[14] = byte.Parse(now.Second.ToString(), System.Globalization.NumberStyles.HexNumber);
                //校验和
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(bytes, 8, 7));
                bytes[15] = crc16[0];//0x00;//算校验和
                bytes[16] = crc16[1];
                //结束
                bytes[17] = 0x7B;
                bytes[18] = 0x7B;
            }
            else   //不需要
            {
                bytes = new byte[13];

                //长度
                bytes[6] = 0x01;
                bytes[7] = 0x00;
                //数据区
                bytes[8] = 0x00;
                //校验和
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(bytes, 8, 1));
                bytes[9] = crc16[0];//0x00;//算校验和
                bytes[10] = crc16[1];
                //结束
                bytes[11] = 0x7B;
                bytes[12] = 0x7B;
            }

            bytes[0] = 0x7A;
            bytes[1] = 0x7A;
            //协议版本
            bytes[2] = 0x01;
            bytes[3] = 0x04;
            bytes[4] = 0x00;
            //命令字
            bytes[5] = 0x08;
            return bytes;
        }
        #endregion
        #region 违规运行通知
        public static byte[] OnViolation(byte[] b, int bCount, TcpSocketClient client, ref DBFrame df)
        {
            #region 解析
            Lift_IllegalOperation data = new Lift_IllegalOperation();
            //设备编号
            byte[] t = new byte[8];
            for (int i = 8, j = 0; i < 16; i++, j++)
            {
                t[j] = b[i];
            }
            data.SN = Encoding.ASCII.GetString(t);
            //违规运行标识暂时先标注，对象再改
            data.IllegalOperationIdentifier = b[16];
            //包装
            df.deviceid = data.SN;
            df.datatype = "IllegalOperation";
            df.contentjson = JsonConvert.SerializeObject(data);
            #endregion

            #region 应答
            List<byte> rb = new List<byte>();
            byte[] byteTemp = new byte[6];
            Array.Copy(b, byteTemp, 6);
            rb.AddRange(byteTemp);
            //长度
            rb.Add(1); rb.Add(0);
            rb.Add(1);
            byte[] byteT = new byte[rb.Count];
            rb.CopyTo(byteT);
            byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(byteT, 8, rb.Count - 8));
            rb.Add(crc16[0]);
            rb.Add(crc16[1]);
            //结束符
            rb.Add(0x7B);
            rb.Add(0x7B);
            byteT = new byte[rb.Count];
            rb.CopyTo(byteT);
            client.SendBuffer(byteT);
            #endregion
            return null;
        }
        #endregion
    }
}
