using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Threading;
using TCPAPI;
using ToolAPI;
using ProtocolAnalysis.TowerCrane.OE;
using ProtocolAnalysis.Tool;
using Newtonsoft.Json;
using Architecture;
using ProtocolAnalysis.TowerCrane;

namespace ProtocolAnalysis
{
    public static class GprsResolveDataV01
    {
        /// <summary>
        /// 解析、存储、显示数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static string OnResolveRecvMessage(byte[] b, int c, TcpSocketClient client)
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
                OnResolveRealData(b, c, ref df);
            }
            else if (typ == 0x02)//身份验证
            {
                typ = b[5];
                if (typ == 0x01)//刷卡请求
                {
                    byte[] rb = OnResolveAuthentication(b, c);
                    if (rb != null)
                        client.SendBuffer(rb);
                }
                if (typ == 0x03)//司机卡删后设备应答帧
                {
                   OnResolveIcDelAck(b, c);
                }
            }
            else if (typ == 0x03)//司机离线登录信息
            {
                byte[] rb = OnResolveDriverLogin(b, c);
                if (rb != null)
                    client.SendBuffer(rb);
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

        /// <summary>
        /// 实时数据
        /// </summary>
        /// <param name="b"></param>
        /// 7E7E010100390607000000001804191029080000000000000000000000000000000066667E419A99993E00370266662E420000000000000000000000000400367D7D
        public static Byte[] OnResolveRealData(byte[] b, int bCount,ref DBFrame df)
        {
            if (bCount != 66)
            {
                return null;
            }
            string tStr = ConvertData.ToHexString(b, 0, 2);
            if (tStr != "7E7E")
                return null;
            GprsCraneDataObject data = new GprsCraneDataObject();
            
            //设备号
            data.Current.Craneno = ConvertData.ToHexString(b, 5, 3);
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
            
            //传感器状态
            string m = Convert.ToString(b[26], 2).PadLeft(8, '0');
            //安全起重量
            UShortValue s = new UShortValue();
            s.bValue1 = b[32];
            s.bValue2 = b[33];
            string k = (s.sValue/100.00).ToString("0.00");

            //高度
            FloatValue f = new FloatValue();
            f.bValue1 = b[34];
            f.bValue2 = b[35];
            f.bValue3 = b[36];
            f.bValue4 = b[37];
            data.Current.Height = f.fValue.ToString("0.00");
            //重量
            f.bValue1 = b[38];
            f.bValue2 = b[39];
            f.bValue3 = b[40];
            f.bValue4 = b[41];
            data.Current.Weight = f.fValue.ToString("0.00");

            //风速等级
            data.Current.WindLevel = Convert.ToInt32(b[42]).ToString();
            if (int.Parse(data.Current.WindLevel) > 13)
                data.Current.WindLevel = "12";
            data.Current.Wind = ConvertWind.LeveToWind(Convert.ToInt32(data.Current.WindLevel)).ToString("0.00");

            //安全力矩
            s.bValue1 = b[43];
            s.bValue2 = b[44];
            data.Current.Safetorque = (s.sValue / 10.00).ToString("0.00");
            //幅度
            f.bValue1 = b[45];
            f.bValue2 = b[46];
            f.bValue3 = b[47];
            f.bValue4 = b[48];
            data.Current.Radius = f.fValue.ToString("0.00");
            if (data.Current.Craneno == "390417" || data.Current.Craneno == "390495")
            {
                //力矩百分比
                if (data.Current.Safetorque != "0.00")
                {
                    double dl = (double.Parse(data.Current.Weight) * double.Parse(data.Current.Radius)) / double.Parse(data.Current.Safetorque);
                    if (dl >= 1.10)
                    {
                        data.Current.Weight = ((1.05 * double.Parse(data.Current.Safetorque)) / double.Parse(data.Current.Radius)).ToString("0.00");
                        data.Current.Torquepercent = "1.05";
                    }
                    else
                        data.Current.Torquepercent = ((double.Parse(data.Current.Weight) * double.Parse(data.Current.Radius)) / double.Parse(data.Current.Safetorque)).ToString("0.00");
                }
                else
                    data.Current.Torquepercent = "0.00";
            }
            else
            {
                //力矩百分比
                if (data.Current.Safetorque != "0.00")

                    data.Current.Torquepercent = ((double.Parse(data.Current.Weight) * double.Parse(data.Current.Radius)) / double.Parse(data.Current.Safetorque)).ToString("0.00");
                else
                    data.Current.Torquepercent = "0.00";
            }
            //转角
            f.bValue1 = b[49];
            f.bValue2 = b[50];
            f.bValue3 = b[51];
            f.bValue4 = b[52];
            data.Current.Angle = f.fValue.ToString("0.00");
            //倾角X
            f.bValue1 = b[53];
            f.bValue2 = b[54];
            f.bValue3 = b[55];
            f.bValue4 = b[56];
            data.Current.AngleX = f.fValue.ToString("0.00");
            //倾角Y
            f.bValue1 = b[57];
            f.bValue2 = b[58];
            f.bValue3 = b[59];
            f.bValue4 = b[60];
            data.Current.AngleY = f.fValue.ToString("0.00");
            //倍率
            data.Current.Times = Convert.ToInt32(b[61]).ToString();
            if (int.Parse(data.Current.Times) > 4)
                data.Current.Times = "2";

            if (data.Current.Craneno == "390854")
            {
                data.Current.SafeWeight = "3.00";//安全起重量
            }
            //警告码
            data.Current.AlarmType = Convert.ToString(b[62], 2).PadLeft(32, '0');
            AlarmFlag(data, data.Current.AlarmType);//报警标识
            tStr = ConvertData.ToHexString(b, 64, 2);
            if (tStr != "7D7D")
                return null;

            //存数据库
            //看看是否发送短信
            //存入数据库
            df.deviceid = data.Current.Craneno;
            df.datatype = "current";
            df.contentjson = JsonConvert.SerializeObject(data.Current);
            return null;
        }

        /// <summary>
        /// 警告标识
        /// </summary>
        public static void AlarmFlag(GprsCraneDataObject data, string AlarmStr)
        {
            int l = AlarmStr.Length;
            bool flag = false;
            if (AlarmStr.Substring(l - 4, 1) == "1")//力矩报警
            {
                data.Current.TorqueAlarm = "2";
                flag = true;
            }
            else
            {
                data.Current.TorqueAlarm = "0";
            }
            if (AlarmStr.Substring(l - 3, 1) == "1")//交叉干涉报警
            {
                data.Current.HitAlarm = "2";
                flag = true;
            }
            else
            {
                data.Current.HitAlarm = "0";
            }
            if (AlarmStr.Substring(l - 2, 1) == "1")//超重报警
            {
                data.Current.WeightAlarm = "2";
                flag = true;
            }
            else
            {
                data.Current.WeightAlarm = "0";
            }
            if (AlarmStr.Substring(l - 1, 1) == "1")//风速报警
            {
                data.Current.WindAlarm = "2";
                flag = true;
            }
            else
            {
                data.Current.WindAlarm = "0";
            }
            //报警标示
            if (flag)
                data.Current.Type = "2";
            else
                data.Current.Type = "0";
        }
        /// <summary>
        /// 报警短信
        /// </summary>
        public static void AlarmMessge(GprsCraneDataObject data)
        {

            if (data.Current.Type == "0")//无报警时 终止
            {
                return;
            }
            #region 发送报警短信
            ////如果当前报警时间  与 上次存入数据库时间 相差 大于60秒 才再次存入数据库
            //try
            //{
            //    IGprsCraneDataComm gd = new GprsDataComm().CreateInstance();
            //    string[] Alarm = { data.Current.WeightAlarm, data.Current.WindAlarm, data.Current.HitAlarm };
            //    int Times = gd.GetOnceAlarmTime(data.Current.Craneno, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Alarm);

            //    if (Times < 60 && Times != -1)
            //        return;

            //    System.Data.DataTable dtProj = gd.GetCraneProj(data.Current.Craneno);

            //    if (dtProj == null || dtProj.Rows.Count < 0)
            //        return;

            //    if (dtProj.Rows[0]["IsMsg"].ToString() != "1")
            //        return;
            //    string Telephone = dtProj.Rows[0]["PrincipalTel"].ToString();
            //    if (Telephone.Length != 11 && Telephone.IndexOf(',') == -1)
            //    {
            //        Telephone = "";
            //    }
            //    //短信列表

            //    lock (MsgList.LsMsg)
            //    {
            //        MsgList.InsertCacheLs(Telephone + "@" + "工地:" + dtProj.Rows[0]["ProName"].ToString() + " 设备编号:" + data.Current.Craneno + " 时间:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 报警类型:" + (data.Current.HitAlarm == "2" ? "碰撞 " : "") + (data.Current.WeightAlarm == "2" ? "超重 " : "") + (data.Current.WindAlarm == "2" ? "风速 " : ""));
            //    }
            //}
            //catch
            //{
            //}
            #endregion
        }

        /// <summary>
        /// 身份认证应答
        /// </summary>
        /// <param name="b"></param>
        private static byte[] OnResolveAuthentication(byte[] b, int bCount)
        {
            #region 解析
            if (bCount != 18)
                return null;
            string tStr = ConvertData.ToHexString(b, 0, 2);
            if (tStr != "7E7E")
                return null;
            #endregion
            #region 应答
            byte[] rb = new byte[13];
            rb[0] = 0x7E;
            rb[1] = 0x7E;
            rb[2] = 0x01;
            rb[3] = 0x02;
            rb[4] = 0x05;//应用数据区数据长度
            rb[5] = 0x01;//错误：0x00；正确：0x01；删除：0x02
            rb[6] = b[9];
            rb[7] = b[10];
            rb[8] = b[11];
            rb[9] = b[12];
            rb[11] = 0x7D;
            rb[12] = 0x7D;
            return rb;
            #endregion
        }

        /// <summary>
        /// 心跳
        /// </summary>
        /// <param name="b"></param>
        /// 7E7E0100003906071804191030093C7D7D
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
            rb[0] = 0x7E;
            rb[1] = 0x7E;
            rb[2] = 0x01;
            rb[3] = 0x00;//功能码


            //存入数据库
            df.deviceid = data.Heartbeat.SN;
            df.datatype = "heartbeat";
            df.contentjson = JsonConvert.SerializeObject(data.Heartbeat);

            return rb;


        }

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
            else if(b[8] == 0x00)
            {
                DB_MysqlTowerCrane.UpdateDataCongfig(craneNo, 3,false);
                DB_MysqlTowerCrane.UpdateIPCommandIssued(craneNo, 3);


            }
            ToolAPI.XMLOperation.WriteLogXmlNoTail("GprsResolveDataV41.IP应答", string.Format("{0},{1}", craneNo, ConvertData.ToHexString(b, 0, b.Length)));
            return null;
        }

        /// <summary>
        /// 发送删除IC卡指令后设备应答
        /// </summary>
        /// <param name="b"></param>
        /// <param name="bCount"></param>
        /// <returns></returns>
        private static byte[] OnResolveIcDelAck(byte[] b, int bCount)
        {
            if (bCount != 16)
            {
                return null;
            }
            string tStr = ConvertData.ToHexString(b, 0, 2);
            if (tStr != "7E7E")
                return null;
            //设备号
            //string craneNo = ConvertData.ToHexString(b, 6, 3);
            //string icSN = ConvertData.ToHexString(b, 9, 4);
            //IGprsCraneDataComm gd = new GprsDataComm().CreateInstance();
            //gd.UpdateIdentifyCurrent(craneNo, icSN);
            return null;
        }
        #region
        /// <summary>
        /// 司机刷卡历史信息
        /// </summary>
        /// <param name="b"></param>
        /// <param name="bCount"></param>
        private static byte[] OnResolveDriverLogin(byte[] b, int bCount)
        {
            #region 解析
            if (bCount != 22)
                return null;
            string tStr = ConvertData.ToHexString(b, 0, 2);
            if (tStr != "7E7E")
                return null;
            //GprsCraneDataObject data = new GprsCraneDataObject();
            
            ////设备号
            //data.personLogin.NodeID = ConvertData.ToHexString(b, 5, 3);
            //// 司机卡号
            //data.personLogin.IcCode = ConvertData.ToHexString(b, 9, 4); 
            //// 时间
            //tStr = ConvertData.ToHexString(b, 13, 6);
            //try
            //{
            //    data.personLogin.KqTime = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
            //}
            //catch
            //{
            //    data.personLogin.KqTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //}
            #endregion
            //data.DataType = CraneList.MessageTypes.Authen;
            //CraneList.InsertCacheLs(data);
            #region 应答
            byte[] rb = new byte[9];
            rb[0] = 0x7E;
            rb[1] = 0x7E;
            rb[2] = 0x01;
            rb[3] = 0x03;
            rb[4] = 0x01;//应用数据区数据长度
            rb[5] = 0x01;//0x01：接收成功
            rb[7] = 0x7D;
            rb[8] = 0x7D;
            return rb;
            #endregion

        }
        /// <summary>
        /// 参数上传
        /// </summary>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// 7E7E010400390607047354CE43FA000A009BB0F401B6C14102F1148A0700000000000000000000000070179600F401E803B80BF000F401E80300000000000000000000000000000014000A000A000A00000000000000000000000000326E3264544D455320312E312E37302E52475043372E32303135303432305F54D47D7D
        private static byte[] OnResolveParamDataUpload(byte[] b, int bCount, ref DBFrame df)
        {
            #region 解析
            string tStr = ConvertData.ToHexString(b, 0, 2);
            if (tStr != "7E7E")
                return null;
            GprsCraneDataObject data = new GprsCraneDataObject();
            
            //设备号
            data.CraneConfig.craneNo = ConvertData.ToHexString(b, 5, 3);
            // 倍率
            data.CraneConfig.ratio = Convert.ToInt32(b[8]).ToString();
            UShortValue s = new UShortValue();
            //最小高度时 AD 采样值
            s.bValue1 = b[9];
            s.bValue2 = b[10];
            data.CraneConfig.minHighAD = s.sValue.ToString("0.00");
            //最大高度时 AD 采样值
            s.bValue1 = b[11];
            s.bValue2 = b[12];
            data.CraneConfig.maxHighAD = s.sValue.ToString("0.00");
            //标准尺长度
            s.bValue1 = b[13];
            s.bValue2 = b[14];
            data.CraneConfig.standardScale = (s.sValue / 10.00).ToString("0.00");
            //最小幅度
            s.bValue1 = b[15];
            s.bValue2 = b[16];
            data.CraneConfig.minAmplitude = (s.sValue / 10.00).ToString("0.00");
            //最小幅度时 AD 采样值
            s.bValue1 = b[17];
            s.bValue2 = b[18];
            data.CraneConfig.minAmplitudeAD = s.sValue.ToString("0.00");//
            //最大幅度
            s.bValue1 = b[19];
            s.bValue2 = b[20];
            data.CraneConfig.maxAmplitude = (s.sValue / 10.00).ToString("0.00");
            //最大幅度时 AD 采样值
            s.bValue1 = b[21];
            s.bValue2 = b[22];
            data.CraneConfig.maxAmplitudeAD = s.sValue.ToString("0.00");
            //空钩时 AD 采样值
            s.bValue1 = b[23];
            s.bValue2 = b[24];
            data.CraneConfig.emptyhookAD = s.sValue.ToString("0.00");
            //吊载砝码时 AD 采样值
            s.bValue1 = b[25];
            s.bValue2 = b[26];
            data.CraneConfig.loadWeightAD = s.sValue.ToString("0.00");
            //砝码重量
            s.bValue1 = b[27];
            s.bValue2 = b[28];
            data.CraneConfig.farmarWeight = s.sValue.ToString("0.00");
            //回转类型
            data.CraneConfig.rotaryType = Convert.ToInt32(b[29]).ToString();
            //绝对值回转方向
            data.CraneConfig.absTurnDirection = Convert.ToInt32(b[30]).ToString();

            //绝对值回转值
            s.bValue1 = b[31];
            s.bValue2 = b[32];
            data.CraneConfig.absTurnValue = s.sValue.ToString("0.00");
            //绝对值回转点确认后的回转值
            s.bValue1 = b[33];
            s.bValue2 = b[34];
            data.CraneConfig.absTurnPointValue = s.sValue.ToString("0.00");
            //电位器回转左限位 AD 值
            s.bValue1 = b[35];
            s.bValue2 = b[36];
            data.CraneConfig.potLeftLimitAD = s.sValue.ToString("0.00");
            //电位器回转右限位 AD 值
            s.bValue1 = b[37];
            s.bValue2 = b[38];
            data.CraneConfig.potRightLimitAD = s.sValue.ToString("0.00");
            //电位器回转左右限位角度和
            s.bValue1 = b[39];
            s.bValue2 = b[40];
            data.CraneConfig.potLimitAngle = s.sValue.ToString("0.00");
            //4 倍率时最大起重量
            s.bValue1 = b[41];
            s.bValue2 = b[42];
            data.CraneConfig.liftWeight4Ratio = s.sValue.ToString("0.00");
            //4 倍率时最大起重量幅度
            s.bValue1 = b[43];
            s.bValue2 = b[44];
            data.CraneConfig.liftWeightRange4R = (s.sValue / 10.00).ToString("0.00");
            //4 倍率时最大幅度
            s.bValue1 = b[45];
            s.bValue2 = b[46];
            data.CraneConfig.maxRange4Ratio = (s.sValue / 10.00).ToString("0.00");
            //4 倍率时最大幅度起重量
            s.bValue1 = b[47];
            s.bValue2 = b[48];
            data.CraneConfig.maxRangeWeight4R = s.sValue.ToString("0.00");
            //2 倍率时最大起重量
            s.bValue1 = b[49];
            s.bValue2 = b[50];
            data.CraneConfig.liftWeight2Ratio = s.sValue.ToString("0.00");
            //2 倍率时最大起重量幅度
            s.bValue1 = b[51];
            s.bValue2 = b[52];
            data.CraneConfig.liftWeightRange2R = (s.sValue / 10.00).ToString("0.00");
            //2 倍率时最大幅度
            s.bValue1 = b[53];
            s.bValue2 = b[54];
            data.CraneConfig.maxRange2Ratio = (s.sValue / 10.00).ToString("0.00");
            //2 倍率时最大幅度起重量
            s.bValue1 = b[55];
            s.bValue2 = b[56];
            data.CraneConfig.maxRangeWeight2R = s.sValue.ToString("0.00");
            //ZIGBEE 本机编号
            data.CraneConfig.zigbeeLocalNo = Convert.ToInt32(b[57]).ToString();
            //ZIGBEE 本机频道号
            data.CraneConfig.zigbeeChannelNo = Convert.ToInt32(b[58]).ToString();
            //ZIGBEE 本机组号
            data.CraneConfig.zigbeeGroupNo = Convert.ToInt32(b[59]).ToString();
            //防碰撞信息本机 X
            s.bValue1 = b[60];
            s.bValue2 = b[61];
            data.CraneConfig.antiCollisionX = s.sValue.ToString("0.00");
            //防碰撞信息本机 Y
            s.bValue1 = b[62];
            s.bValue2 = b[63];
            data.CraneConfig.antiCollisionY = s.sValue.ToString("0.00");
            //起重臂长
            s.bValue1 = b[64];
            s.bValue2 = b[65];
            data.CraneConfig.liftWeightArmLenght = s.sValue.ToString("0.00");
            //平衡臂长
            s.bValue1 = b[66];
            s.bValue2 = b[67];
            data.CraneConfig.balanceArmLenght = s.sValue.ToString("0.00");
            //塔身高度
            s.bValue1 = b[68];
            s.bValue2 = b[69];
            data.CraneConfig.towerHeight = s.sValue.ToString("0.00");
            //塔冒高度
            s.bValue1 = b[70];
            s.bValue2 = b[71];
            data.CraneConfig.towerAtHeight = s.sValue.ToString("0.00");
            //幅度减速值
            s.bValue1 = b[72];
            s.bValue2 = b[73];
            data.CraneConfig.ampReductionValue = (s.sValue / 10.00).ToString("0.00");
            //幅度限速值
            s.bValue1 = b[74];
            s.bValue2 = b[75];
            data.CraneConfig.ampRestrictValue = (s.sValue / 10.00).ToString("0.00");
            //高度减速值
            s.bValue1 = b[76];
            s.bValue2 = b[77];
            data.CraneConfig.highReductionValue = (s.sValue / 10.00).ToString("0.00");
            //高度限速值
            s.bValue1 = b[78];
            s.bValue2 = b[79];
            data.CraneConfig.highRestrictValue = (s.sValue / 10.00).ToString("0.00");
            //回转减速值
            s.bValue1 = b[80];
            s.bValue2 = b[81];
            data.CraneConfig.turnReducionValue = s.sValue.ToString("0.00");
            //回转限位值
            s.bValue1 = b[82];
            s.bValue2 = b[83];
            data.CraneConfig.turnRestrictValue = s.sValue.ToString("0.00");
            //区域保护减速值
            s.bValue1 = b[84];
            s.bValue2 = b[85];
            data.CraneConfig.areaReductionValue = s.sValue.ToString("0.00");
            //区域保护限位值
            s.bValue1 = b[86];
            s.bValue2 = b[87];
            data.CraneConfig.areaRestrictValue = s.sValue.ToString("0.00");
            //防碰撞减速值
            s.bValue1 = b[88];
            s.bValue2 = b[89];
            data.CraneConfig.acReductionValue = (s.sValue / 10.00).ToString("0.00");
            //防碰撞限位值
            s.bValue1 = b[90];
            s.bValue2 = b[91];
            data.CraneConfig.acRestrictValue = (s.sValue / 10.00).ToString("0.00");
            //换速力矩
            data.CraneConfig.throwOverTorque = Convert.ToInt32(b[92]).ToString();
            //切断力矩
            data.CraneConfig.cutTorque = Convert.ToInt32(b[93]).ToString();
            //换速重量
            data.CraneConfig.throwOverWeight = Convert.ToInt32(b[94]).ToString();
            //切断重量
            data.CraneConfig.cutWeight = Convert.ToInt32(b[95]).ToString();
            int i = bCount - 96 > 0 ? bCount - 96 : 0;
            if (i > 3)
            {
                i = i - 3;
            }
            byte[] va = new byte[i];
            for (int ss = 0; ss < i; ss++)
            {
                va[ss] = b[96 + ss];
            }
            data.CraneConfig.softVersion = System.Text.Encoding.ASCII.GetString(va);

            df.deviceid = data.CraneConfig.craneNo;
            df.datatype = "parameterUpload";
            df.contentjson = JsonConvert.SerializeObject(data.CraneConfig);
            #endregion
            #region 应答
            byte[] rb = new byte[9];
            rb[0] = 0x7E;
            rb[1] = 0x7E;
            rb[2] = 0x01;
            rb[3] = 0x04;
            rb[4] = 0x01;//应用数据区数据长度
            rb[5] = 0x01;//0x01：接收成功
            rb[7] = 0x7D;
            rb[8] = 0x7D;
            return rb;
            #endregion

        }

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
                ms.WriteByte(0x01);
                ms.WriteByte(0x05);

                byte[] ip = Encoding.ASCII.GetBytes(dr["ip"].ToString());
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
            rb[2] = 0x01;
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
    }
}
        #endregion