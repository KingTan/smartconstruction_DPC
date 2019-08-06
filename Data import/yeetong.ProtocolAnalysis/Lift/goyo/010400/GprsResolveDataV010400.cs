using Architecture;
using DPC;
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

            byte typ = b[5];
            //心跳
            if (typ == 0x00)
            {
                byte[] rb = OnResolveHeabert(b, c, client);
                if (rb != null)
                    client.SendBuffer(rb);
            }
            //实时数据 
            if (typ == 0x01)
            {
                OnResolveRealData(b, c);
            }
            //身份验证
            if (typ == 0x02)
            {
                AuthenticationDispose(b, c, client);
            }
            //离线数据
            if (typ == 0x03)
            {
                OnResolveRealData(b, c);
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
                byte[] rb = OnResolveParamDataUpload(b, c);
                if (rb != null)
                    client.SendBuffer(rb);
            }
            /*重新整理*/
            if (typ == 0x07) // 设备运行时间
            {
                byte[] rb = OnResolveRunTime(b, c);
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
                OnViolation(b, c, client);
            }
            return null;
        }

        #region 心跳
        /// <summary>
        /// 心跳接收数据
        /// </summary>
        /// <param name="b"></param>
        public static byte[] OnResolveHeabert(byte[] b, int bCount, TcpSocketClient client)//1.2.1
        {

            if (bCount != 0x1A)
                return null;
            if (BitConverter.ToUInt16(b, 22) != ConvertData.CRC16(b, 8, 14))//检验和
                return null;
            string tStr = ConvertData.ToHexString(b, 0, 2);//获取帧头
            if (tStr != "7A7A")
                return null;
            byte[] t = new byte[8];
            for (int i = 8, j = 0; i < 16; i++, j++)
            {
                t[j] = b[i];
            }
            string SN = Encoding.ASCII.GetString(t);
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

                //更新redis
                DPC.Lift_operation.Update_equminet_last_online_time(SN, DPC_Tool.GetTimeStamp(nowTime));
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
                getdate = DateTime.Now;
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
            }

            bytes[0] = 0x7A;
            bytes[1] = 0x7A;
            //协议版本
            bytes[2] = 0x01;
            bytes[3] = 0x04;
            bytes[4] = 0x00;
            //命令字
            bytes[5] = 0x00;


            //更新redis
            DPC.Lift_operation.Update_equminet_last_online_time(SN, DPC_Tool.GetTimeStamp(getdate));
            return bytes;
        }
        #endregion
        #region 实时数据
        /// <summary>
        /// 实时数据
        /// </summary>
        /// <param name="b"></param>
        /// 7A7A0103000144003132333435363738000000000000000000000000000102000841000000000000000000000000000000000F0000000000000000000000000000000000000000000000000006637B7B7A7A010300000E003132333435363738000102000841DC7F7B7B
        public static Byte[] OnResolveRealData(byte[] b, int bCount)
        {
            string tStr = ConvertData.ToHexString(b, 0, 2);
            if (tStr != "7A7A")
                return null;
            Zhgd_iot_lift_current data = new Zhgd_iot_lift_current();
            //设备编号
            byte[] t = new byte[8];
            for (int i = 8, j = 0; i < 16; i++, j++)
            {
                t[j] = b[i];
            }
            data.sn = Encoding.ASCII.GetString(t);
            string str1 = b[19].ToString("X2") + b[18].ToString("X2") + b[17].ToString("X2") + b[16].ToString("X2");
            //司机工号
            for (int i = 20, j = 0; i < 28; i++, j++)
            {
                t[j] = b[i];
            }
            //日期
            tStr = ConvertData.ToHexString(b, 28, 6);
            try
            {
                data.timestamp =DPC.DPC_Tool.GetTimeStamp( DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture));

            }
            catch
            {
                data.timestamp = DPC.DPC_Tool.GetTimeStamp(DateTime.Now);
            }
            //重量 吨
            UShortValue s = new UShortValue();
            string str = b[35].ToString("X2") + b[34].ToString("X2");
            int result = int.Parse(str, System.Globalization.NumberStyles.HexNumber);
            data.weight = double.Parse((result / 1000.00).ToString("0.00"));//设备发送单位：吨
            //当前额定载荷
            str = b[37].ToString("X2") + b[36].ToString("X2");
            result = int.Parse(str, System.Globalization.NumberStyles.HexNumber);
            //data.RatedWeight = (result / 1000.00).ToString("0.00");//设备发送单位：吨
            //高度 m
            s.bValue1 = b[38];
            s.bValue2 = b[39];
            data.height = double.Parse((s.sValue / 10.00).ToString("0.00"));//设备发送单位：分米
            //速度 m/秒
            s.bValue1 = b[40];
            s.bValue2 = b[41];
            data.speed = double.Parse((s.sValue / 10.00).ToString("0.00"));//设备发送单位：分米//设备发送单位：分米/秒
            //楼层
            s.bValue1 = b[42];
            data.floor = int.Parse( s.sValue.ToString("0"));
            //人数
            s.bValue1 = b[43];
            data.peoples = int.Parse(s.sValue.ToString("0"));
            //传感器状态
            s.bValue1 = b[44];
            s.bValue2 = b[45];
           // data.SensorSet = Convert.ToString(s.sValue, 2).PadLeft(16, '0');
            //报警码
            s.bValue1 = b[46];
            s.bValue2 = b[47];
            //2字节： 0-重量预警 1-重量报警 2-顶层预警 3-顶层报警 4-蹲底 5-门打开 6-风速预警 7-风速报警 9-人数报警 10-防坠器报警
            //data.AlarmType = Convert.ToString(s.sValue, 2).PadLeft(16, '0');//2字节：0-顶层预警 1-底层预警 2-顶层报警 3-底层报警 4-重量预警 5-重量报警 6-偏载 7-风速预警 8-风速报警 9-门未关好
            string alarm = Convert.ToString(s.sValue, 2).PadLeft(16, '0');
            List<string> vs = new List<string>();
            data.is_warning = "N";
            if (alarm[14] == '1') { vs.Add(Warning_type.重量告警); data.is_warning = "Y"; }
            if (alarm[8] == '1') { vs.Add(Warning_type.风速报警); data.is_warning = "Y"; }
            if (alarm[7] == '1') { vs.Add(Warning_type.人数报警); data.is_warning = "Y"; }
            if (alarm[6] == '1') { vs.Add(Warning_type.防坠器报警); data.is_warning = "Y"; }
            data.warning_type = vs.ToArray();
            //继电器状态
            s.bValue1 = b[48];
          //  data.PowerStatu = s.sValue.ToString("0");
            //GPRS信号强度
          //  data.GprsSignal = ((sbyte)b[49]).ToString();
            ///门状态
            data.door_status = (b[50]+Convert.ToByte(64)).ToString();//b[50].ToString();/// 加192 Convert.ToInt32("11000000",2)
            
            //风力等级
            data.wind_grade = b[51];
            //风速
            s.bValue1 = b[52];
            s.bValue2 = b[53];
            data.wind_speed = double.Parse( (s.sValue / 100.00).ToString("0.00"));//设备发送单位：cm/s 除以100得出m/s
            //倾角X
            short X = Convert.ToInt16(("0x" + b[55].ToString("X2") + b[54].ToString("X2")), 16);
            data.dip_x = double.Parse((X / 100.00).ToString("0.00"));  //倾角X 协议要求除以100
            //倾角Y
            short Y = Convert.ToInt16(("0x" + b[57].ToString("X2") + b[56].ToString("X2")), 16);
            data.dip_y = double.Parse((Y / 100.00).ToString("0.00"));  //倾角Y 协议要求除以100
            tStr = ConvertData.ToHexString(b, 78, 2);
            //身份证号处理
            byte[] d = new byte[18];
            for (int i = 58, j = 0; i < 76; i++, j++)
            {
                d[j] = b[i];
            }
            data.driver_id_code = Encoding.ASCII.GetString(d).Replace("\0","");
            if (tStr != "7B7B")
                return null;

            //进行数据put 
            Lift_operation.Send_Lift_Current(data);
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
            }
            catch (Exception) { }
        }
       
        #endregion
        #region 参数上传
        /// <summary>
        /// 参数上传
        /// </summary>
        /// <param name="b"></param>
        /// <param name="c"></param>
        private static byte[] OnResolveParamDataUpload(byte[] b, int bCount)
        {
           
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
        private static byte[] OnResolveRunTime(byte[] b, int bCount)//1.2.1
        {
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
        public static byte[] OnViolation(byte[] b, int bCount, TcpSocketClient client)
        {
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
