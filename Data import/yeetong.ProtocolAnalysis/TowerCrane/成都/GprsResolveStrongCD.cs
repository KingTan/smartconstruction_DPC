using Architecture;
using ProtocolAnalysis.Tool;
using ProtocolAnalysis.TowerCrane.OE;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolAPI;

namespace ProtocolAnalysis.TowerCrane
{
    public static class GprsResolveStrongCD
    {
        static Dictionary<string, string> dic_device = new Dictionary<string, string>();
        /// <summary>
        /// 实时数据
        /// </summary>
        /// <param name="b"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static string OnResolveRecvCurrentMessage(byte[] b, UdpState client)
        {
            //判断帧类型  Convert.ToString(d, 2)
            string Frame = Convert.ToString(Convert.ToInt16(b[3]), 2).PadLeft(8, '0') + Convert.ToString(Convert.ToInt16(b[4]), 2).PadLeft(8, '0');
            //信息段长度（截取）string.Format("{0:x}",Convert.ToInt32(Frame.Substring(5),2)
            string length = string.Format("{0:x}", Convert.ToInt32(Frame.Substring(5), 2));
            DBFrame df = new DBFrame();
            df.contenthex = ConvertData.ToHexString(b, 0, b.Length);
            df.version = "7E7E0E";
            ToolAPI.XMLOperation.WriteLogXmlNoTail(System.Windows.Forms.Application.StartupPath + @"\Crane", "Crane数据原包", df.contenthex);
            OnResolve_Current(b, ref df);//保存实时数据
            ToolAPI.XMLOperation.WriteLogXmlNoTail(System.Windows.Forms.Application.StartupPath + @"\GOYO", "GOYO设备数据原包", "333333333333333333333333");
            try
            {
                UdpSever.SendMsgStr(client, "FEFB4020028AA53200747DFEFC");
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("OnResolveRecvMessage-->返回信息异常", ex.Message);
            }
            return "";
        }

