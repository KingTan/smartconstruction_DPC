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
using Newtonsoft.Json;//存放对象的
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
                DBFrame df = new DBFrame();
                df.contenthex = ConvertData.ToHexString(b, 0, c);
                df.version = (client.External.External as TcpClientBindingExternalClass).TVersion;

                byte typ = b[3];
                if (typ == 0x00)//心跳
                {
                    byte[] rb = OnResolveHeabert(b, c, ref df);
                    if (rb != null)
                        client.SendBuffer(rb);
                }
                else if (typ == 0x01)//实时数据
                {
                    OnResolveRealData(b, c, client, ref df);
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
                    byte[] rb = OnResolveParamDataUpload(b, c, ref df);
                    if (rb != null)
                        client.SendBuffer(rb);
                }
                else if (typ == 0x05)//修改IP的设备应答帧
                {
                    OnResolveIpAck(b, c);
                }
                else if (typ == 0x06)//限位控制
                {
                    OnResolveControlAck(b, c);
                }
                else if (typ == 0x07)//设备运行时间
                {
                    byte[] rb = OnResolveRunTimeAck(b, c, ref df);
                    if (rb != null)
                        client.SendBuffer(rb);
                }

                TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;
                if (TcpExtendTemp.EquipmentID == null || TcpExtendTemp.EquipmentID.Equals(""))
                {
                    TcpExtendTemp.EquipmentID = df.deviceid;
                }
                //存入数据库
                if (df.contentjson != null && df.contentjson != "")
                {
                    DB_MysqlTowerCrane.SaveTowerCrane(df);
                }
                return "";
            }
            catch(Exception ex)
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
        public static Byte[] OnResolveRealData(byte[] b, int bCount, TcpSocketClient client, ref DBFrame df)
        {
            string tStr = ConvertData.ToHexString(b, 0, 2);
            if (tStr != "7E7E")
                return null;
            GprsCraneDataObject data = new GprsCraneDataObject();
            if (bCount == 66)
            {
                #region 原协议
                //设备号
                data.Current.Craneno = ConvertData.ToHexString(b, 5, 3);
                //司机卡号
                data.Current.Card = ConvertData.ToHexString(b, 8, 4);
                //日期
                tStr = ConvertData.ToHexString(b, 12, 6);
                try
                {
                    data.Current.Rtime = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
                }
                catch
                {
                    data.Current.Rtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
                data.Current.Rtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                IntValue iv = new IntValue();
                //经度
                iv.bValue1 = b[18];
                iv.bValue2 = b[19];
                iv.bValue3 = b[20];
                iv.bValue4 = b[21];
                data.Current.Longitude = (iv.iValue / 100000.0).ToString("0.00");
                //纬度
                iv.bValue1 = b[22];
                iv.bValue2 = b[23];
                iv.bValue3 = b[24];
                iv.bValue4 = b[25];
                data.Current.Latitude = (iv.iValue / 100000.0).ToString("0.00");
                UShortValue s = new UShortValue();
                //高度
                s.bValue1 = b[26];
                s.bValue2 = b[27];
                data.Current.Height = (s.sValue / 100.00).ToString("0.00");
                //幅度
                s.bValue1 = b[28];
                s.bValue2 = b[29];
                data.Current.Radius = (s.sValue / 100.00).ToString("0.00");
                ShortValue sv = new ShortValue();
                //转角
                sv.bValue1 = b[30];
                sv.bValue2 = b[31];
                data.Current.Angle = (sv.sValue / 10.00).ToString("0.00");
                //重量
                s.bValue1 = b[32];
                s.bValue2 = b[33];
                data.Current.Weight = (s.sValue / 100.00).ToString("0.00");
                //风速
                s.bValue1 = b[34];
                s.bValue2 = b[35];
                data.Current.Wind = (s.sValue / 100.00).ToString("0.00");
                data.Current.WindLevel = ConvertWind.WindToLeve(s.sValue / 100.0f).ToString();
                if (int.Parse(data.Current.WindLevel) > 13)
                    data.Current.Wind = "12";
                //倾角X
                sv.bValue1 = b[36];
                sv.bValue2 = b[37];
                data.Current.AngleX = (sv.sValue / 100.00).ToString("0.00");
                //倾角Y
                sv.bValue1 = b[38];
                sv.bValue2 = b[39];
                data.Current.AngleY = (sv.sValue / 100.00).ToString("0.00");
                //安全力矩
                s.bValue1 = b[40];
                s.bValue2 = b[41];
                data.Current.Safetorque = (s.sValue / 10.00).ToString("0.00");
                //安全起重量
                s.bValue1 = b[42];
                s.bValue2 = b[43];
                data.Current.SafeWeight = (s.sValue / 100.00).ToString("0.00");
                /*ZT20160923添加计算力矩*/
                data.Current.Torque = (double.Parse(data.Current.Weight) * double.Parse(data.Current.Radius)).ToString("0.00");
                //力矩百分比
                if (data.Current.Safetorque != "0.00")
                    data.Current.Torquepercent = ((double.Parse(data.Current.Weight) * double.Parse(data.Current.Radius)) / double.Parse(data.Current.Safetorque)).ToString("0.00");
                else
                    data.Current.Torquepercent = "0.00";
                //倍率
                data.Current.Times = Convert.ToInt32(b[44]).ToString();
                if (int.Parse(data.Current.Times) > 4)
                    data.Current.Times = "2";
                //限位控制器状态
                s.bValue1 = b[51];
                s.bValue2 = b[52];
                data.Current.LimitStatus = Convert.ToString(s.sValue, 2).PadLeft(16, '0');
                LimitFlag(data, data.Current.LimitStatus);

                //传感器状态
                s.bValue1 = b[53];
                s.bValue2 = b[54];
                data.Current.SensorStatus = Convert.ToString(s.sValue, 2).PadLeft(16, '0');

                //预警告码
                IntValue i = new IntValue();
                i.bValue1 = b[55];
                i.bValue2 = b[56];
                i.bValue3 = b[57];
                i.bValue4 = b[58];
                data.Current.WarnType = Convert.ToString(i.iValue, 2).PadLeft(32, '0');
                WarnFlag(data, data.Current.WarnType);
                //报警告码
                i.bValue1 = b[59];
                i.bValue2 = b[60];
                i.bValue3 = b[61];
                i.bValue4 = b[62];
                data.Current.AlarmType = Convert.ToString(i.iValue, 2).PadLeft(32, '0');//总共是32位右对齐
                AlarmFlag(data, data.Current.AlarmType); //报警码解析
                data.Current.WorkCircle = "0"; //注销不需要计算了工作循环了
                tStr = ConvertData.ToHexString(b, 64, 2);
                if (tStr != "7D7D")
                    return null;
                #endregion
            }
            else if (bCount == 70)
            {
                #region 荆门
                //设备号
                data.Current.Craneno = ConvertData.ToHexString(b, 5, 3);
                //司机卡号
                byte[] emp = new byte[8];
                for (int l = 8, j = 0; l < 16; l++, j++)
                {
                    emp[j] = b[l];
                }
                data.Current.Card = Encoding.ASCII.GetString(emp);
                //日期
                tStr = ConvertData.ToHexString(b, 16, 6);
                try
                {
                    data.Current.Rtime = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
                }
                catch
                {
                    data.Current.Rtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }
                data.Current.Rtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
               
                UShortValue s = new UShortValue();
                //高度
                s.bValue1 = b[30];
                s.bValue2 = b[31];
                data.Current.Height = (s.sValue / 100.00).ToString("0.00");
                //幅度
                s.bValue1 = b[32];
                s.bValue2 = b[33];
                data.Current.Radius = (s.sValue / 100.00).ToString("0.00");
                ShortValue sv = new ShortValue();
                //转角
                sv.bValue1 = b[34];
                sv.bValue2 = b[35];
                data.Current.Angle = (sv.sValue / 10.00).ToString("0.00");
                //重量
                s.bValue1 = b[36];
                s.bValue2 = b[37];
                data.Current.Weight = (s.sValue / 100.00).ToString("0.00");
                //风速
                s.bValue1 = b[38];
                s.bValue2 = b[39];
                data.Current.Wind = (s.sValue / 100.00).ToString("0.00");
                data.Current.WindLevel = ConvertWind.WindToLeve(s.sValue / 100.0f).ToString();
                if (int.Parse(data.Current.WindLevel) > 13)
                    data.Current.Wind = "12";
                //倾角X
                sv.bValue1 = b[40];
                sv.bValue2 = b[41];
                data.Current.AngleX = (sv.sValue / 100.00).ToString("0.00");
                //倾角Y
                sv.bValue1 = b[42];
                sv.bValue2 = b[43];
                data.Current.AngleY = (sv.sValue / 100.00).ToString("0.00");
                //安全力矩
                s.bValue1 = b[44];
                s.bValue2 = b[45];
                data.Current.Safetorque = (s.sValue / 10.00).ToString("0.00");
                //安全起重量
                s.bValue1 = b[46];
                s.bValue2 = b[47];
                //ToolAPI.XMLOperation.WriteLogXmlNoTail(string.Format("【{0}】设备号:{1} 安全起重量{2}{3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), data.Current.Craneno, b[42].ToString(), b[43].ToString()));
                data.Current.SafeWeight = (s.sValue / 100.00).ToString("0.00");
                /*ZT20160923添加计算力矩*/
                data.Current.Torque = (double.Parse(data.Current.Weight) * double.Parse(data.Current.Radius)).ToString("0.00");
                //力矩百分比
                if (data.Current.Safetorque != "0.00")

                    data.Current.Torquepercent = ((double.Parse(data.Current.Weight) * double.Parse(data.Current.Radius)) / double.Parse(data.Current.Safetorque)).ToString("0.00");
                else
                    data.Current.Torquepercent = "0.00";
                //倍率
                data.Current.Times = Convert.ToInt32(b[48]).ToString();
                if (int.Parse(data.Current.Times) > 4)
                    data.Current.Times = "2";
                //限位控制器状态
                s.bValue1 = b[55];
                s.bValue2 = b[56];
                data.Current.LimitStatus = Convert.ToString(s.sValue, 2).PadLeft(16, '0');
                LimitFlag(data, data.Current.LimitStatus);

                //传感器状态
                s.bValue1 = b[57];
                s.bValue2 = b[58];
                data.Current.SensorStatus = Convert.ToString(s.sValue, 2).PadLeft(16, '0');

                //预警告码
                IntValue i = new IntValue();
                i.bValue1 = b[59];
                i.bValue2 = b[60];
                i.bValue3 = b[61];
                i.bValue4 = b[62];
                data.Current.WarnType = Convert.ToString(i.iValue, 2).PadLeft(32, '0');
                WarnFlag(data, data.Current.WarnType);
                //报警告码
                i.bValue1 = b[63];
                i.bValue2 = b[64];
                i.bValue3 = b[65];
                i.bValue4 = b[66];
                data.Current.AlarmType = Convert.ToString(i.iValue, 2).PadLeft(32, '0');
                AlarmFlag(data, data.Current.AlarmType);
                tStr = ConvertData.ToHexString(b, 68, 2);
                if (tStr != "7D7D")
                    return null;

                #endregion
            }
            else
            {
                return null;
            }
            //存数据库
            //看看是否发送短信
            //存入数据库
            df.deviceid = data.Current.Craneno;
            df.datatype = "current";
            df.contentjson = JsonConvert.SerializeObject(data.Current);

            string sourId = data.Current.Craneno;
            //数据库的拷贝
            if (!string.IsNullOrEmpty(MainStatic.DeviceCopy_TowerCrane))
            {
                if (MainStatic.DeviceCopy_TowerCrane.Contains(sourId + "#"))
                {
                    try
                    {
                        string[] strary = MainStatic.DeviceCopy_TowerCrane.Split(';');
                        foreach (string dev in strary)
                        {
                            if(dev.Contains(sourId + "#"))
                            {
                                string[] devcopy = dev.Split('#');
                                data.Current.Craneno = devcopy[1];
                                DBFrame dfcopy = DBFrame.DeepCopy(df);
                                dfcopy.deviceid = devcopy[1];
                                dfcopy.datatype = "current";
                                dfcopy.contentjson = JsonConvert.SerializeObject(data.Current);
                                if (dfcopy.contentjson != null && dfcopy.contentjson != "")
                                {
                                    DB_MysqlTowerCrane.SaveTowerCrane(dfcopy);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                }
            }
            return null;
        }
        /// <summary>
        /// 警告标识
        /// </summary>
        public static void AlarmFlag(GprsCraneDataObject data, string AlarmStr)
        {
            int l = AlarmStr.Length;
            bool flag = false;
            #region 风速报警 bit0
            if (AlarmStr.Substring(l - 1, 1) == "1")//风速报警
            {
                data.Current.WindAlarm = "2";
                flag = true;
                //去掉预警与报警同时出现
                if (data.Current.WindAlarm_Warn == "1")
                    data.Current.WindAlarm_Warn = "0";
            }
            else
            {
                data.Current.WindAlarm = "0";
            }
            #endregion
            #region 超重报警 bit1
            if (AlarmStr.Substring(l - 2, 1) == "1")//超重报警
            {
                data.Current.WeightAlarm = "2";
                flag = true;
                //去掉预警与报警同时出现
                if (data.Current.WeightAlarm_Warn == "1")
                    data.Current.WeightAlarm_Warn = "0";
            }
            else
            {
                data.Current.WeightAlarm = "0";
            }
            #endregion
            #region 碰撞报警 bit2
            if (AlarmStr.Substring(l - 3, 1) == "1")//交叉干涉报警
            {
                data.Current.HitAlarm = "2";
                flag = true;
                //去掉预警与报警同时出现
                if (data.Current.HitAlarm_Warn == "1")
                    data.Current.HitAlarm_Warn = "0";
            }
            else
            {
                data.Current.HitAlarm = "0";
            }
            #endregion
            #region 力矩报警 bit3
            if (AlarmStr.Substring(l - 4, 1) == "1")//力矩报警
            {
                data.Current.TorqueAlarm = "2";
                flag = true;
                //去掉预警与报警同时出现
                if (data.Current.TorqueAlarm_Warn == "1")
                    data.Current.TorqueAlarm_Warn = "0";
            }
            else
            {
                data.Current.TorqueAlarm = "0";
            }
            #endregion
            #region 倾斜 bit4
            if (AlarmStr.Substring(l - 5, 1) == "1")//倾斜报警
            {
                data.Current.AngleAlarm = "2";
                flag = true;
                //去掉预警与报警同时出现
                if (data.Current.AngleAlarm_Warn == "1")
                    data.Current.AngleAlarm_Warn = "0";
            }
            else
            {
                data.Current.AngleAlarm = "0";
            }
            #endregion
            #region 前碰撞bit8  后碰撞bit9 左碰撞bit10 右碰撞bit11
            if (AlarmStr.Substring(l - 9, 1) == "1")//前进多机防碰撞报警
            {
                data.Current.InAlarm_Hit = "2";
                flag = true;
                //去掉预警与报警同时出现
                if (data.Current.InAlarm_Hit_Warn == "1")
                    data.Current.InAlarm_Hit_Warn = "0";
            }
            else
            {
                data.Current.InAlarm_Hit = "0";
            }
            if (AlarmStr.Substring(l - 10, 1) == "1")//后退多机防碰撞报警
            {
                data.Current.OutAlarm_Hit = "2";
                flag = true;
                //去掉预警与报警同时出现
                if (data.Current.OutAlarm_Hit_Warn == "1")
                    data.Current.OutAlarm_Hit_Warn = "0";
            }
            else
            {
                data.Current.OutAlarm_Hit = "0";
            }
            if (AlarmStr.Substring(l - 11, 1) == "1")//左转多机防碰撞报警
            {
                data.Current.LeftAlarm_Hit = "2";
                flag = true;
                //去掉预警与报警同时出现
                if (data.Current.LeftAlarm_Hit_Warn == "1")
                    data.Current.LeftAlarm_Hit_Warn = "0";
            }
            else
            {
                data.Current.LeftAlarm_Hit = "0";
            }
            if (AlarmStr.Substring(l - 12, 1) == "1")//右转多机防碰撞报警
            {
                data.Current.RightAlarm_Hit = "2";
                flag = true;
                //去掉预警与报警同时出现
                if (data.Current.RightAlarm_Hit_Warn == "1")
                    data.Current.RightAlarm_Hit_Warn = "0";
            }
            else
            {
                data.Current.RightAlarm_Hit = "0";
            }
            #endregion
            #region 区域保护前bit12  区域保护后bit13 区域保护左bit14 区域保护右bit15
            if (AlarmStr.Substring(l - 13, 1) == "1")//前进进入禁止区域报警
            {
                data.Current.InAlarm_Area = "2";
                flag = true;
                //去掉预警与报警同时出现
                if (data.Current.InAlarm_Area_Warn == "1")
                    data.Current.InAlarm_Area_Warn = "0";
            }
            else
            {
                data.Current.InAlarm_Area = "0";
            }
            if (AlarmStr.Substring(l - 14, 1) == "1")//后退进入禁止区域报警
            {
                data.Current.OutAlarm_Area = "2";
                flag = true;
                //去掉预警与报警同时出现
                if (data.Current.OutAlarm_Area_Warn == "1")
                    data.Current.OutAlarm_Area_Warn = "0";
            }
            else
            {
                data.Current.OutAlarm_Area = "0";
            }
            if (AlarmStr.Substring(l - 15, 1) == "1")//左转进入禁止区域报警
            {
                data.Current.LeftAlarm_Area = "2";
                flag = true;
                //去掉预警与报警同时出现
                if (data.Current.LeftAlarm_Area_Warn == "1")
                    data.Current.LeftAlarm_Area_Warn = "0";
            }
            else
            {
                data.Current.LeftAlarm_Area = "0";
            }
            if (AlarmStr.Substring(l - 16, 1) == "1")//右转进入禁止区域报警
            {
                data.Current.RightAlarm_Area = "2";
                flag = true;
                //去掉预警与报警同时出现
                if (data.Current.RightAlarm_Area_Warn == "1")
                    data.Current.RightAlarm_Area_Warn = "0";
            }
            else
            {
                data.Current.RightAlarm_Area = "0";
            }
            #endregion
            #region 上限位bit16  下限位bit17 外限位bit18 内限位bit19 左限位bit20 右限位bit21
            if (AlarmStr.Substring(l - 17, 1) == "1")//上升限位报警
            {
                data.Current.UpAlarm = "2";
                flag = true;
                //去掉预警与报警同时出现
                if (data.Current.UpAlarm_Warn == "1")
                    data.Current.UpAlarm_Warn = "0";
            }
            else
            {
                data.Current.UpAlarm = "0";
            }
            if (AlarmStr.Substring(l - 18, 1) == "1")//下降限位报警
            {
                data.Current.DownAlarm = "2";
                flag = true;
                //去掉预警与报警同时出现
                if (data.Current.DownAlarm_Warn == "1")
                    data.Current.DownAlarm_Warn = "0";
            }
            else
            {
                data.Current.DownAlarm = "0";
            }
            if (AlarmStr.Substring(l - 19, 1) == "1")//小车外限位报警
            {
                data.Current.OutAlarm = "2";
                flag = true;
                //去掉预警与报警同时出现
                if (data.Current.OutAlarm_Warn == "1")
                    data.Current.OutAlarm_Warn = "0";
            }
            else
            {
                data.Current.OutAlarm = "0";
            }
            if (AlarmStr.Substring(l - 20, 1) == "1")//小车内限位报警
            {
                data.Current.InAlarm = "2";
                flag = true;
                //去掉预警与报警同时出现
                if (data.Current.InAlarm_Warn == "1")
                    data.Current.InAlarm_Warn = "0";
            }
            else
            {
                data.Current.InAlarm = "0";
            }
            if (AlarmStr.Substring(l - 21, 1) == "1")//左转限位报警
            {
                data.Current.LeftAlarm = "2";
                flag = true;
                //去掉预警与报警同时出现
                if (data.Current.LeftAlarm_Warn == "1")
                    data.Current.LeftAlarm_Warn = "0";
            }
            else
            {
                data.Current.LeftAlarm = "0";
            }
            if (AlarmStr.Substring(l - 22, 1) == "1")//右转限位报警
            {
                data.Current.RightAlarm = "2";
                flag = true;
                //去掉预警与报警同时出现
                if (data.Current.RightAlarm_Warn == "1")
                    data.Current.RightAlarm_Warn = "0";
            }
            else
            {
                data.Current.RightAlarm = "0";
            }
            #endregion

            //报警标示
            if (flag)
                data.Current.Type = "2";
            else
                data.Current.Type = "0";
        }
        /// <summary>
        /// 预警标识
        /// </summary>
        public static void WarnFlag(GprsCraneDataObject data, string WarnStr)
        {
            int l = WarnStr.Length;
            if (WarnStr.Substring(l - 5, 1) == "1")//倾斜预警
            {
                data.Current.AngleAlarm_Warn = "1";
            }
            else
            {
                data.Current.AngleAlarm_Warn = "0";
            }
            if (WarnStr.Substring(l - 4, 1) == "1")//力矩预警
            {
                data.Current.TorqueAlarm_Warn = "1";
            }
            else
            {
                data.Current.TorqueAlarm_Warn = "0";
            }
            if (WarnStr.Substring(l - 3, 1) == "1")//交叉干涉预警
            {
                data.Current.HitAlarm_Warn = "1";
            }
            else
            {
                data.Current.HitAlarm_Warn = "0";
            }
            if (WarnStr.Substring(l - 2, 1) == "1")//超重预警
            {
                data.Current.WeightAlarm_Warn = "1";
            }
            else
            {
                data.Current.WeightAlarm_Warn = "0";
            }
            if (WarnStr.Substring(l - 1, 1) == "1")//风速预警
            {
                data.Current.WindAlarm_Warn = "1";
            }
            else
            {
                data.Current.WindAlarm_Warn = "0";
            }
            if (WarnStr.Substring(l - 9, 1) == "1")//前进多机防碰撞预警
            {
                data.Current.InAlarm_Hit_Warn = "1";
            }
            else
            {
                data.Current.InAlarm_Hit_Warn = "0";
            }
            if (WarnStr.Substring(l - 10, 1) == "1")//后退多机防碰撞预警
            {
                data.Current.OutAlarm_Hit_Warn = "1";
            }
            else
            {
                data.Current.OutAlarm_Hit_Warn = "0";
            }
            if (WarnStr.Substring(l - 11, 1) == "1")//左转多机防碰撞预警
            {
                data.Current.LeftAlarm_Hit_Warn = "1";
            }
            else
            {
                data.Current.LeftAlarm_Hit_Warn = "0";
            }
            if (WarnStr.Substring(l - 12, 1) == "1")//右转多机防碰撞预警
            {
                data.Current.RightAlarm_Hit_Warn = "1";
            }
            else
            {
                data.Current.RightAlarm_Hit_Warn = "0";
            }
            if (WarnStr.Substring(l - 13, 1) == "1")//前进进入禁止区域预警
            {
                data.Current.InAlarm_Area_Warn = "1";
            }
            else
            {
                data.Current.InAlarm_Area_Warn = "0";
            }
            if (WarnStr.Substring(l - 14, 1) == "1")//后退进入禁止区域预警
            {
                data.Current.OutAlarm_Area_Warn = "1";
            }
            else
            {
                data.Current.OutAlarm_Area_Warn = "0";
            }
            if (WarnStr.Substring(l - 15, 1) == "1")//左转进入禁止区域预警
            {
                data.Current.LeftAlarm_Area_Warn = "1";
            }
            else
            {
                data.Current.LeftAlarm_Area_Warn = "0";
            }
            if (WarnStr.Substring(l - 16, 1) == "1")//右转进入禁止区域预警
            {
                data.Current.RightAlarm_Area_Warn = "1";
            }
            else
            {
                data.Current.RightAlarm_Area_Warn = "0";
            }
            if (WarnStr.Substring(l - 17, 1) == "1")//上升限位预警
            {
                data.Current.UpAlarm_Warn = "1";
            }
            else
            {
                data.Current.UpAlarm_Warn = "0";
            }
            if (WarnStr.Substring(l - 18, 1) == "1")//下降限位预警
            {
                data.Current.DownAlarm_Warn = "1";
            }
            else
            {
                data.Current.DownAlarm_Warn = "0";
            }
            if (WarnStr.Substring(l - 19, 1) == "1")//小车外限位预警
            {
                data.Current.OutAlarm_Warn = "1";
            }
            else
            {
                data.Current.OutAlarm_Warn = "0";
            }
            if (WarnStr.Substring(l - 20, 1) == "1")//小车内限位预警
            {
                data.Current.InAlarm_Warn = "1";
            }
            else
            {
                data.Current.InAlarm_Warn = "0";
            }
            if (WarnStr.Substring(l - 21, 1) == "1")//左转限位预警
            {
                data.Current.LeftAlarm_Warn = "1";
            }
            else
            {
                data.Current.LeftAlarm_Warn = "0";
            }
            if (WarnStr.Substring(l - 22, 1) == "1")//右转限位预警
            {
                data.Current.RightAlarm_Warn = "1";
            }
            else
            {
                data.Current.RightAlarm_Warn = "0";
            }
        }
        /// <summary>
        /// 限位控制器状态
        /// </summary>
        public static void LimitFlag(GprsCraneDataObject data, string LimitStatus)
        {
            int l = LimitStatus.Length;
            if (LimitStatus.Substring(l - 5, 1) == "1")//高度上限位减速状态
            {
                data.Current.LimitUpStatue_sub = "1";
            }
            else
            {
                data.Current.LimitUpStatue_sub = "0";
            }
            if (LimitStatus.Substring(l - 4, 1) == "1")//回转右限位减速状态
            {
                data.Current.LimitRightStatue_sub = "1";
            }
            else
            {
                data.Current.LimitRightStatue_sub = "0";
            }
            if (LimitStatus.Substring(l - 3, 1) == "1")//回转右限位状态
            {
                data.Current.LimitRightStatue = "1";
            }
            else
            {
                data.Current.LimitRightStatue = "0";
            }
            if (LimitStatus.Substring(l - 2, 1) == "1")//回转左限位减速状态
            {
                data.Current.LimitLeftStatue_sub = "1";
            }
            else
            {
                data.Current.LimitLeftStatue_sub = "0";
            }
            if (LimitStatus.Substring(l - 1, 1) == "1")//回转左限位状态
            {
                data.Current.LimitLeftStatue = "1";
            }
            else
            {
                data.Current.LimitLeftStatue = "0";
            }
            if (LimitStatus.Substring(l - 6, 1) == "1")//高度上限位状态
            {
                data.Current.LimitUpStatue = "1";
            }
            else
            {
                data.Current.LimitUpStatue = "0";
            }
            if (LimitStatus.Substring(l - 7, 1) == "1")//高度下限位状态
            {
                data.Current.LimitDownStatue = "1";
            }
            else
            {
                data.Current.LimitDownStatue = "0";
            }
            if (LimitStatus.Substring(l - 8, 1) == "1")//高度下限位换速状态
            {
                data.Current.LimitDownStatue_sub = "1";
            }
            else
            {
                data.Current.LimitDownStatue_sub = "0";
            }
            if (LimitStatus.Substring(l - 9, 1) == "1")//幅度外预减速状态
            {
                data.Current.LimitOutStatue_sub = "1";
            }
            else
            {
                data.Current.LimitOutStatue_sub = "0";
            }
            if (LimitStatus.Substring(l - 10, 1) == "1")//幅度外限位状态
            {
                data.Current.LimitOutStatue = "1";
            }
            else
            {
                data.Current.LimitOutStatue = "0";
            }
            if (LimitStatus.Substring(l - 11, 1) == "1")//幅度内限位状态
            {
                data.Current.LimitInStatue = "1";
            }
            else
            {
                data.Current.LimitInStatue = "0";
            }
            if (LimitStatus.Substring(l - 12, 1) == "1")//幅度内换速状态
            {
                data.Current.LimitInStatue_sub = "1";
            }
            else
            {
                data.Current.LimitInStatue_sub = "0";
            }
            if (LimitStatus.Substring(l - 13, 1) == "1")//风速预警限位状态
            {
                data.Current.LimitWindStatue_sub = "1";
            }
            else
            {
                data.Current.LimitWindStatue_sub = "0";
            }
            if (LimitStatus.Substring(l - 16, 1) == "1")//风速报警限位状态
            {
                data.Current.LimitWindStatue = "1";
            }
            else
            {
                data.Current.LimitWindStatue = "0";
            }
        }
        /// <summary>
        /// 报警短信
        /// </summary>
        //public static void AlarmMessge(GprsCraneDataObject data)
        //{
        //    if (data.Current.Type == "0")//无报警时 终止
        //    {
        //        return;
        //    }
        //    #region 发送报警短信
        //    //如果当前报警时间  与 上次存入数据库时间 相差 大于60秒 才再次存入数据库
        //    try
        //    {
        //        IGprsCraneDataComm gd = new GprsDataComm().CreateInstance();
        //        //吊重，风速，碰撞，力矩，倾斜
        //        string[] Alarm = { data.Current.WeightAlarm, data.Current.WindAlarm, data.Current.HitAlarm, data.Current.TorqueAlarm, data.Current.AngleAlarm };
        //        int Times = gd.GetOnceAlarmTime(data.Current.Craneno, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Alarm);

        //        if (Times < 300 && Times != -1)
        //            return;

        //        System.Data.DataTable dtProj = gd.GetCraneProj(data.Current.Craneno);

        //        if (dtProj == null || dtProj.Rows.Count < 0)
        //            return;

        //        if (dtProj.Rows[0]["IsMsg"].ToString() != "1")
        //            return;
        //        string Telephone = dtProj.Rows[0]["PrincipalTel"].ToString();
        //        if (Telephone.Length != 11 && Telephone.IndexOf(',') == -1)
        //        {
        //            Telephone = "";
        //        }
        //        //只有吊重，风速，力矩  ZT20170510更改
        //        if (data.Current.WeightAlarm == "2" || data.Current.WindAlarm == "2" || data.Current.TorqueAlarm == "2" || data.Current.HitAlarm == "2" || data.Current.AngleAlarm == "2")
        //        {
        //            //短信列表
        //            lock (MsgList.LsMsg)
        //            {
        //                string SendStr = Telephone + "@" + "工地:" + dtProj.Rows[0]["ProName"].ToString() + " 设备编号:" + data.Current.Craneno + " 时间:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 报警类型:" + (data.Current.TorqueAlarm == "2" ? "力矩 " : "") + (data.Current.WeightAlarm == "2" ? "超重 " : "") + (data.Current.HitAlarm == "2" ? "碰撞 " : "") + (data.Current.AngleAlarm == "2" ? "倾斜 " : "") + (data.Current.WindAlarm == "2" ? "风速 " : "");
        //                MsgList.InsertCacheLs(SendStr);
        //                try
        //                {
        //                    ToolAPI.XMLOperation.WriteLogXmlNoTail("短信写入缓存", SendStr);
        //                }
        //                catch (Exception) { }

        //                //MsgList.InsertCacheLs("17010054624" + "@" + "工地:" + "北京测试" + " 设备编号:" + "399901" + " 时间:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 报警类型:应该是风速");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //    #endregion
        //}

        #endregion

        #region 心跳
        /// <summary>
        /// 心跳
        /// </summary>
        /// <param name="b"></param>
        /// 发送7e 7e 0e 00 00 12 34 56 17 09 27 10 10 10 00 7d 7d
        /// 接收7e 7e 0e 00 07 01 17 09 27 10 38 42 00 7d 7d 
        private static byte[] OnResolveHeabert(byte[] b, int bCount, ref DBFrame df)
        {
            DateTime dt = DateTime.Now;
            if (bCount != 17)
            {
                return null;
            }
            GprsCraneDataObject data = new GprsCraneDataObject();

            string tStr = ConvertData.ToHexString(b, 0, 2);
            if (tStr != "7E7E")
                return null;
            //设备号
            data.Heartbeat.SN = ConvertData.ToHexString(b, 5, 3);
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
                data.Heartbeat.OnlineTime = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
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
                data.Heartbeat.OnlineTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            data.Current.Rtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            rb[0] = 0x7E;
            rb[1] = 0x7E;
            rb[2] = 0x0E;
            rb[3] = 0x00;//功能码
            //存入数据库
            df.deviceid = data.Heartbeat.SN;
            df.datatype = "heartbeat";
            df.contentjson = JsonConvert.SerializeObject(data.Heartbeat);


            string sourId = data.Heartbeat.SN;
            //数据库的拷贝
            if (!string.IsNullOrEmpty(MainStatic.DeviceCopy_TowerCrane))
            {
                if (MainStatic.DeviceCopy_TowerCrane.Contains(sourId + "#"))
                {
                    try
                    {
                        string[] strary = MainStatic.DeviceCopy_TowerCrane.Split(';');
                        foreach (string dev in strary)
                        {
                            if (dev.Contains(sourId + "#"))
                            {
                                string[] devcopy = dev.Split('#');
                                data.Heartbeat.SN  = devcopy[1];
                                DBFrame dfcopy = DBFrame.DeepCopy(df);
                                dfcopy.deviceid = devcopy[1];
                                dfcopy.datatype = "heartbeat";
                                dfcopy.contentjson = JsonConvert.SerializeObject(data.Heartbeat);
                                if (dfcopy.contentjson != null && dfcopy.contentjson != "")
                                {
                                    DB_MysqlTowerCrane.SaveTowerCrane(dfcopy);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                }
            }
            

            return rb;
        }
        #endregion

        #region IP更改相关
        /// <summary>
        /// 发送修改IP指令后设备应答
        /// </summary>
        /// <param name="b"></param>
        /// <param name="bCount"></param>
        /// <returns></returns>
        private static byte[] OnResolveIpAck(byte[] b, int bCount)
        {
            if (bCount != 12)
            {
                return null;
            }
            string tStr = ConvertData.ToHexString(b, 0, 2);
            if (tStr != "7E7E")
                return null;
            //设备号
            string craneNo = ConvertData.ToHexString(b, 5, 3);
            if (b[8] == 0x01)
            {
                DB_MysqlTowerCrane.UpdateDataCongfig(craneNo, 2,true);
                DB_MysqlTowerCrane.UpdateIPCommandIssued(craneNo, 2);

            }
            else if (b[8] == 0x00)
            {
                DB_MysqlTowerCrane.UpdateDataCongfig(craneNo, 3,false);
                DB_MysqlTowerCrane.UpdateIPCommandIssued(craneNo, 3);


            }
            ToolAPI.XMLOperation.WriteLogXmlNoTail("GprsResolveDataV41.IP应答", string.Format("{0},{1}", craneNo, ConvertData.ToHexString(b, 0, b.Length)));
            return null;
        }
        #endregion

        #region 限位控制相关
        /// <summary>
        /// 限位控制
        /// </summary>
        /// <param name="b"></param>
        /// <param name="bCount"></param>
        /// <returns></returns>
        private static byte[] OnResolveControlAck(byte[] b, int bCount)
        {
            if (bCount != 15)
            {
                return null;
            }
            string tStr = ConvertData.ToHexString(b, 0, 2);
            if (tStr != "7E7E")
                return null;
            //设备号
            string craneNo = ConvertData.ToHexString(b, 5, 3);
            DB_MysqlTowerCrane.UpdateControlCongfig(craneNo);
            return null;
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
        private static byte[] OnResolveParamDataUpload(byte[] b, int bCount, ref DBFrame df)
        {
            #region 解析
            string tStr = ConvertData.ToHexString(b, 0, 2);
            if (tStr != "7E7E")
                return null;
            GprsCraneDataObject data = new GprsCraneDataObject();

            //设备号
            data.CraneConfig.craneNo = ConvertData.ToHexString(b, 5, 3);
            //日期
            tStr = ConvertData.ToHexString(b, 8, 6);
            try
            {
                data.CraneConfig.SetTime = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch
            {
                data.CraneConfig.SetTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            // 倍率
            data.CraneConfig.ratio = Convert.ToInt32(b[14]).ToString();
            UShortValue s = new UShortValue();
            //最小高度时 AD 采样值
            s.bValue1 = b[15];
            s.bValue2 = b[16];
            data.CraneConfig.minHighAD = s.sValue.ToString("0.00");
            //最大高度时 AD 采样值
            s.bValue1 = b[17];
            s.bValue2 = b[18];
            data.CraneConfig.maxHighAD = s.sValue.ToString("0.00");
            //标准尺长度
            s.bValue1 = b[19];
            s.bValue2 = b[20];
            data.CraneConfig.standardScale = (s.sValue / 10.00).ToString("0.00");
            //最小幅度
            s.bValue1 = b[21];
            s.bValue2 = b[22];
            data.CraneConfig.minAmplitude = (s.sValue / 10.00).ToString("0.00");
            //最小幅度时 AD 采样值
            s.bValue1 = b[23];
            s.bValue2 = b[24];
            data.CraneConfig.minAmplitudeAD = s.sValue.ToString("0.00");
            //最大幅度
            s.bValue1 = b[25];
            s.bValue2 = b[26];
            data.CraneConfig.maxAmplitude = (s.sValue / 10.00).ToString("0.00");
            //最大幅度时 AD 采样值
            s.bValue1 = b[27];
            s.bValue2 = b[28];
            data.CraneConfig.maxAmplitudeAD = s.sValue.ToString("0.00");
            //空钩时 AD 采样值
            s.bValue1 = b[29];
            s.bValue2 = b[30];
            data.CraneConfig.emptyhookAD = s.sValue.ToString("0.00");
            //吊载砝码时 AD 采样值
            s.bValue1 = b[31];
            s.bValue2 = b[32];
            data.CraneConfig.loadWeightAD = s.sValue.ToString("0.00");
            //砝码重量
            s.bValue1 = b[33];
            s.bValue2 = b[34];
            data.CraneConfig.farmarWeight = s.sValue.ToString("0.00");
            //回转类型
            data.CraneConfig.rotaryType = Convert.ToInt32(b[35]).ToString();
            //绝对值回转方向
            data.CraneConfig.absTurnDirection = Convert.ToInt32(b[36]).ToString();

            IntValue iv = new IntValue();
            //绝对值回转值
            iv.bValue1 = b[37];
            iv.bValue2 = b[38];
            iv.bValue3 = b[39];
            iv.bValue4 = b[40];
            data.CraneConfig.absTurnValue = iv.iValue.ToString("0.00");
            //绝对值回转点确认后的回转值
            iv.bValue1 = b[41];
            iv.bValue2 = b[42];
            iv.bValue3 = b[43];
            iv.bValue4 = b[44];
            data.CraneConfig.absTurnPointValue = iv.iValue.ToString("0.00");
            //电位器回转左限位 AD 值
            s.bValue1 = b[45];
            s.bValue2 = b[46];
            data.CraneConfig.potLeftLimitAD = s.sValue.ToString("0.00");
            //电位器回转右限位 AD 值
            s.bValue1 = b[47];
            s.bValue2 = b[48];
            data.CraneConfig.potRightLimitAD = s.sValue.ToString("0.00");
            //电位器回转左右限位角度和
            s.bValue1 = b[49];
            s.bValue2 = b[50];
            data.CraneConfig.potLimitAngle = s.sValue.ToString("0.00");
            //4 倍率时最大起重量
            s.bValue1 = b[51];
            s.bValue2 = b[52];
            data.CraneConfig.liftWeight4Ratio = s.sValue.ToString("0.00");
            //4 倍率时最大起重量幅度
            s.bValue1 = b[53];
            s.bValue2 = b[54];
            data.CraneConfig.liftWeightRange4R = (s.sValue / 10.00).ToString("0.00");
            //4 倍率时最大幅度
            s.bValue1 = b[55];
            s.bValue2 = b[56];
            data.CraneConfig.maxRange4Ratio = (s.sValue / 10.00).ToString("0.00");
            //4 倍率时最大幅度起重量
            s.bValue1 = b[57];
            s.bValue2 = b[58];
            data.CraneConfig.maxRangeWeight4R = s.sValue.ToString("0.00");
            //2 倍率时最大起重量
            s.bValue1 = b[59];
            s.bValue2 = b[60];
            data.CraneConfig.liftWeight2Ratio = s.sValue.ToString("0.00");
            //2 倍率时最大起重量幅度
            s.bValue1 = b[61];
            s.bValue2 = b[62];
            data.CraneConfig.liftWeightRange2R = (s.sValue / 10.00).ToString("0.00");
            //2 倍率时最大幅度
            s.bValue1 = b[63];
            s.bValue2 = b[64];
            data.CraneConfig.maxRange2Ratio = (s.sValue / 10.00).ToString("0.00");
            //2 倍率时最大幅度起重量
            s.bValue1 = b[65];
            s.bValue2 = b[66];
            data.CraneConfig.maxRangeWeight2R = s.sValue.ToString("0.00");
            //ZIGBEE 本机编号
            data.CraneConfig.zigbeeLocalNo = Convert.ToInt32(b[67]).ToString();
            //ZIGBEE 本机频道号
            data.CraneConfig.zigbeeChannelNo = Convert.ToInt32(b[68]).ToString();
            //ZIGBEE 本机组号
            data.CraneConfig.zigbeeGroupNo = Convert.ToInt32(b[69]).ToString();
            //防碰撞信息本机 X
            s.bValue1 = b[70];
            s.bValue2 = b[71];
            data.CraneConfig.antiCollisionX = s.sValue.ToString("0.00");
            //防碰撞信息本机 Y
            s.bValue1 = b[72];
            s.bValue2 = b[73];
            data.CraneConfig.antiCollisionY = s.sValue.ToString("0.00");
            //起重臂长
            s.bValue1 = b[74];
            s.bValue2 = b[75];
            data.CraneConfig.liftWeightArmLenght = s.sValue.ToString("0.00");
            //平衡臂长
            s.bValue1 = b[76];
            s.bValue2 = b[77];
            data.CraneConfig.balanceArmLenght = s.sValue.ToString("0.00");
            //塔身高度
            s.bValue1 = b[78];
            s.bValue2 = b[79];
            data.CraneConfig.towerHeight = s.sValue.ToString("0.00");
            //塔冒高度
            s.bValue1 = b[80];
            s.bValue2 = b[81];
            data.CraneConfig.towerAtHeight = s.sValue.ToString("0.00");
            //幅度减速值
            s.bValue1 = b[82];
            s.bValue2 = b[83];
            data.CraneConfig.ampReductionValue = (s.sValue / 10.00).ToString("0.00");
            //幅度限速值
            s.bValue1 = b[84];
            s.bValue2 = b[85];
            data.CraneConfig.ampRestrictValue = (s.sValue / 10.00).ToString("0.00");
            //高度减速值
            s.bValue1 = b[86];
            s.bValue2 = b[87];
            data.CraneConfig.highReductionValue = (s.sValue / 10.00).ToString("0.00");
            //高度限速值
            s.bValue1 = b[88];
            s.bValue2 = b[89];
            data.CraneConfig.highRestrictValue = (s.sValue / 10.00).ToString("0.00");
            //回转减速值
            s.bValue1 = b[90];
            s.bValue2 = b[91];
            data.CraneConfig.turnReducionValue = s.sValue.ToString("0.00");
            //回转限位值
            s.bValue1 = b[92];
            s.bValue2 = b[93];
            data.CraneConfig.turnRestrictValue = s.sValue.ToString("0.00");
            //区域保护减速值
            s.bValue1 = b[94];
            s.bValue2 = b[95];
            data.CraneConfig.areaReductionValue = (s.sValue / 10.00).ToString("0.00");//2017年11月14号除以10
            //区域保护限位值
            s.bValue1 = b[96];
            s.bValue2 = b[97];
            data.CraneConfig.areaRestrictValue = (s.sValue / 10.00).ToString("0.00");//2017年11月14号除以10
            //防碰撞减速值
            s.bValue1 = b[98];
            s.bValue2 = b[99];
            data.CraneConfig.acReductionValue = (s.sValue / 10.00).ToString("0.00");
            //防碰撞限位值
            s.bValue1 = b[100];
            s.bValue2 = b[101];
            data.CraneConfig.acRestrictValue = (s.sValue / 10.00).ToString("0.00");
            //换速力矩
            data.CraneConfig.throwOverTorque = Convert.ToInt32(b[102]).ToString();
            //切断力矩
            data.CraneConfig.cutTorque = Convert.ToInt32(b[103]).ToString();
            //换速重量
            data.CraneConfig.throwOverWeight = Convert.ToInt32(b[104]).ToString();
            //切断重量
            data.CraneConfig.cutWeight = Convert.ToInt32(b[105]).ToString();
            int i = bCount - 106 > 0 ? bCount - 106 : 0;
            if (i > 3)
            {
                i = i - 3;
            }
            byte[] va = new byte[i];
            for (int ss = 0; ss < i; ss++)
            {
                va[ss] = b[106 + ss];
            }
            data.CraneConfig.softVersion = System.Text.Encoding.ASCII.GetString(va);



            #endregion

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

            //存入数据库
            df.deviceid = data.CraneConfig.craneNo;
            df.datatype = "parameterUpload";
            df.contentjson = JsonConvert.SerializeObject(data.CraneConfig);


            string sourId = data.CraneConfig.craneNo;
            //数据库的拷贝
            if (!string.IsNullOrEmpty(MainStatic.DeviceCopy_TowerCrane))
            {
                if (MainStatic.DeviceCopy_TowerCrane.Contains(sourId + "#"))
                {
                    try
                    {
                        string[] strary = MainStatic.DeviceCopy_TowerCrane.Split(';');
                        foreach (string dev in strary)
                        {
                            if (dev.Contains(sourId + "#"))
                            {
                                string[] devcopy = dev.Split('#');
                                data.CraneConfig.craneNo = devcopy[1];
                                DBFrame dfcopy = DBFrame.DeepCopy(df);
                                dfcopy.deviceid = devcopy[1];
                                dfcopy.datatype = "parameterUpload";
                                dfcopy.contentjson = JsonConvert.SerializeObject(data.CraneConfig);
                                if (dfcopy.contentjson != null && dfcopy.contentjson != "")
                                {
                                    DB_MysqlTowerCrane.SaveTowerCrane(dfcopy);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                }
            }
            return rb;
            #endregion

        }
        #endregion

        #region 运行时间
        /// <summary>
        /// 运行时间
        /// </summary>
        /// <param name="b"></param>
        /// <param name="bCount"></param>
        /// <returns></returns>
        /// 发送7e 7e 0e 07 00 12 34 56  01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 7d 7d
        /// 接收7e 7e 0e 07 01 01 00 7d 7d 
        private static byte[] OnResolveRunTimeAck(byte[] b, int bCount, ref DBFrame df)
        {
            if (bCount != 29)
            {
                return null;
            }
            string tStr = ConvertData.ToHexString(b, 0, 2);
            if (tStr != "7E7E")
                return null;
            GprsCraneDataObject data = new GprsCraneDataObject();
            //设备号
            data.CraneRuntime.CraneNo = ConvertData.ToHexString(b, 5, 3);
            IntValue iv = new IntValue();
            iv.bValue1 = b[8];
            iv.bValue2 = b[9];
            iv.bValue3 = b[10];
            iv.bValue4 = b[11];
            data.CraneRuntime.Run_second = iv.iValue.ToString();
            iv.bValue1 = b[12];
            iv.bValue2 = b[13];
            iv.bValue3 = b[14];
            iv.bValue4 = b[15];
            data.CraneRuntime.Run_second_sum = iv.iValue.ToString();
            //写入数据库
            #region 应答
            byte[] rb = new byte[9];
            rb[0] = 0x7E;
            rb[1] = 0x7E;
            rb[2] = 0x0E;
            rb[3] = 0x07;
            rb[4] = 0x01;//应用数据区数据长度
            rb[5] = 0x01;//0x01：接收成功
            rb[7] = 0x7D;
            rb[8] = 0x7D;

            //存入数据库
            df.deviceid = data.CraneRuntime.CraneNo;
            df.datatype = "runtimeEp";
            df.contentjson = JsonConvert.SerializeObject(data.CraneRuntime);
            return rb;
            #endregion
        }

        #endregion

        #region 身份验证
        //身份验证解析入口
        private static void AuthenticationDispose(byte[] b, int c, TcpSocketClient client)
        {
            Authentication AuthenticationTemp = AuthenticationAnalysis(b, c);
            try
            {
                #region 需要验证  //ZT20160909添加身份验证的使用与否
                if (MainStatic.IsAuthentication.Equals("true"))
                {
                    if (b[8] == 0x00 && b[13] == 0x02)//司机卡删后设备应答帧   第8字节表示登录状态，第13字节表示操作标识
                    {
                        PlatformAuthentication(AuthenticationTemp, 3);
                    }
                    else if (b[8] == 0x00 && b[13] == 0x00)//身份认证
                    {
                        byte result = PlatformAuthentication(AuthenticationTemp, 2);
                        if (result > 1)
                            result = 1;
                        byte[] rb = OnResolveAuthentication(b, 0, result);
                        if (rb != null)
                            client.SendBuffer(rb);
                    }
                    else if (b[8] == 0x01 || b[8] == 0x02)//上班或下班
                    {
                        //数据库比对是否存在此司机卡号
                        if (PlatformAuthentication(AuthenticationTemp, 1) == 1)
                        {
                            //打卡上下班
                            byte[] rb;
                            if (AuthenticationTemp.Status == 2)//下班
                            {
                                AuthenticationTemp.Status = 0;
                                byte result = PlatformAuthentication(AuthenticationTemp, 4);
                                if (result > 1)
                                    result = 1;
                                rb = OnResolveAuthentication(b, 2, result);
                            }
                            else
                            {
                                byte result = PlatformAuthentication(AuthenticationTemp, 4);
                                if (result > 1)
                                    result = 1;
                                rb = OnResolveAuthentication(b, 1, result);
                            }
                            if (rb != null)
                                client.SendBuffer(rb);
                        }
                        else
                        {
                            //删除设备信息下发操作
                            byte[] responseData = OnResolveAuthentication(b, 3, 0);
                            if (responseData != null)
                                client.SendBuffer(responseData);
                        }
                    }
                    //人脸识别
                    else if (b[8] >= 3 && b[8] <= 5)
                    {
                        if (AuthenticationTemp == null || (AuthenticationTemp != null && AuthenticationTemp.empNo == null) || (AuthenticationTemp != null && AuthenticationTemp.empNo != null && AuthenticationTemp.empNo.Length != 8))
                        {
                            //返回不存在
                            byte[] responseData = OnResolveAuthenticationByAuthentication(b, 0, AuthenticationTemp);
                            if (responseData != null)
                                client.SendBuffer(responseData);
                        }
                        else
                        {
                            //坐记录 20170217

                            PlatformAuthentication(AuthenticationTemp, 4);
                            //返回存在
                            byte[] responseData = OnResolveAuthenticationByAuthentication(b, 1, AuthenticationTemp);
                            if (responseData != null)
                                client.SendBuffer(responseData);
                        }

                    }
                    else
                    {
                        return;
                    }
                }
                #endregion
                #region 不需要验证  //ZT20160909添加身份验证的使用与否
                else
                {
                    if (b[8] == 0x00 && b[13] == 0x00)//身份验证
                    {
                        byte[] rb = OnResolveAuthentication(b, 0, 1);
                        if (rb != null)
                            client.SendBuffer(rb);
                    }
                    else if (b[8] == 0x01 || b[8] == 0x02)//上下班状态
                    {
                        if (AuthenticationTemp.Status == 2)//下班
                        {
                            byte[] rb = OnResolveAuthentication(b, 2, 1);
                            if (rb != null)
                                client.SendBuffer(rb);
                        }
                        else
                        {
                            byte[] rb = OnResolveAuthentication(b, 1, 1);
                            if (rb != null)
                                client.SendBuffer(rb);
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                if (b[8] == 0x00 && b[13] == 0x00)//身份验证
                {
                    byte[] rb = OnResolveAuthentication(b, 0, 1);
                    if (rb != null)
                        client.SendBuffer(rb);
                }
                else if (b[8] == 0x01 || b[8] == 0x02)//上下班状态
                {
                    if (AuthenticationTemp.Status == 2)//下班
                    {
                        byte[] rb = OnResolveAuthentication(b, 2, 1);
                        if (rb != null)
                            client.SendBuffer(rb);
                    }
                    else    //下班
                    {
                        byte[] rb = OnResolveAuthentication(b, 1, 1);
                        if (rb != null)
                            client.SendBuffer(rb);
                    }
                }
            }
        }
        /// <summary>
        /// 平台数据库进行处理
        /// 命令 1是否存在司机卡号 2身份验证  3 删除卡号 4上下班
        /// </summary>
        /// <param name="AuthenticationTemp">身份验证对象</param>
        /// <returns></returns>
        private static byte PlatformAuthentication(Authentication AuthenticationTemp, int Commod)
        {
            #region 对应数据库操作
            switch (Commod)
            {
                case 1: return DB_MysqlTowerCrane.IsExistCard(AuthenticationTemp.SN,AuthenticationTemp.KardID);//是否存在司机卡号
                case 2: return DB_MysqlTowerCrane.IsExistCard(AuthenticationTemp.SN, AuthenticationTemp.KardID);//身份验证
                case 3: DB_MysqlTowerCrane.UpdateIdentifyCurrent(AuthenticationTemp.SN, AuthenticationTemp.KardID); break;//删除卡号
                case 4: return (byte)DB_MysqlTowerCrane.Pro_Authentication(AuthenticationTemp);//上班下班
            }
            #endregion
            return 0;
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
        /// <summary>
        /// 服务应答（与设备的交互）
        ///type 3身份认证及进入  4人脸退出  5人脸信息删除
        /// </summary>
        /// <returns></returns>
        private static byte[] OnResolveAuthenticationByAuthentication(byte[] b, byte result, Authentication AuthenticationTemp)
        {
            //数据原路返回
            if (b.Length > 17)
            {
                byte[] rb = new byte[20];
                for (int i = 0; i < 9; i++)
                    rb[i] = b[i];
                //帧长度
                rb[4] = 0x0C;
                //司机工号
                if (result == 0)//默认填充
                {
                    for (int i = 0; i < 8; i++)
                    {
                        rb[9 + i] = 0xff;
                    }
                }
                else//填写工号
                {
                    try
                    {
                        byte[] byteArray = System.Text.Encoding.Default.GetBytes(AuthenticationTemp.empNo);
                        for (int i = 0; i < 8; i++)
                        {
                            rb[9 + i] = byteArray[i];
                        }
                    }
                    catch (Exception)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            rb[9 + i] = 0xff;
                        }
                    }
                }
                rb[17] = 0x00; //校验和 下位机暂未处理，可以默认值处理
                rb[18] = 0x7d;
                rb[19] = 0x7d;
                return rb;
            }
            return null;
        }
        /// <summary>
        /// 协议解析
        /// </summary>
        /// <returns></returns>
        private static Authentication AuthenticationAnalysis(byte[] b, int bCount)
        {
            try
            {
                #region 解析
                //if (bCount != 23 && bCount != 17 && bCount != 33&& bCount!=36)
                //    return null;
                string tStr = ConvertData.ToHexString(b, 0, 2);
                if (tStr != "7E7E")
                    return null;
                Authentication au = new Authentication();
                //设备编号
                au.SN = ConvertData.ToHexString(b, 5, 3);
                byte loginState = b[8];
                if (loginState < 3) //身份验证，上班，下班。
                {
                    au.isFace = false;
                    au.KardID = ConvertData.ToHexString(b, 9, 4);
                    //日期
                    tStr = ConvertData.ToHexString(b, 14, 6);
                    try
                    {
                        au.OnlineTime = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    catch
                    {
                        au.OnlineTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    //au.OnlineTime = ConvertData.ToHexString(b, 5, 3);
                    au.Status = Convert.ToInt32(b[8]);
                    return au;
                }
                else//人脸认证及进入，人脸退出，人脸信息删除。
                {
                    au.isFace = true;
                    byte[] IDCard = new byte[18];
                    for (int l = 9, j = 0; l < 27; l++, j++)
                    {
                        IDCard[j] = b[l];
                    }
                    au.IDCard = Encoding.ASCII.GetString(IDCard);
                    //访问数据库
                    DataTable dt = DB_MysqlTowerCrane.GetDriverInfoByIDCard(au.SN,au.IDCard);  //获取司机相关信息
                    if (dt.Rows.Count > 0)
                    {
                        au.empNo = dt.Rows[0]["empNo"].ToString().Trim();  //工号
                        au.KardID = dt.Rows[0]["empNo"].ToString().Trim();  //工号
                        au.OnlineTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        au.Status = 0;
                        return au;
                    }
                    else
                    {
                        return null;
                    }
                    return null;
                }
                #endregion
            }
            catch (Exception)
            {
                return null;
            }
        }
        #endregion

        #region 向设备发送命令
        /// <summary>
        /// 向终端设备发送设置IP的数据
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static byte[] Byte_IP(DataRow dr)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                ms.WriteByte(0x7E);
                ms.WriteByte(0x7E);
                ms.WriteByte(0x0E);
                ms.WriteByte(0x05);

                byte[] ip = Encoding.ASCII.GetBytes(dr["ip_dn"].ToString());
                byte[] port = Encoding.ASCII.GetBytes(dr["port"].ToString());
                ms.WriteByte((byte)(ip.Length + port.Length + 2));

                ms.WriteByte((byte)ip.Length);
                ms.Write(ip, 0, ip.Length);

                ms.WriteByte((byte)port.Length);
                ms.Write(port, 0, port.Length);

                ms.WriteByte(0x01);
                ms.WriteByte(0x7D);
                ms.WriteByte(0x7D);
                return ms.ToArray();
            }
        }
        /// <summary>
        /// 向终端设备发送删除司机卡信息
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static byte[] Byte_DelCard(DataRow dr)
        {
            string craneNo = dr["nodeID"].ToString();
            byte[] rb = new byte[17];
            rb[0] = 0x7E;
            rb[1] = 0x7E;
            rb[2] = 0x0E;
            rb[3] = 0x02;
            rb[4] = 0x9;//应用数据区数据长度
            rb[5] = Convert.ToByte(craneNo.Substring(0, 2), 16);
            rb[6] = Convert.ToByte(craneNo.Substring(2, 2), 16);
            rb[7] = Convert.ToByte(craneNo.Substring(4, 2), 16);
            rb[8] = 0x00;
            string dn = dr["IcCode"].ToString();
            for (int current = 0; current < 4; current++)
            {
                rb[9 + current] = Convert.ToByte(dn.Substring(current * 2, 2), 16);
            }
            rb[13] = 0x02;
            rb[15] = 0x7D;
            rb[16] = 0x7D;
            return rb;
        }
        /// <summary>
        /// 向终端设备发送远程控制
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static byte[] Byte_Control(DataRow dr)
        {
            byte[] rb = new byte[12];
            rb[0] = 0x7E;
            rb[1] = 0x7E;
            rb[2] = 0x0E;
            rb[3] = 0x06;
            rb[4] = 0x04;//应用数据区数据长度
            rb[5] = 0x00;
            rb[6] = 0x00;
            rb[7] = 0x00;
            int status = int.Parse((dr["kztype"] == null || dr["kztype"].ToString() == "") ? "1" : dr["kztype"].ToString());
            if (status == 0)//控制
                rb[8] = 0x80;
            else//解除控制
                rb[8] = 0x00;
            rb[10] = 0x7D;
            rb[11] = 0x7D;
            return rb;
        }
        #endregion
        #endregion

    }

}
