using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Threading;
using TCPAPI;
using Architecture;
using ToolAPI;
using ProtocolAnalysis.Tool;
using Newtonsoft.Json;//存放对象的
using DPC;

namespace ProtocolAnalysis
{
    public static class GprsResolveDataV0E
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
                if (b.Length < 3)
                    return "";
                byte typ = b[3];
                if (typ == 0x00)//心跳
                {
                    byte[] rb = OnResolveHeabert(b, c);
                    if (rb != null)
                        client.SendBuffer(rb);
                }
                else if (typ == 0x01)//实时数据
                {
                    OnResolveRealData(b, c, client);
                }
                else if (typ == 0x03)//离线数据
                {
                    client.SendBuffer(new byte[] { 0x7E, 0x7E, 0x0E, 0x03, 0x00, 0x00, 0x7D, 0x7D });
                }
                else if (typ == 0x02)//身份验证    考虑考虑怎么处理，是不是给个直通车啥的。
                {
                    AuthenticationDispose(b, c, client);
                }
                else if (typ == 0x04)//召唤信息
                {
                    byte[] rb = OnResolveParamDataUpload(b, c);
                    if (rb != null)
                        client.SendBuffer(rb);
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
        #region 实时数据
        /// <summary>
        /// 实时数据
        /// </summary>
        /// <param name="b"></param>
        /// 发送7e 7e 0e 01 00 12 34 56 00 00 00 00 17 09 27 10 12 10 01 00 00 00 02 00 00 00 03 00 04 00 05 00 06 00 01 00 02 00 03 00 04 00 05 00 02 00 00 00 00 00 00 01 00 02 00 03 00 00 00 04 00 00 00 00 7d 7d
        public static Byte[] OnResolveRealData(byte[] b, int bCount, TcpSocketClient client)
        {
            string tStr = ConvertData.ToHexString(b, 0, 2);
            if (tStr != "7E7E")
                return null;
            Zhgd_iot_tower_current data = new Zhgd_iot_tower_current();
            #region 原协议
            //设备号
            data.sn = ConvertData.ToHexString(b, 5, 3);
            //司机卡号
            data.driver_id_code = ConvertData.ToHexString(b, 8, 4);
            //日期
            tStr = ConvertData.ToHexString(b, 12, 6);
            try
            {
                data.@timestamp = DPC_Tool.GetTimeStamp(DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture));
            }
            catch
            {
                data.@timestamp = DPC_Tool.GetTimeStamp();
            }
            IntValue iv = new IntValue();
            //经度
            iv.bValue1 = b[18];
            iv.bValue2 = b[19];
            iv.bValue3 = b[20];
            iv.bValue4 = b[21];
            //data.Current.Longitude = (iv.iValue / 100000.0).ToString("0.00");
            //纬度
            iv.bValue1 = b[22];
            iv.bValue2 = b[23];
            iv.bValue3 = b[24];
            iv.bValue4 = b[25];
            //data.Current.Latitude = (iv.iValue / 100000.0).ToString("0.00");
            UShortValue s = new UShortValue();
            //高度
            s.bValue1 = b[26];
            s.bValue2 = b[27];
            data.height = double.Parse((s.sValue / 100.00).ToString("0.00"));
            //幅度
            s.bValue1 = b[28];
            s.bValue2 = b[29];
            data.range = double.Parse((s.sValue / 100.00).ToString("0.00"));
            ShortValue sv = new ShortValue();
            //转角
            sv.bValue1 = b[30];
            sv.bValue2 = b[31];
            data.rotation = double.Parse((sv.sValue / 10.00).ToString("0.00"));
            //重量
            s.bValue1 = b[32];
            s.bValue2 = b[33];
            data.weight = double.Parse((s.sValue / 100.00).ToString("0.00"));
            //风速
            s.bValue1 = b[34];
            s.bValue2 = b[35];
            data.wind_speed = double.Parse((s.sValue / 100.00).ToString("0.00"));
            data.wind_grade = ConvertWind.WindToLeve(s.sValue / 100.0f);
            if (data.wind_grade >= 13)
                data.wind_grade = 12;
            //倾角X
            sv.bValue1 = b[36];
            sv.bValue2 = b[37];
            data.dip_x = double.Parse((sv.sValue / 100.00).ToString("0.00"));
            //倾角Y
            sv.bValue1 = b[38];
            sv.bValue2 = b[39];
            data.dip_y = double.Parse((sv.sValue / 100.00).ToString("0.00"));
            //安全力矩
            s.bValue1 = b[40];
            s.bValue2 = b[41];
            //data.Current.Safetorque = (s.sValue / 10.00).ToString("0.00");
            //安全起重量
            s.bValue1 = b[42];
            s.bValue2 = b[43];
            //data.Current.SafeWeight = (s.sValue / 100.00).ToString("0.00");
            //倍率
            //data.Current.Times = Convert.ToInt32(b[44]).ToString();

            //限位控制器状态
            //s.bValue1 = b[51];
            //s.bValue2 = b[52];
            //data.Current.LimitStatus = Convert.ToString(s.sValue, 2).PadLeft(16, '0');
            //LimitFlag(data, data.Current.LimitStatus);

            ////传感器状态
            //s.bValue1 = b[53];
            //s.bValue2 = b[54];
            //data.Current.SensorStatus = Convert.ToString(s.sValue, 2).PadLeft(16, '0');

            //预警告码
            IntValue i = new IntValue();
            //i.bValue1 = b[55];
            //i.bValue2 = b[56];
            //i.bValue3 = b[57];
            //i.bValue4 = b[58];
            //string WarnType = Convert.ToString(i.iValue, 2).PadLeft(32, '0');

            //WarnFlag(data, WarnType);
            //报警告码
            i.bValue1 = b[59];
            i.bValue2 = b[60];
            i.bValue3 = b[61];
            i.bValue4 = b[62];
            string AlarmType = Convert.ToString(i.iValue, 2).PadLeft(32, '0');//总共是32位右对齐
            AlarmFlag(data, AlarmType); //报警码解析
            tStr = ConvertData.ToHexString(b, 64, 2);
            if (tStr != "7D7D")
                return null;
            #endregion

            //进行数据put 
            Tower_operation.Send_tower_Current(data);
            return null;
        }
        /// <summary>
        /// 警告标识
        /// </summary>
        public static void AlarmFlag(Zhgd_iot_tower_current data, string AlarmStr)
        {
            int l = AlarmStr.Length;
            List<string> vs = new List<string>();
            data.is_warning = "N";
            #region 风速报警 bit0
            if (AlarmStr.Substring(l - 1, 1) == "1")//风速报警
            {
                vs.Add(Warning_type.风速报警);
                data.is_warning = "Y";
            }
            #endregion
            #region 超重报警 bit1
            if (AlarmStr.Substring(l - 2, 1) == "1")//超重报警
            {
                vs.Add(Warning_type.重量告警);
                data.is_warning = "Y";
            }
            #endregion
            #region 碰撞报警 bit2
            if (AlarmStr.Substring(l - 3, 1) == "1")//交叉干涉报警
            {
                vs.Add(Warning_type.碰撞报警);
                data.is_warning = "Y";
            }
            #endregion
            #region 力矩报警 bit3
            if (AlarmStr.Substring(l - 4, 1) == "1")//力矩报警
            {
                vs.Add(Warning_type.力矩报警);
                data.is_warning = "Y";
            }
            #endregion
            #region 倾斜 bit4
            if (AlarmStr.Substring(l - 5, 1) == "1")//倾斜报警
            {
                vs.Add(Warning_type.倾斜报警);
                data.is_warning = "Y";
            }
            #endregion
            #region 区域保护前bit12  区域保护后bit13 区域保护左bit14 区域保护右bit15
            if (AlarmStr.Substring(l - 13, 1) == "1" || AlarmStr.Substring(l - 14, 1) == "1" || AlarmStr.Substring(l - 15, 1) == "1" || AlarmStr.Substring(l - 16, 1) == "1")//前进进入禁止区域报警
            {
                vs.Add(Warning_type.区域保护报警);
                data.is_warning = "Y";
            }
            #endregion
            #region 上限位bit16  下限位bit17 外限位bit18 内限位bit19 左限位bit20 右限位bit21
            if (AlarmStr.Substring(l - 17, 1) == "1" || AlarmStr.Substring(l - 18, 1) == "1" || AlarmStr.Substring(l - 19, 1) == "1" || AlarmStr.Substring(l - 20, 1) == "1" || AlarmStr.Substring(l - 21, 1) == "1" || AlarmStr.Substring(l - 22, 1) == "1")//上升限位报警
            {
                vs.Add(Warning_type.限位报警);
                data.is_warning = "Y";
            }
            data.warning_type = vs.ToArray();
            #endregion
        }
        #endregion
        #region 心跳
        /// <summary>
        /// 心跳
        /// </summary>
        /// <param name="b"></param>
        /// 发送7e 7e 0e 00 00 12 34 56 17 09 27 10 10 10 00 7d 7d
        /// 接收7e 7e 0e 00 07 01 17 09 27 10 38 42 00 7d 7d 
        private static byte[] OnResolveHeabert(byte[] b, int bCount)
        {
            DateTime dt = DateTime.Now;
            if (bCount != 17)
            {
                return null;
            }
            string tStr = ConvertData.ToHexString(b, 0, 2);
            if (tStr != "7E7E")
                return null;
            //设备号
            string SN = ConvertData.ToHexString(b, 5, 3);
            long dttemp = DPC_Tool.GetTimeStamp(DateTime.Now);
            byte[] rb = new byte[15];
            //时间
            tStr = ConvertData.ToHexString(b, 8, 6);
            try
            {
                rb = new byte[9];
                rb[4] = 0x01;//应用数据区数据长度 
                rb[5] = 0x00;//1：校准时间
                rb[7] = 0x7D;
                rb[8] = 0x7D;
                DateTime getdate = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                double compare = (dt - getdate).TotalMinutes;
                if (compare > 1 || compare < 0)
                {
                    throw new Exception();
                }
                string time = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
                dttemp = DPC_Tool.GetTimeStamp(DateTime.Parse(time));
            }
            catch
            {
                rb = new byte[15];

                rb[4] = 0x07;//应用数据区数据长度
                rb[5] = 0x01;//1：校准时间
                rb[6] = byte.Parse(dt.Year.ToString().Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                rb[7] = byte.Parse(dt.Month.ToString(), System.Globalization.NumberStyles.HexNumber);
                rb[8] = byte.Parse(dt.Day.ToString(), System.Globalization.NumberStyles.HexNumber);
                rb[9] = byte.Parse(dt.Hour.ToString(), System.Globalization.NumberStyles.HexNumber);
                rb[10] = byte.Parse(dt.Minute.ToString(), System.Globalization.NumberStyles.HexNumber);
                rb[11] = byte.Parse(dt.Second.ToString(), System.Globalization.NumberStyles.HexNumber);
                rb[13] = 0x7D;
                rb[14] = 0x7D;
                string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                dttemp = DPC_Tool.GetTimeStamp(DateTime.Parse(time));
            }
            rb[0] = 0x7E;
            rb[1] = 0x7E;
            rb[2] = 0x0E;
            rb[3] = 0x00;//功能码
            //更新redis
            DPC.Tower_operation.Update_equminet_last_online_time(SN, dttemp);
            return rb;
        }
        #endregion
        #region 参数上传
        /// <summary>
        /// 参数上传
        /// </summary>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// 发送7e 7e 0e 04 00 12 34 56 17 09 27 10 15 10 02 01 00 02 00 03 00 04 00 05 00 06 00 07 00 08 00 09 00 0a 00 00 01 01 00 00 00 02 00 00 00 01 00 02 00 03 00 04 00 05 00 06 00 07 00 08 00 09 00 0a 00 01 02 03 01 00 02 00 03 00 04 00 05 00 06 00 07 00 08 00 01 00 02 00 03 00 04 00 05 00 06 00 07 00 08 00 01 02 03 04 01 02 03 00 7d 7d
        /// 返回7e 7e 0e 04 01 01 00 7d 7d
        private static byte[] OnResolveParamDataUpload(byte[] b, int bCount)
        {
            //写入数据库
            #region 应答
            byte[] rb = new byte[9];
            rb[0] = 0x7E;
            rb[1] = 0x7E;
            rb[2] = 0x0E;
            rb[3] = 0x04;
            rb[4] = 0x01;//应用数据区数据长度
            rb[5] = 0x01;//0x01：接收成功
            rb[7] = 0x7D;
            rb[8] = 0x7D;
            return rb;
            #endregion

        }
        #endregion
        #region 身份验证
        //身份验证解析入口
        private static void AuthenticationDispose(byte[] b, int c, TcpSocketClient client)
        {
            try
            {
                if (b[8] == 0x00 && b[13] == 0x00)//身份验证
                {
                    byte[] rb = OnResolveAuthentication(b, 0, 1);
                    if (rb != null)
                        client.SendBuffer(rb);
                }
                else if (b[8] == 0x01)//上下班状态
                {
                    byte[] rb = OnResolveAuthentication(b, 1, 1);
                    if (rb != null)
                        client.SendBuffer(rb);
                }
                else if (b[8] == 0x02)
                {
                    byte[] rb = OnResolveAuthentication(b, 2, 1);
                    if (rb != null)
                        client.SendBuffer(rb);
                }
             
            }
            catch (Exception ex)
            { }
        }
        

        /// <summary>
        /// 服务应答（与设备的交互）
        ///type 0身份认证  1上班  2下班  3删除命令
        /// </summary>
        /// <returns></returns>
        private static byte[] OnResolveAuthentication(byte[] b, int type, byte result)
        {
            //数据原路返回
            if (b.Length > 17)
            {
                byte[] rb = new byte[17];
                for (int i = 0; i < 17; i++)
                    rb[i] = b[i];
                //帧长度
                rb[4] = 0x09;
                switch (type)
                {
                    case 0: b[8] = 0x00; rb[13] = result; break;//身份认证
                    case 1: b[8] = 0x01; rb[13] = result; break;//上班
                    case 2: b[8] = 0x02; rb[13] = result; break;//下班
                    case 3: b[8] = 0x00; rb[13] = 0x02; break;//删除卡号
                }
                rb[14] = 0x00; //校验和 下位机暂未处理，可以默认值处理
                rb[15] = 0x7d;
                rb[16] = 0x7d;
                return rb;
            }
            return null;
        }
        #endregion
        #endregion
    }

}