        /// <summary>
        /// 信息传输
        /// </summary>
        /// <param name="b"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static string OnResolveRecvEquipmentMessage(byte[] b, UdpState client)
        {
            //FEFB
            //40
            //2011
            //8AA532 设备代码
            //0000000000000000000000363132383836   信息段
            //747D
            //FEFA   29
            string byteStr=ToHexString(b);
            //判断帧类型  Convert.ToString(d, 2)
            string Frame = Convert.ToString(Convert.ToInt16(b[3]), 2).PadLeft(8, '0') + Convert.ToString(Convert.ToInt16(b[4]), 2).PadLeft(8, '0');
            //信息段长度（截取）string.Format("{0:x}",Convert.ToInt32(Frame.Substring(5),2)
            string length = string.Format("{0:x}", Convert.ToInt32(Frame.Substring(5), 2));
            //设备代码和设备号
            //设备号---从集合里获取
            byte[] t = new byte[6];
            for (int x = 19, j = 0; x < 25; x++, j++)
            {
                t[j] = b[x];
            }
            string devNumber= byteStr.Substring(10,6);
            string devCode= Encoding.ASCII.GetString(t);
            //保存和更新设备号对应的通信id
            if (dic_device.ContainsKey(devNumber))//存在--更新
            {
                
                dic_device[devNumber] = devCode;
            }
            else//不存在--添加
            {
                dic_device.Add(devNumber, devCode);
            }
            try
            {
                UdpSever.SendMsgStr(client, "FEFB4020028AA53200747DFEFC");
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("OnResolveRecvMessage-->返回信息异常", ex.Message);
            }
            return "";
        }
        /// <summary>
        /// 实时数据
        /// </summary>
        /// <param name="b"></param>
        /// <param name="bCount"></param>
        /// <param name="df"></param>
        private static void OnResolve_Current(byte[] b, ref DBFrame df)
        {
            try
            {
                string byteStr = ToHexString(b);
                string devNumber = byteStr.Substring(10, 6);//设备代码
                string str = ConvertData.ToHexString(b, 0, b.Length);
                
                GprsCraneDataObject data = new GprsCraneDataObject();

                #region 原协议
                //设备号---从集合里获取
                byte[] t = new byte[8];
                //for (int x = 6, j = 0; x < 14; x++, j++)
                //{
                //    t[j] = b[x];
                //}
                if (dic_device.ContainsKey(devNumber))
                {
                    data.Current.Craneno = dic_device[devNumber];
                }
                else
                {
                    return;
                }

                t = new byte[18];
                //司机卡号
                for (int x = 14, j = 0; x < 32; x++, j++)
                {
                    t[j] = 0;
                }
                data.Current.CardNo = Encoding.ASCII.GetString(t);
                //日期
                string tStr = ConvertData.ToHexString(b, 46, 8);
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
                iv.bValue1 = 0;
                iv.bValue2 = 0;
                iv.bValue3 = 0;
                iv.bValue4 = 0;
                data.Current.Longitude = (iv.iValue / 100000.0).ToString("0.00");
                //纬度
                iv.bValue1 = 0;
                iv.bValue2 = 0;
                iv.bValue3 = 0;
                iv.bValue4 = 0;
                data.Current.Latitude = (iv.iValue / 100000.0).ToString("0.00");
                UShortValue s = new UShortValue();
                //高度
                s.bValue1 = b[17];
                s.bValue2 = b[18];
                data.Current.Height = (s.sValue / 100.00).ToString("0.00");
                //幅度
                s.bValue1 = b[15];
                s.bValue2 = b[16];
                data.Current.Radius = (s.sValue / 100.00).ToString("0.00");
                ShortValue sv = new ShortValue();
                //转角
                sv.bValue1 = b[13];
                sv.bValue2 = b[14];
                data.Current.Angle = (sv.sValue / 10.00).ToString("0.00");
                //重量
                s.bValue1 = b[19];
                s.bValue2 = b[20];
                data.Current.Weight = (s.sValue / 100.00).ToString("0.00");
                //风速
                s.bValue1 = b[23];
                s.bValue2 = b[24];
                data.Current.Wind = (s.sValue / 100.00).ToString("0.00");
                data.Current.WindLevel = ConvertWind.WindToLeve(s.sValue / 100.0f).ToString();
                if (int.Parse(data.Current.WindLevel) > 13)
                    data.Current.Wind = "12";
                //倾角X
                sv.bValue1 = b[25];
                data.Current.AngleX = (sv.sValue / 100.00).ToString("0.00");
                //倾角Y
                sv.bValue1 = b[26];
                data.Current.AngleY = (sv.sValue / 100.00).ToString("0.00");
                //安全力矩
                s.bValue1 = b[21];
                data.Current.Safetorque = (s.sValue / 10.00).ToString("0.00");
                //安全起重量
                s.bValue1 = b[19];
                s.bValue2 = b[20];
                data.Current.SafeWeight = (s.sValue / 100.00).ToString("0.00");
                /*ZT20160923添加计算力矩*/
                data.Current.Torque = (double.Parse(data.Current.Weight) * double.Parse(data.Current.Radius)).ToString("0.00");
                //力矩百分比
                if (data.Current.Safetorque != "0.00")
                    data.Current.Torquepercent = ((double.Parse(data.Current.Weight) * double.Parse(data.Current.Radius)) / double.Parse(data.Current.Safetorque)).ToString("0.00");
                else
                    data.Current.Torquepercent = "0.00";
                //倍率
                //data.Current.Times = Convert.ToInt32(b[64]).ToString();
                //if (int.Parse(data.Current.Times) > 4)
                    data.Current.Times = "2";
                //限位控制器状态
                s.bValue1 = 0;
                s.bValue2 = 0;
                data.Current.LimitStatus = Convert.ToString(s.sValue, 2).PadLeft(16, '0');
                LimitFlag(data, data.Current.LimitStatus);

                //传感器状态
                s.bValue1 = 0;
                s.bValue2 = 0;
                data.Current.SensorStatus = Convert.ToString(s.sValue, 2).PadLeft(16, '0');

                //预警告码
                IntValue i = new IntValue();
                i.bValue1 = 0;
                i.bValue2 = 0;
                i.bValue3 = 0;
                i.bValue4 = 0;
                data.Current.WarnType = Convert.ToString(i.iValue, 2).PadLeft(32, '0');
                WarnFlag(data, data.Current.WarnType);
                //报警告码
                i.bValue1 = b[35];
                i.bValue2 = b[36];
                i.bValue3 = b[37];
                i.bValue4 = b[38];
                data.Current.AlarmType = Convert.ToString(i.iValue, 2).PadLeft(32, '0');//总共是32位右对齐
                AlarmFlag(data, data.Current.AlarmType); //报警码解析
                data.Current.WorkCircle = "0"; //注销不需要计算了工作循环了
                tStr = ConvertData.ToHexString(b, 84, 2);
                #endregion


                //存数据库
                //看看是否发送短信
                //存入数据库
                df.deviceid = data.Current.Craneno;
                df.datatype = "current";
                df.contentjson = JsonConvert.SerializeObject(data.Current);

                string sourId = data.Current.Craneno;
                //数据库的拷贝
                //存入数据库
                if (df.contentjson != null && df.contentjson != "")
                {
                    DB_MysqlTowerCrane.SaveTowerCrane(df);
                    ToolAPI.XMLOperation.WriteLogXmlNoTail("OnResolveRecvMessage-->收到参数配置应答", "应答内容：" + sourId +"-------------------"+ df.contenthex);
                }
            }
            catch (Exception ex) { XMLOperation.WriteLogXmlNoTail("转成都平台塔机实时数据错误信息", ex.Message); }
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
    }
}
