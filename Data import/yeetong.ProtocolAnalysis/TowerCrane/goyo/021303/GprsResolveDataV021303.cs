using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using TCPAPI;
using System.Reflection;
using ToolAPI;
using ProtocolAnalysis.TowerCrane._021303;
using ProtocolAnalysis.TowerCrane;
using Architecture;
using Newtonsoft.Json;
namespace ProtocolAnalysis
{
    public class GprsResolveDataV021303
    {
        //static bool istrue = true;
        //起始符+协议版本号+命令字+数据长度+数据载荷+CEC16+结束符
        //(字节)2+3+1+2+len+2+2 = 8(头)+LEn(数据载荷)+4(尾)
        public static string OnResolveRecvMessage(byte[] b, int c, TcpSocketClient client)
        {
            if (b.Length < 5)
                return "";
            byte typ = b[5];
            byte[] frameHead = b.Take(6).ToArray();//协议帧头
            byte[] frameDataLoad = GetDataLoad(b, c);//数据载荷

            DBFrame df = new DBFrame();
            df.contenthex = ConvertData.ToHexString(b, 0, c);
            df.version = (client.External.External as TcpClientBindingExternalClass).TVersion;
            switch (typ)
            {
                case 0x00: //心跳
                    //XMLOperation.WriteLogXmlNoTail("心跳接收", ConvertData.ToHexString(b, 0, c));
                    OnResolveHeabert(frameDataLoad, frameHead, client,ref df); break;
                case 0x01: //实时数据
                    OnResolveCurrent(frameDataLoad, frameHead, typ,ref df); break;
                case 0x02: //身份验证
                    AuthenticationDispose(frameDataLoad, frameHead, client); break;
                case 0x03: //离线数据
                    OnResolveCurrent(frameDataLoad, frameHead, typ, ref df);
                    OnResolveOfflineData(frameDataLoad, frameHead, client); break;
                case 0x04: //参数信息上传
                    OnResolveParameterUpload(frameDataLoad, frameHead, client,ref df); break;
                case 0x05: //IP地址配置
                    XMLOperation.WriteLogXmlNoTail("IP命令下发应答", ConvertData.ToHexString(b, 0, c));
                    OnResolveIPConfigure(frameDataLoad); break;
                case 0x06: break;//保留
                case 0x07://设备运行时间
                    OnResolveRuntimeEp(frameDataLoad, frameHead, client,ref df); break;
                case 0x08://命令下发
                    XMLOperation.WriteLogXmlNoTail("控制命令下发应答", ConvertData.ToHexString(b, 0, c));
                    OnResolveCommandIssued(frameDataLoad); break;
                case 0x09://信息上传
                    OnResolveInformationUpload(frameDataLoad, frameHead, client,ref df); break;
                case 0x0A://黑匣子信息
                    OnResolveBlackBox(frameDataLoad, frameHead, client,ref df); break;
                case 0x0B://塔吊基本参数
                    OnResolveTowerCraneBasicInformation(frameDataLoad, frameHead, client,ref df); break;
                case 0x0C://防碰撞
                    OnResolvePreventCollision(frameDataLoad, frameHead, client,ref df); break;
                case 0x0D://区域保护设置
                    OnResolveLocalityProtection(frameDataLoad, frameHead, client,ref df); break;
                default: break;
            }

            TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;
            if (TcpExtendTemp.EquipmentID == null || TcpExtendTemp.EquipmentID.Equals(""))
            {
                TcpExtendTemp.EquipmentID = df.deviceid;
            }
            //存入数据库
            if(df.contentjson!=null&&df.contentjson!="")
            {
                DB_MysqlTowerCrane.SaveTowerCrane(df);
            }
            return "";
        }

        #region 心跳
        //模拟接受帧：A5 5A 02 13 03 00 0E 00 31 32 33 34 35 36 37 38 17 02 27 11 09 20 25 F3 EE FF
        //模拟应答帧：a5 5a 02 13 03 00 07 00 01 17 02 27 11 37 16 e2 d2 ee ff
        private static void OnResolveHeabert(byte[] frameDataLoad, byte[] frameHead, TcpSocketClient client,ref DBFrame df)
        {
            try
            {
                if (frameDataLoad.Length != 14)
                {
                    return;
                }
                HeartbeatV021303 data = new HeartbeatV021303();
                //设备编号
                byte[] sn = new byte[8];   //设备编号
                Array.Copy(frameDataLoad, 0, sn, 0, 8);
                data.EquipmentID = Encoding.ASCII.GetString(sn);  //获取设备编号ASCII
                //时间
                string tStr = ConvertData.ToHexString(frameDataLoad, 8, 6);
                DateTime getdate = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                //根据时间差判断是否需要时间校准
                DateTime dt = DateTime.Now;
                double compare = (dt - getdate).TotalMinutes;
                List<byte> rb = new List<byte>();
                //头
                rb.AddRange(frameHead);
                if (Math.Abs(compare) > 1)//需要校验
                {
                    //长度
                    rb.Add(0x07); rb.Add(0x00);
                    //时间标识
                    rb.Add(0x01);
                    //时间
                    rb.Add(byte.Parse(dt.Year.ToString().Substring(2, 2), System.Globalization.NumberStyles.HexNumber));
                    rb.Add(byte.Parse(dt.Month.ToString(), System.Globalization.NumberStyles.HexNumber));
                    rb.Add(byte.Parse(dt.Day.ToString(), System.Globalization.NumberStyles.HexNumber));
                    rb.Add(byte.Parse(dt.Hour.ToString(), System.Globalization.NumberStyles.HexNumber));
                    rb.Add(byte.Parse(dt.Minute.ToString(), System.Globalization.NumberStyles.HexNumber));
                    rb.Add(byte.Parse(dt.Second.ToString(), System.Globalization.NumberStyles.HexNumber));
                    data.OnlineTime = dt.ToString("yyyy-MM-dd HH:mm:ss");
                }
                else//不需要校验
                {
                    //长度
                    rb.Add(0x01); rb.Add(0x00);
                    //时间标识
                    rb.Add(0x00);
                    data.OnlineTime = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
                }
                //返回应答帧
                byte[] result = PackageFrameTail(rb);
                client.SendBuffer(result);
                //XMLOperation.WriteLogXmlNoTail("心跳应答", ConvertData.ToHexString(result, 0, result.Length));
                //存入数据库
                df.deviceid = data.EquipmentID;
                df.datatype = "heartbeat";
                df.contentjson = JsonConvert.SerializeObject(data);
            }
            catch (Exception) { }
        }
        #endregion

        #region 实时数据
        //模拟接受帧：A5 5A 02 13 03 01 00 00 31 32 33 34 35 36 37 38 01 00 38 37 36 35 34 32 33 31 17 06 22 12 01 02 00 02 01 00 02 00 03 00 04 00 05 00 06 00 07 00 08 00 09 00 10 00 11 00 12 00 13 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 02 02 00 01 00 02 00 03 00 04 00 05 00 06 00 07 00 08 00 00 00 00 00 00 00 00 00 02 02 00 02 00 03 00 04 00 05 00 06 00 07 00 08 00 00 00 00 00 00 00 00 00 D8 19 EE FF
        private static void OnResolveCurrent(byte[] frameDataLoad, byte[] frameHead, byte typ, ref DBFrame df)
        {
            try
            {
                CurrentV021303 data = new CurrentV021303();
                //设备编号
                byte[] sn = new byte[8];   //设备编号
                Array.Copy(frameDataLoad, 0, sn, 0, 8);
                data.EquipmentID = Encoding.ASCII.GetString(sn);  //获取设备编号ASCII
              
                //工作循环序列号
                data.WorkCircle = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", frameDataLoad[9], frameDataLoad[8]), 16);
                //司机工号/司机卡号
                Array.Copy(frameDataLoad, 10, sn, 0, 8);
                data.DriverCardNo = Encoding.ASCII.GetString(sn);
              
                //RTC
                string tStr = ConvertData.ToHexString(frameDataLoad, 18, 6);
                data.RTC = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
             
                //电源状态
                data.PowerState = frameDataLoad[24];
              
                //倍率
                data.Times = frameDataLoad[25];
              
                //高度
                data.Height = Convert.ToInt16(string.Format("{0:X2}{1:X2}", frameDataLoad[27], frameDataLoad[26]), 16);
             
                //幅度
                data.Range = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", frameDataLoad[29], frameDataLoad[28]), 16);
              
                //回转
                data.Rotation = Convert.ToInt16(string.Format("{0:X2}{1:X2}", frameDataLoad[31], frameDataLoad[30]), 16);
             
                //重量
                data.Weight = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", frameDataLoad[33], frameDataLoad[32]), 16);
              
                //风速
                data.WindSpeed = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", frameDataLoad[35], frameDataLoad[34]), 16);

                //倾角X
                data.DipAngle_X = Convert.ToInt16(string.Format("{0:X2}{1:X2}", frameDataLoad[37], frameDataLoad[36]), 16);
              
                //倾角Y
                data.DipAngle_Y = Convert.ToInt16(string.Format("{0:X2}{1:X2}", frameDataLoad[39], frameDataLoad[38]), 16);
          
                //动臂俯仰角
                data.BoomAngle = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", frameDataLoad[41], frameDataLoad[40]), 16);
                //行程
                data.Stroke = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", frameDataLoad[43], frameDataLoad[42]), 16);
                //安全幅度
                data.SafeRange = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", frameDataLoad[45], frameDataLoad[44]), 16);
                // 安全重量
                data.SafeWeight = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", frameDataLoad[47], frameDataLoad[46]), 16);
              
                // 安全力矩
                data.SafeMoment = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", frameDataLoad[49], frameDataLoad[48]), 16);
           
               
                // 防碰撞通信状态
                data.CollisionCommunicationState = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", frameDataLoad[51], frameDataLoad[50]), 16);
                // 模块状态
                data.ModuleState = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", frameDataLoad[53], frameDataLoad[52]), 16);
                // 继电器状态
                data.RelayState = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", frameDataLoad[55], frameDataLoad[54]), 16);
            
                // 传感器状态
                data.SensorState = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", frameDataLoad[57], frameDataLoad[56]), 16);
            
                //预警信息
                data.WarningMessage = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", frameDataLoad[61], frameDataLoad[60], frameDataLoad[59], frameDataLoad[58]), 16);
               
                //报警信息
                data.AlarmMessage = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", frameDataLoad[65], frameDataLoad[64], frameDataLoad[63], frameDataLoad[62]), 16);
             
                AlarmFlag(data.EquipmentID, Convert.ToString(data.AlarmMessage, 2).PadLeft(32, '0'));//处理报警码，进行短信发送
                // 设备显示
                data.EquipmentDisplaying = frameDataLoad[66];
                // 可设置信息
                data.CanSetMessage = frameDataLoad[67];
                // 副臂个数/副钩个数
                data.ViceHookCount = frameDataLoad[68];
                //副钩数据信息
                if (data.ViceHookCount == 0)
                {
                    data.ViceHookMessage = null;
                }
                else
                {
                    data.ViceHookMessage = new CurrentV021303.ViceHook[data.ViceHookCount];
                    for (int i = 0; i < data.ViceHookCount; i++)
                    {
                        try
                        {
                            byte[] byteTemp = new byte[25];
                            Array.Copy(frameDataLoad, 69 + i * 25, byteTemp, 0, 25);
                            CurrentV021303.ViceHook viceHookTemp = new CurrentV021303.ViceHook();
                            #region 协议解析
                            //倍率
                            viceHookTemp.Times = byteTemp[0];
                            //高度
                            viceHookTemp.Height = Convert.ToInt16(string.Format("{0:X2}{1:X2}", byteTemp[2], byteTemp[1]), 16);
                            //幅度
                            viceHookTemp.Range = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[4], byteTemp[3]), 16);
                            //重量
                            viceHookTemp.Weight = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[6], byteTemp[5]), 16);
                            //安全幅度
                            viceHookTemp.SafeRange = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[8], byteTemp[7]), 16);
                            // 安全重量
                            viceHookTemp.SafeWeight = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[10], byteTemp[9]), 16);
                            // 安全力矩
                            viceHookTemp.SafeMoment = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[12], byteTemp[11]), 16);
                            // 继电器状态
                            viceHookTemp.RelayState = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[14], byteTemp[13]), 16);
                            // 传感器状态
                            viceHookTemp.SensorState = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[16], byteTemp[15]), 16);
                            //预警信息
                            viceHookTemp.WarningMessage = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", byteTemp[20], byteTemp[19], byteTemp[18], byteTemp[17]), 16);
                            //报警信息
                            viceHookTemp.AlarmMessage = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", byteTemp[24], byteTemp[23], byteTemp[22], byteTemp[21]), 16);
                            #endregion
                            data.ViceHookMessage[i] = viceHookTemp;
                        }
                        catch (Exception) { }
                    }

                }
                //创建针对象
                //string jsonstr = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                //存入数据库
                df.deviceid = data.EquipmentID;
                df.datatype = "current";
                df.contentjson = JsonConvert.SerializeObject(data);
            }
            catch (Exception) { };
        }
        #endregion

        #region 身份验证
        /*命令字1且刷脸：A5 5A 02 13 03 02 00 00 31 32 33 34 35 36 37 38 06 01 31 30 30 30 30 33 37 35 01 93 2A EE FF 
         * 应答：A5 5A 02 13 03 02 13 00 31 32 33 34 35 36 37 38 06 01 31 30 30 30 30 33 37 35 01 93 2a ee ff
          命令字1且刷卡：A5 5A 02 13 03 02 00 00 31 32 33 34 35 36 37 38 01 01 44 44 37 36 45 30 31 42 01 9E 72 EE FF
         * 应答：A5 5A 02 13 03 02 13 00 31 32 33 34 35 36 37 38 01 01 44 44 37 36 45 30 31 42 01 9e 72 ee ff
          命令字2且刷脸：A5 5A 02 13 03 02 00 00 31 32 33 34 35 36 37 38 06 02 01 01 34 31 31 34 30 32 31 39 39 32 31 32 30 37 35 35 31 38 B5 9B EE FF
         * 应答：A5 5A 02 13 03 02 0d 00 31 32 33 34 35 36 37 38 06 02 01 01 01 a5 b5 ee ff A5 5A 02 13 03 02 51 00 31 32 33 34 35 36 37 38 06 03 01 31 30 30 30 30 33 37 35 31 30 30 30 30 33 37 35 34 31 31 34 30 32 31 39 39 32 31 32 30 37 35 35 31 38 ff ff ff ff ff ff ff ff ff ff ff d6 a3 cf fe b6 ac ff ff ff ff ff ff ff ff ff ff 20 17 03 02 00 ff ff ff ff b4 9d ee ff
          命令字2且刷卡：A5 5A 02 13 03 02 00 00 31 32 33 34 35 36 37 38 01 02 01 01 44 44 37 36 45 30 31 42 0D EC EE FF
         * 应答：A5 5A 02 13 03 02 0d 00 31 32 33 34 35 36 37 38 01 02 01 01 01 71 d2 ee ff A5 5A 02 13 03 02 51 00 31 32 33 34 35 36 37 38 01 03 01 31 30 30 30 30 33 37 35 44 44 37 36 45 30 31 42 31 33 32 39 30 32 31 39 38 30 31 32 31 31 32 30 33 34 ff ff ff ff ff ff ff ff ff ff ff d6 a3 cf fe b6 ac ff ff ff ff ff ff ff ff ff ff 20 17 03 02 00 ff ff ff ff 35 4e ee ff*/
        private static void AuthenticationDispose(byte[] frameDataLoad, byte[] frameHead, TcpSocketClient client)
        {
            try
            {
                //查看是否需要验证
                string root = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string path = root.Remove(root.LastIndexOf('\\') + 1) + "Config.ini";
                bool IsAuthentication = bool.Parse(FileHelp.IniReadValue("goyo", "IsAuthentication", path));
                //进行协议解析
                AuthenticateV021303 AuthenticationTemp = AuthenticationAnalysis(frameDataLoad);
                #region 需要验证
                if (IsAuthentication)
                {
                    DVerification(AuthenticationTemp, frameHead, client);
                }
                #endregion
                #region 不需要验证
                else
                {
                    #region
                    List<byte> rb = new List<byte>();
                    //头
                    rb.AddRange(frameHead);
                    //长度 
                    rb.Add(0); rb.Add(0);
                    //设备编号
                    byte[] byteArray = System.Text.Encoding.Default.GetBytes(AuthenticationTemp.EquipmentID);
                    rb.AddRange(byteArray);
                    //身份识别方式
                    rb.Add(AuthenticationTemp.IdentificationType);
                    //子命令字
                    rb.Add(AuthenticationTemp.Subcommand);

                    switch (AuthenticationTemp.Subcommand)
                    {
                        case 1://系统状态通知
                            rb[6] = 19; rb[7] = 0;
                            //数据区
                            //司机工号
                            string DriverCardNo = (AuthenticationTemp.SubcommandDistrict as AuthenticateV021303.OneSubcommandEP).DriverCardNo;
                            byte[] byteArray1 = System.Text.Encoding.Default.GetBytes(DriverCardNo);
                            rb.AddRange(byteArray1);
                            rb.Add(1); break;
                    #endregion
                        case 2://特征码上传验证
                            #region
                            //长度 
                            rb[6] = 13; rb[7] = 0;
                            rb.Add((AuthenticationTemp.SubcommandDistrict as AuthenticateV021303.TwoAndFourSubcommandEP).AllPackageCount);
                            rb.Add((AuthenticationTemp.SubcommandDistrict as AuthenticateV021303.TwoAndFourSubcommandEP).PresentPackage);
                            rb.Add(1); break;
                            #endregion
                        default: break;//特征码下发应答 平台删除应答 建立授权人信息 不需要做处理
                    }
                    client.SendBuffer(PackageFrameTail(rb));
                }
                #endregion
            }
            catch (Exception) { }
        }
        // 协议解析
        private static AuthenticateV021303 AuthenticationAnalysis(byte[] frameDataLoad)
        {
            try
            {
                AuthenticateV021303 data = new AuthenticateV021303();
                //设备编号
                byte[] sn = new byte[8];   //设备编号
                Array.Copy(frameDataLoad, 0, sn, 0, 8);
                data.EquipmentID = Encoding.ASCII.GetString(sn);  //获取设备编号ASCII
                //身份识别方式
                data.IdentificationType = frameDataLoad[8];
                //子命令号
                data.Subcommand = frameDataLoad[9];
                //数据区
                byte[] SubcommandDistrictByteArr = new byte[frameDataLoad.Length - 10];
                Array.Copy(frameDataLoad, 10, SubcommandDistrictByteArr, 0, frameDataLoad.Length - 10);
                switch (data.Subcommand)
                {
                    case 1:
                        AuthenticateV021303.OneSubcommandEP dataTempOne = new AuthenticateV021303.OneSubcommandEP();
                        Array.Copy(SubcommandDistrictByteArr, 0, sn, 0, 8);
                        dataTempOne.DriverCardNo = Encoding.ASCII.GetString(sn);
                        dataTempOne.State = SubcommandDistrictByteArr[8];
                        data.SubcommandDistrict = dataTempOne; break;
                    case 2:
                        AuthenticateV021303.TwoAndFourSubcommandEP dataTempTwo = new AuthenticateV021303.TwoAndFourSubcommandEP();
                        dataTempTwo.AllPackageCount = SubcommandDistrictByteArr[0];
                        dataTempTwo.PresentPackage = SubcommandDistrictByteArr[1];
                        //由于当前使用得验证方式上传得都为ASCII码，所有做同意处理
                        byte[] sdArr = new byte[SubcommandDistrictByteArr.Length - 2];
                        Array.Copy(SubcommandDistrictByteArr, 2, sdArr, 0, SubcommandDistrictByteArr.Length - 2);
                        dataTempTwo.FeatureLibrary = Encoding.ASCII.GetString(sdArr);
                        data.SubcommandDistrict = dataTempTwo; break;
                    case 4:
                        AuthenticateV021303.TwoAndFourSubcommandSE dataTempFour = new AuthenticateV021303.TwoAndFourSubcommandSE();
                        dataTempFour.AllPackageCount = SubcommandDistrictByteArr[0];
                        dataTempFour.PresentPackage = SubcommandDistrictByteArr[1];
                        dataTempFour.Result = SubcommandDistrictByteArr[2];
                        data.SubcommandDistrict = dataTempFour; break;
                    case 5:
                        data.SubcommandDistrict = SubcommandDistrictByteArr[0]; break;
                    case 6:
                        AuthenticateV021303.SixSubcommandEP dataTempSix = new AuthenticateV021303.SixSubcommandEP();
                        byte[] gh = new byte[8];
                        Array.Copy(SubcommandDistrictByteArr, 0, gh, 0, 8);
                        dataTempSix.Administrator_gh = Encoding.ASCII.GetString(gh);
                        byte[] idCard = new byte[18];
                        Array.Copy(SubcommandDistrictByteArr, 8, idCard, 0, 18);
                        dataTempSix.Authorizer_IDCard = Encoding.ASCII.GetString(idCard);
                        data.SubcommandDistrict = dataTempSix; break;
                    default: break;
                }
                return data;
            }
            catch (Exception) { return null; }
        }
        //数据库验证并出结果
        private static void DVerification(AuthenticateV021303 AuthenticationTemp, byte[] frameHead, TcpSocketClient client)
        {
            switch (AuthenticationTemp.Subcommand)
            {
                case 1://系统状态通知
                    #region
                    List<byte> rb = new List<byte>();
                    //头
                    rb.AddRange(frameHead);
                    //长度 
                    rb.Add(19); rb.Add(0);
                    //设备编号
                    byte[] byteArray = System.Text.Encoding.Default.GetBytes(AuthenticationTemp.EquipmentID);
                    rb.AddRange(byteArray);
                    //身份识别方式
                    rb.Add(AuthenticationTemp.IdentificationType);
                    //子命令字
                    rb.Add(AuthenticationTemp.Subcommand);

                    //司机工号
                    string DriverCardNo = (AuthenticationTemp.SubcommandDistrict as AuthenticateV021303.OneSubcommandEP).DriverCardNo;
                    byteArray = System.Text.Encoding.Default.GetBytes(DriverCardNo);
                    rb.AddRange(byteArray);
                    if (AuthenticationTemp.IdentificationType == 6)//人脸识别
                    {
                        //验证是否正确
                        DataTable dt = DB_MysqlTowerCrane.GetDriverInfoByEmpNo(AuthenticationTemp.EquipmentID,DriverCardNo);  //获取司机相关信息
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            rb.Add(1);
                        }
                        else rb.Add(0);
                    }
                    else//其它
                    {
                        //验证是否正确
                        DataTable dt = DB_MysqlTowerCrane.GetIdentifyInfoByEmpNo(AuthenticationTemp.EquipmentID,DriverCardNo);  //获取司机相关信息
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            rb.Add(1);
                        }
                        else rb.Add(0);
                    }
                    //做一下考勤记录
                    DB_MysqlTowerCrane.Pro_Authentication(AuthenticationTemp);
                    //应答下发
                    client.SendBuffer(PackageFrameTail(rb));
                    break;
                    #endregion
                case 2://特征码上传验证
                    #region
                    #region 进行一次应答
                    List<byte> rb2 = new List<byte>();
                    //头
                    rb2.AddRange(frameHead);
                    //长度 
                    rb2.Add(13); rb2.Add(0);
                    //设备编号
                    byteArray = System.Text.Encoding.Default.GetBytes(AuthenticationTemp.EquipmentID);
                    rb2.AddRange(byteArray);
                    //身份识别方式
                    rb2.Add(AuthenticationTemp.IdentificationType);
                    //子命令字
                    rb2.Add(AuthenticationTemp.Subcommand);

                    rb2.Add((AuthenticationTemp.SubcommandDistrict as AuthenticateV021303.TwoAndFourSubcommandEP).AllPackageCount);
                    rb2.Add((AuthenticationTemp.SubcommandDistrict as AuthenticateV021303.TwoAndFourSubcommandEP).PresentPackage);
                    rb2.Add(1);
                    //client.SendBuffer(PackageFrameTail(rb2));
                    #endregion
                    #region 再下发司机信息帧
                    //获取司机相关信息，然后做一个下发
                    rb2.Clear();
                    rb2.AddRange(frameHead);
                    //长度 
                    rb2.Add(0); rb2.Add(0);
                    //设备编号
                    byteArray = System.Text.Encoding.Default.GetBytes(AuthenticationTemp.EquipmentID);
                    rb2.AddRange(byteArray);
                    //身份识别方式
                    rb2.Add(AuthenticationTemp.IdentificationType);
                    //子命令字
                    rb2.Add(3);//验证信息下发
                    string DriverCardNo2 = (AuthenticationTemp.SubcommandDistrict as AuthenticateV021303.TwoAndFourSubcommandEP).FeatureLibrary;
                    if (AuthenticationTemp.IdentificationType == 6)//人脸识别
                    {
                        //验证是否正确
                        DataTable dt = DB_MysqlTowerCrane.GetDriverInfoByIDCARD(AuthenticationTemp.EquipmentID,DriverCardNo2);  //获取司机相关信息
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            //长度 
                            rb2[6] = 81; rb2[7] = 0;
                            rb2.Add(1);
                            //工号
                            rb2.AddRange(FixedLengthFormat_byte(System.Text.Encoding.Default.GetBytes(dt.Rows[0]["empNo"].ToString().Trim()), 8, 0xff));
                            //卡号
                            rb2.AddRange(FixedLengthFormat_byte(System.Text.Encoding.Default.GetBytes(dt.Rows[0]["cardNo"].ToString().Trim()), 8, 0xff));
                            //身份证号
                            rb2.AddRange(FixedLengthFormat_byte(System.Text.Encoding.Default.GetBytes(dt.Rows[0]["code"].ToString().Trim()), 18, 0xff));
                            //电话
                            rb2.AddRange(FixedLengthFormat_byte(System.Text.Encoding.Default.GetBytes(dt.Rows[0]["tel"].ToString().Trim()), 11, 0xff));
                            //姓名
                            rb2.AddRange(FixedLengthFormat_byte(Encoding.GetEncoding("GBK").GetBytes(dt.Rows[0]["name"].ToString().Trim()), 16, 0xff));
                            DateTime date = System.DateTime.Now;                  //有效期
                            int year = date.Year - 2000;
                            int month = date.Month;
                            int day = date.Day;
                            rb2.Add(0x20);
                            rb2.Add(fomaterByte(year));
                            rb2.Add(fomaterByte(month));
                            rb2.Add(fomaterByte(day));
                            string cardType = dt.Rows[0]["cardType"].ToString();
                            if (!string.IsNullOrEmpty(cardType))               //职位
                            {
                                if (cardType.Contains("塔吊司机卡"))
                                    rb2.Add(0x00);
                                else if (cardType.Contains("升降机司机"))
                                    rb2.Add(0x01);
                                else if (cardType.Contains("监理卡"))
                                    rb2.Add(0x02);
                                else if (cardType.Contains("施工卡"))
                                    rb2.Add(0x03);
                                else rb2.Add(0x00);

                            }
                            else rb2.Add(0x00);
                            rb2.AddRange(new byte[] { 0xff, 0xff, 0xff, 0xff });
                        }
                        else
                        {
                            //长度 
                            rb2[6] = 11; rb2[7] = 0;
                            rb2.Add(0);
                        }
                    }
                    else//其它
                    {
                        //验证是否正确
                        DataTable dt = DB_MysqlTowerCrane.GetIdentifyInfo(AuthenticationTemp.EquipmentID,DriverCardNo2);  //获取司机相关信息
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            //长度 
                            rb2[6] = 81; rb2[7] = 0;
                            rb2.Add(1);
                            //工号
                            rb2.AddRange(FixedLengthFormat_byte(System.Text.Encoding.Default.GetBytes(dt.Rows[0]["empNo"].ToString().Trim()), 8, 0xff));
                            //卡号
                            rb2.AddRange(FixedLengthFormat_byte(System.Text.Encoding.Default.GetBytes(DriverCardNo2), 8, 0xff));
                            //身份证号
                            rb2.AddRange(FixedLengthFormat_byte(System.Text.Encoding.Default.GetBytes(dt.Rows[0]["code"].ToString().Trim()), 18, 0xff));
                            //电话
                            rb2.AddRange(FixedLengthFormat_byte(System.Text.Encoding.Default.GetBytes(dt.Rows[0]["telephone"].ToString().Trim()), 11, 0xff));
                            //姓名
                            rb2.AddRange(FixedLengthFormat_byte(Encoding.GetEncoding("GBK").GetBytes(dt.Rows[0]["name"].ToString().Trim()), 16, 0xff));
                            DateTime date = System.DateTime.Now;                  //有效期
                            int year = date.Year - 2000;
                            int month = date.Month;
                            int day = date.Day;
                            rb2.Add(0x20);
                            rb2.Add(fomaterByte(year));
                            rb2.Add(fomaterByte(month));
                            rb2.Add(fomaterByte(day));
                            string cardType = dt.Rows[0]["job"].ToString();
                            if (!string.IsNullOrEmpty(cardType))               //职位
                            {
                                if (cardType.Contains("塔吊司机卡"))
                                    rb2.Add(0x00);
                                else if (cardType.Contains("升降机司机"))
                                    rb2.Add(0x01);
                                else if (cardType.Contains("监理卡"))
                                    rb2.Add(0x02);
                                else if (cardType.Contains("施工卡"))
                                    rb2.Add(0x03);
                                else rb2.Add(0x00);
                            }
                            else rb2.Add(0x00);
                            rb2.AddRange(new byte[] { 0xff, 0xff, 0xff, 0xff });
                        }
                        else
                        {
                            //长度 
                            rb2[6] = 11; rb2[7] = 0;
                            rb2.Add(0);
                        }
                    }
                    //应答下发
                    client.SendBuffer(PackageFrameTail(rb2));
                    break;
                    #endregion
                    #endregion
                default: break;//特征码下发应答 平台删除应答 建立授权人信息 不需要做处理
            }
        }
        #endregion

        #region 离线数据
        private static void OnResolveOfflineData(byte[] frameDataLoad, byte[] frameHead, TcpSocketClient client)
        {
            List<byte> rb = new List<byte>();
            rb.AddRange(frameHead);//头
            rb.Add(0); rb.Add(0);//长度
            client.SendBuffer(PackageFrameTail(rb));
        }
        #endregion

        #region 参数信息上传
        //模拟数据:A5 5A 02 13 03 04 00 00 31 32 33 34 35 36 37 38 17 06 22 09 23 02 01 01 00 01 00 02 00 00 00 03 00 00 00 04 00 05 00 06 00 01 00 01 00 01 00 02 00 00 00 03 00 00 00 04 00 05 00 06 00 07 00 05 00 02 00 03 00 04 00 05 00 06 00 07 00 00 00 08 00 00 00 00 02 01 00 00 00 02 00 00 00 03 00 00 00 01 02 00 00 00 00 01 02 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 00 00 00 00 00 01 00 02 00 00 00 00 00 00 01 02 00 03 00 04 00 05 00 00 00 00 00 00 00 00 00 00 00 02 01 00 02 00 03 00 04 00 05 00 06 00 07 08 00 09 00 00 00 00 01 00 02 00 03 00 04 00 05 00 06 00 07 08 00 09 00 00 00 00 02 01 00 01 00 02 00 00 00 03 00 00 00 04 00 05 00 06 00 01 00 00 02 01 00 00 00 02 00 00 00 03 00 00 00 01 02 00 00 00 00 01 02 00 00 00 00 00 00 00 00 01 00 01 00 02 00 00 00 03 00 00 00 04 00 05 00 06 00 01 00 00 02 01 00 00 00 02 00 00 00 03 00 00 00 01 02 00 00 00 00 01 02 00 00 00 00 00 00 00 00 E2 20 EE FF
        private static void OnResolveParameterUpload(byte[] frameDataLoad, byte[] frameHead, TcpSocketClient client, ref DBFrame df)
        {
            try
            {
                ParameterUploadV021303 data = new ParameterUploadV021303();
                //设备编号
                byte[] sn = new byte[8];   //设备编号
                Array.Copy(frameDataLoad, 0, sn, 0, 8);
                data.EquipmentID = Encoding.ASCII.GetString(sn);  //获取设备编号ASCII
                //参数修改时间/RTC
                string tStr = ConvertData.ToHexString(frameDataLoad, 8, 6);
                data.ChangeRTC = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
                //修改标识
                data.ModifyIdentification = frameDataLoad[14];
                #region 高度设置
                byte[] byteTemp = new byte[20];
                Array.Copy(frameDataLoad, 15, byteTemp, 0, 20);
                if (byteTemp[0] == 0)
                {
                    ParameterUploadV021303.HeightSet_Potentiometer hp = new
                      ParameterUploadV021303.HeightSet_Potentiometer();
                    hp.SensorType = byteTemp[0];
                    hp.UpperLimitSampling = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", byteTemp[4], byteTemp[3], byteTemp[2], byteTemp[1]), 16);
                    hp.LowerLimitSampling = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", byteTemp[8], byteTemp[7], byteTemp[6], byteTemp[5]), 16);
                    hp.TowerBodyheight = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[10], byteTemp[9]), 16);
                    hp.ReductionSpeed = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[12], byteTemp[11]), 16);
                    hp.Limit = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[14], byteTemp[13]), 16);
                    hp.JackingMark = byteTemp[15];
                    data.HeightSet = hp;
                }
                else if (byteTemp[0] == 1)
                {
                    ParameterUploadV021303.HeightSet_Coder hc = new
                    ParameterUploadV021303.HeightSet_Coder();
                    hc.SensorType = byteTemp[0];
                    hc.SamplingAddDirection = byteTemp[1];
                    hc.InitialCylinderNumber = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[3], byteTemp[2]), 16);
                    hc.UpperLimitSampling = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", byteTemp[7], byteTemp[6], byteTemp[5], byteTemp[4]), 16);
                    hc.LowerLimitSampling = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", byteTemp[11], byteTemp[10], byteTemp[9], byteTemp[8]), 16);
                    hc.TowerBodyheight = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[13], byteTemp[12]), 16);
                    hc.ReductionSpeed = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[15], byteTemp[14]), 16);
                    hc.Limit = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[17], byteTemp[16]), 16);
                    hc.JackingMark = byteTemp[18];
                    data.HeightSet = hc;
                }
                #endregion
                #region 幅度设置
                byteTemp = new byte[20];
                Array.Copy(frameDataLoad, 35, byteTemp, 0, 20);
                if (byteTemp[0] == 0)
                {
                    ParameterUploadV021303.RangeSet_Potentiometer Rp = new
                      ParameterUploadV021303.RangeSet_Potentiometer();
                    Rp.SensorType = byteTemp[0];
                    Rp.InsideLimitSampling = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", byteTemp[4], byteTemp[3], byteTemp[2], byteTemp[1]), 16);
                    Rp.OutsideLimitSampling = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", byteTemp[8], byteTemp[7], byteTemp[6], byteTemp[5]), 16);
                    Rp.MinARange = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[10], byteTemp[9]), 16);
                    Rp.MaxARange = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[12], byteTemp[11]), 16);
                    Rp.ReductionSpeed = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[14], byteTemp[13]), 16);
                    Rp.Limit = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[16], byteTemp[15]), 16);
                    data.RangeSet = Rp;
                }
                else if (byteTemp[0] == 1)
                {
                    ParameterUploadV021303.RangeSet_Coder rc = new
                    ParameterUploadV021303.RangeSet_Coder();
                    rc.SensorType = byteTemp[0];
                    rc.SamplingAddDirection = byteTemp[1];
                    rc.InitialCylinderNumber = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[3], byteTemp[2]), 16);
                    rc.InsideLimitSampling = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", byteTemp[7], byteTemp[6], byteTemp[5], byteTemp[4]), 16);
                    rc.OutsideLimitSampling = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", byteTemp[11], byteTemp[10], byteTemp[9], byteTemp[8]), 16);
                    rc.MinARange = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[13], byteTemp[12]), 16);
                    rc.MaxARange = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[15], byteTemp[14]), 16);
                    rc.ReductionSpeed = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[17], byteTemp[16]), 16);
                    rc.Limit = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[19], byteTemp[18]), 16);
                    data.RangeSet = rc;
                }
                #endregion
                #region 回转设置
                byteTemp = new byte[20];
                Array.Copy(frameDataLoad, 55, byteTemp, 0, 20);
                if (byteTemp[0] == 1)
                {
                    ParameterUploadV021303.RotationSet_A ra = new
                      ParameterUploadV021303.RotationSet_A();
                    ra.RotationType = byteTemp[0];
                    ra.LeftLimitSampling = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", byteTemp[4], byteTemp[3], byteTemp[2], byteTemp[1]), 16);
                    ra.RightLimitSampling = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", byteTemp[8], byteTemp[7], byteTemp[6], byteTemp[5]), 16);
                    ra.EquipmentIDLeftAndRightLimitAngleSum = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[10], byteTemp[9]), 16);
                    ra.ReductionSpeed = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[12], byteTemp[11]), 16);
                    ra.Limit = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[14], byteTemp[13]), 16);
                    data.RotationSet = ra;
                }
                else if (byteTemp[0] == 2)
                {
                    ParameterUploadV021303.RotationSet_B rb = new
                      ParameterUploadV021303.RotationSet_B();
                    rb.RotationType = byteTemp[0];
                    rb.Direction = byteTemp[1];
                    rb.DirectionButtonGather = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", byteTemp[5], byteTemp[4], byteTemp[3], byteTemp[2]), 16);
                    rb.ConfirmButtonGather = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", byteTemp[9], byteTemp[8], byteTemp[7], byteTemp[6]), 16);
                    data.RotationSet = rb;
                }
                else if (byteTemp[0] == 3)
                {
                    ParameterUploadV021303.RotationSet_C rc = new
                      ParameterUploadV021303.RotationSet_C();
                    rc.RotationType = byteTemp[0];
                    rc.ZeroAngleSampling = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", byteTemp[4], byteTemp[3], byteTemp[2], byteTemp[1]), 16);
                    rc.SetAngleSampling = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", byteTemp[8], byteTemp[7], byteTemp[6], byteTemp[5]), 16);
                    rc.SetAngle = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[10], byteTemp[9]), 16);
                    rc.ReductionSpeed = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[12], byteTemp[11]), 16);
                    rc.Limit = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[14], byteTemp[13]), 16);
                    data.RotationSet = rc;
                }
                else if (byteTemp[0] == 4)
                {
                    ParameterUploadV021303.RotationSet_D rd = new
                      ParameterUploadV021303.RotationSet_D();
                    rd.RotationType = byteTemp[0];
                    rd.RelativeAbsoluteZeroIdentification = byteTemp[1];
                    rd.RotateDirection = byteTemp[2];
                    data.RotationSet = rd;
                }
                else if (byteTemp[0] == 5)
                {
                    ParameterUploadV021303.RotationSet_E re = new
                      ParameterUploadV021303.RotationSet_E();
                    re.RotationType = byteTemp[0];
                    re.SamplingAddDirection = byteTemp[1];
                    re.InitialCylinderNumber = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[3], byteTemp[2]), 16);
                    re.SensorDriveRatio = new ushort[] { Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[5], byteTemp[4]), 16), Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[7], byteTemp[6]), 16) };
                    re.TowerTeeth = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[9], byteTemp[8]), 16);
                    re.SensorTeeth = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[11], byteTemp[10]), 16);
                    re.ZeroSensor = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", byteTemp[15], byteTemp[14], byteTemp[13], byteTemp[12]), 16);
                    re.anticlockwiseSensor = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", byteTemp[19], byteTemp[18], byteTemp[17], byteTemp[16]), 16);
                    data.RotationSet = re;
                }
                #endregion
                #region 重量设置
                byteTemp = new byte[20];
                Array.Copy(frameDataLoad, 75, byteTemp, 0, 20);
                if (byteTemp[0] == 0)
                {
                    ParameterUploadV021303.WeightSet_AnalogSignal wa = new
                   ParameterUploadV021303.WeightSet_AnalogSignal();
                    wa.SensorType = byteTemp[0];
                    wa.SetRate = byteTemp[1];
                    wa.EmptyHookSampling = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", byteTemp[5], byteTemp[4], byteTemp[3], byteTemp[2]), 16);
                    wa.CraneWeightSampling = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", byteTemp[9], byteTemp[8], byteTemp[7], byteTemp[6]), 16);
                    wa.WeightWeight = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", byteTemp[13], byteTemp[12], byteTemp[11], byteTemp[10]), 16);
                    wa.ThrowOverWeight = byteTemp[14];
                    wa.CutOffWeight = byteTemp[15];
                    data.WeightSet = wa;
                }
                #endregion
                #region 力矩设置
                byteTemp = new byte[10];
                Array.Copy(frameDataLoad, 95, byteTemp, 0, 10);
                ParameterUploadV021303.CMomentSet cm = new
                   ParameterUploadV021303.CMomentSet();
                cm.ThrowOverMomentSet = byteTemp[0];
                cm.CutOffMomentSet = byteTemp[1];
                data.MomentSet = cm;
                #endregion
                #region 风速设置
                byteTemp = new byte[10];
                Array.Copy(frameDataLoad, 105, byteTemp, 0, 10);
                ParameterUploadV021303.CWindSpeedSet cws = new
                   ParameterUploadV021303.CWindSpeedSet();
                cws.WindSpeedType = byteTemp[0];
                cws.WindSpeedUnit = byteTemp[1];
                cws.WindSpeedWarning = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[3], byteTemp[2]), 16);
                cws.WindSpeedAlarm = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[5], byteTemp[4]), 16);
                data.WindSpeedSet = cws;
                #endregion
                #region 倾角设置
                byteTemp = new byte[10];
                Array.Copy(frameDataLoad, 115, byteTemp, 0, 10);
                ParameterUploadV021303.CDipAngleSet cds = new
                   ParameterUploadV021303.CDipAngleSet();
                cds.DipAngleType = byteTemp[0];
                cds.DipAngleRelativelyZeroFlag = byteTemp[1];
                cds.DipAngleWarning = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[3], byteTemp[2]), 16);
                cds.DipAngleAlarm = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[5], byteTemp[4]), 16);
                data.DipAngleSet = cds;
                #endregion
                #region 仰角设置
                byteTemp = new byte[20];
                Array.Copy(frameDataLoad, 125, byteTemp, 0, 20);
                ParameterUploadV021303.CBoomAngleSet cbs = new
                   ParameterUploadV021303.CBoomAngleSet();
                cbs.BoomAngleType = byteTemp[0];
                cbs.BoomAngleRelativelyZeroFlag = byteTemp[1];
                cbs.BoomAngleMin = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[3], byteTemp[2]), 16);
                cbs.BoomAngleMax = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[5], byteTemp[4]), 16);
                cbs.BoomAngleThrowOver = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[7], byteTemp[6]), 16);
                cbs.BoomAngleLimit = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[9], byteTemp[8]), 16);
                data.BoomAngleSet = cbs;
                #endregion
                //塔吊眼数量
                data.CraneEyeNumber = frameDataLoad[145];
                #region 塔吊眼参数
                if (data.CraneEyeNumber == 0)
                {
                    data.CraneEyeParameter = null;
                }
                else
                {
                    data.CraneEyeParameter = new ParameterUploadV021303.CCraneEyeParameter[data.CraneEyeNumber];
                    for (int i = 0; i < data.CraneEyeNumber; i++)
                    {
                        try
                        {
                            byteTemp = new byte[20];
                            Array.Copy(frameDataLoad, 146 + i * 20, byteTemp, 0, 20);
                            ParameterUploadV021303.CCraneEyeParameter CCP = new ParameterUploadV021303.CCraneEyeParameter();
                            #region 协议解析
                            CCP.HorizontalAngle = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[1], byteTemp[0]), 16);
                            CCP.AngleOfPitch = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[3], byteTemp[2]), 16);
                            CCP.SpeedDomeCamerasMagnification = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[5], byteTemp[4]), 16);
                            CCP.TargetAltitude = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[7], byteTemp[6]), 16);
                            CCP.FocusingFactor = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[9], byteTemp[8]), 16);
                            CCP.LongArm = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[11], byteTemp[10]), 16);
                            CCP.Algorithm = byteTemp[12];
                            CCP.FocusingMin = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[14], byteTemp[13]), 16);
                            CCP.FocusingMax = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[16], byteTemp[15]), 16);
                            #endregion
                            data.CraneEyeParameter[i] = CCP;
                        }
                        catch (Exception) { }
                    }
                }
                #endregion
                //副臂个数
                data.ViceHookCount = frameDataLoad[146 + 20 * data.CraneEyeNumber];
                #region 副臂设置参数
                if (data.ViceHookCount == 0)
                {
                    data.ViceHookSetParameter = null;
                }
                else
                {
                    data.ViceHookSetParameter = new ParameterUploadV021303.CViceHookSetParameter[data.ViceHookCount];
                    for (int i = 0; i < data.ViceHookCount; i++)
                    {
                        try
                        {
                            byteTemp = new byte[50];
                            Array.Copy(frameDataLoad, 147 + 20 * data.CraneEyeNumber + i * 50, byteTemp, 0, 50);
                            ParameterUploadV021303.CViceHookSetParameter CVP = new ParameterUploadV021303.CViceHookSetParameter();
                            #region 协议解析
                            //高度设置
                            #region 高度设置
                            byte[] byteResult = new byte[20];
                            Array.Copy(byteTemp, 0, byteResult, 0, 20);
                            if (byteResult[0] == 0)
                            {
                                ParameterUploadV021303.HeightSet_Potentiometer hp = new
                                  ParameterUploadV021303.HeightSet_Potentiometer();
                                hp.SensorType = byteResult[0];
                                hp.UpperLimitSampling = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", byteResult[4], byteResult[3], byteResult[2], byteResult[1]), 16);
                                hp.LowerLimitSampling = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", byteResult[8], byteResult[7], byteResult[6], byteResult[5]), 16);
                                hp.TowerBodyheight = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteResult[10], byteResult[9]), 16);
                                hp.ReductionSpeed = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteResult[12], byteResult[11]), 16);
                                hp.Limit = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteResult[14], byteResult[13]), 16);
                                hp.JackingMark = byteResult[15];
                                CVP.HeightSet = hp;
                            }
                            else if (byteResult[0] == 1)
                            {
                                ParameterUploadV021303.HeightSet_Coder hc = new
                                ParameterUploadV021303.HeightSet_Coder();
                                hc.SensorType = byteResult[0];
                                hc.SamplingAddDirection = byteResult[1];
                                hc.InitialCylinderNumber = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteResult[3], byteResult[2]), 16);
                                hc.UpperLimitSampling = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", byteResult[7], byteResult[6], byteResult[5], byteResult[4]), 16);
                                hc.LowerLimitSampling = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", byteResult[11], byteResult[10], byteResult[9], byteResult[8]), 16);
                                hc.TowerBodyheight = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteResult[13], byteResult[12]), 16);
                                hc.ReductionSpeed = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteResult[15], byteResult[14]), 16);
                                hc.Limit = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteResult[17], byteResult[16]), 16);
                                hc.JackingMark = byteResult[18];
                                CVP.HeightSet = hc;
                            }
                            #endregion
                            //重量设置
                            #region 重量设置
                            byteResult = new byte[20];
                            Array.Copy(byteTemp, 20, byteResult, 0, 20);
                            if (byteResult[0] == 0)
                            {
                                ParameterUploadV021303.WeightSet_AnalogSignal wa = new
                               ParameterUploadV021303.WeightSet_AnalogSignal();
                                wa.SensorType = byteResult[0];
                                wa.SetRate = byteResult[1];
                                wa.EmptyHookSampling = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", byteResult[5], byteResult[4], byteResult[3], byteResult[2]), 16);
                                wa.CraneWeightSampling = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", byteResult[9], byteResult[8], byteResult[7], byteResult[6]), 16);
                                wa.WeightWeight = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", byteResult[13], byteResult[12], byteResult[11], byteResult[10]), 16);
                                wa.ThrowOverWeight = byteResult[14];
                                wa.CutOffWeight = byteResult[15];
                                CVP.WeightSet = wa;
                            }
                            #endregion
                            //力矩设置
                            #region 力矩设置
                            byteResult = new byte[10];
                            Array.Copy(byteTemp, 40, byteResult, 0, 10);
                            ParameterUploadV021303.CMomentSet cmm = new
                               ParameterUploadV021303.CMomentSet();
                            cmm.ThrowOverMomentSet = byteResult[0];
                            cmm.CutOffMomentSet = byteResult[1];
                            CVP.MomentSet = cmm;
                            #endregion
                            #endregion
                            data.ViceHookSetParameter[i] = CVP;
                        }
                        catch (Exception) { }
                    }
                }
                #endregion
                #region
                //应答
                List<byte> rb1 = new List<byte>();
                rb1.AddRange(frameHead);//头
                rb1.Add(0); rb1.Add(0);//长度
                client.SendBuffer(PackageFrameTail(rb1));

                //存入数据库
                df.deviceid = data.EquipmentID;
                df.datatype = "parameterUpload";
                df.contentjson = JsonConvert.SerializeObject(data);
                #endregion
            }
            catch (Exception) { };
        }
        #endregion

        #region IP地址配置
        //模拟接受帧（立即应答）：A5 5A 02 13 03 05 00 00 31 32 33 34 35 36 37 38 15 90 EE FF
        //模拟接受帧（不成功后应答）：A5 5A 02 13 03 05 00 00 31 32 33 34 35 36 37 38 00 B9 96 EE FF
        private static void OnResolveIPConfigure(byte[] frameDataLoad)
        {
            try
            {
                IPConfigureV021303 data = new IPConfigureV021303();
                //设备编号
                byte[] sn = new byte[8];   //设备编号
                Array.Copy(frameDataLoad, 0, sn, 0, 8);
                data.EquipmentID = Encoding.ASCII.GetString(sn);  //获取设备编号ASCII
                if (frameDataLoad.Length == 8)
                {
                    data.ResultStatus = 2;
                }
                else if (frameDataLoad.Length == 9)
                {
                    data.ResultStatus = 3;
                    data.IdentificationType = frameDataLoad[8];
                }
                else { return; }
                //存入到数据库
                DB_MysqlTowerCrane.SaveIPConfigure(data);
            }
            catch (Exception) { }
        }
        /// <summary>
        /// ip命令下发
        /// </summary>
        /// <param name="obj"></param>
        public static void IPConfigureSendingEp(List<object> obj)
        {
            //List<object> temp = new List<object> { resultTemp.TVersion, resultTemp.EquipmentID, resultTemp.Ip, resultTemp.Port, resultTemp.TcpClient };
            //测试数据：A5 5A 02 13 03 05 11 00 0b 31 30 2e 31 30 2e 31 30 2e 38 35 04 35 30 30 30 df 91 ee ff
            try
            {
                //ip
                string ip = obj[2].ToString().Trim();
                //端口
                string port = obj[3].ToString().Trim();
                List<byte> rb = new List<byte>();
                //头
                rb.Add(0xA5); rb.Add(0x5A);
                //版本号
                rb.Add(0x02); rb.Add(0x13); rb.Add(0x03);
                //命令字
                rb.Add(0x05);
                //数据长度
                rb.Add((byte)(ip.Length + port.Length + 2)); rb.Add(0x00);
                //数据区
                rb.Add((byte)ip.Length);//ip长度
                byte[] byteArray = System.Text.Encoding.Default.GetBytes(ip);
                rb.AddRange(byteArray);//ip得ASCII码
                rb.Add((byte)port.Length);//port长度
                byteArray = System.Text.Encoding.Default.GetBytes(port);
                rb.AddRange(byteArray);//port得ASCII码
                //封装并发送
                TcpSocketClient tcpTemp = obj[4] as TcpSocketClient;
                byte[] sendRev = PackageFrameTail(rb);
                tcpTemp.SendBuffer(sendRev);
                XMLOperation.WriteLogXmlNoTail("IP命令下发", ConvertData.ToHexString(sendRev, 0, sendRev.Length));
            }
            catch (Exception) { }

        }
        #endregion

        #region 设备运行时间
        //模拟接受帧：A5 5A 02 13 03 07 00 00 31 32 33 34 35 36 37 38 05 00 00 00 02 00 00 00 3F 2B EE FF
        //应答：A5 5A 02 13 03 07 00 00 00 00 ee ff
        private static void OnResolveRuntimeEp(byte[] frameDataLoad, byte[] frameHead, TcpSocketClient client, ref DBFrame df)
        {
            try
            {
                RuntimeEpV021303 data = new RuntimeEpV021303();
                //设备编号
                byte[] sn = new byte[8];   //设备编号
                Array.Copy(frameDataLoad, 0, sn, 0, 8);
                data.EquipmentID = Encoding.ASCII.GetString(sn);  //获取设备编号ASCII
                //总运行时间
                data.TotalRuntime = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", frameDataLoad[11], frameDataLoad[10], frameDataLoad[9], frameDataLoad[8]), 16);
                //本机开机运行时间
                data.StartingUpRuntime = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", frameDataLoad[15], frameDataLoad[14], frameDataLoad[13], frameDataLoad[12]), 16);
                #region
                //应答
                List<byte> rb1 = new List<byte>();
                rb1.AddRange(frameHead);//头
                rb1.Add(0); rb1.Add(0);//长度
                client.SendBuffer(PackageFrameTail(rb1));

                //存入数据库
                df.deviceid = data.EquipmentID;
                df.datatype = "runtimeEp";
                df.contentjson = JsonConvert.SerializeObject(data);
                #endregion
            }
            catch (Exception) { }
        }
        #endregion

        #region 命令下发
        //模拟接受帧：A5 5A 02 13 03 08 0c 00 31 32 33 34 35 36 37 38 00 00 00 00 0c 6e ee ff 
        //应答：
        private static void OnResolveCommandIssued(byte[] frameDataLoad)
        {
            try
            {
                CommandIssuedV021303 data = new CommandIssuedV021303();
                //设备编号
                byte[] sn = new byte[8];   //设备编号
                Array.Copy(frameDataLoad, 0, sn, 0, 8);
                data.EquipmentID = Encoding.ASCII.GetString(sn);  //获取设备编号ASCII
                //确认标识
                data.IdentificationMark = frameDataLoad[8];
                //命令字
                data.Subcommand = frameDataLoad[9];
                //具体参数
                if (data.Subcommand == 0)
                {//功能配置
                    CommandIssuedV021303.CFunctionConfiguration fc = new CommandIssuedV021303.CFunctionConfiguration();
                    fc.FunctionConfiguration = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", frameDataLoad[11], frameDataLoad[10]), 16);
                    data.SpecificParameter = fc;
                }
                else if (data.Subcommand == 1)
                {//限位配置
                    CommandIssuedV021303.CLimitControl lc = new CommandIssuedV021303.CLimitControl();
                    lc.LimitControl = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", frameDataLoad[13], frameDataLoad[12], frameDataLoad[11], frameDataLoad[10]), 16);
                    data.SpecificParameter = lc;
                }
                else if (data.Subcommand == 2)
                {//时间校准
                    CommandIssuedV021303.CTimeCalibration tc = new CommandIssuedV021303.CTimeCalibration();
                    string tStr = ConvertData.ToHexString(frameDataLoad, 10, 6);
                    tc.TimeCalibration = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
                    data.SpecificParameter = tc;
                }
                else return;
                #region
                //数据库更新
                DB_MysqlTowerCrane.SaveCommandIssued(data);
                #endregion
            }
            catch (Exception) { }
        }
        /// <summary>
        /// 命令下发
        /// </summary>
        /// <param name="obj"></param>
        public static void CommandIssuedSendingEp(List<object> obj)
        {
            //List<object> temp = new List<object> { resultTemp.TVersion, resultTemp.EquipmentID, resultTemp.ct_cmdValue, resultTemp.ct_paramConfig, resultTemp.ct_state, resultTemp.TcpClient };

            //测试数据：A5 5A 02 13 03 08 0c 00 31 32 33 34 35 36 37 38 00 00 00 00 0c 6e ee ff
            try
            {
                //命令字
                byte Subcommand = byte.Parse(obj[2].ToString());

                //确认标识
                byte IdentificationMark = 0;

                if(obj[4].ToString()=="0"||obj[4].ToString()=="1")
                    IdentificationMark = 0;
                else if(obj[4].ToString()=="3"||obj[4].ToString()=="4")
                    IdentificationMark = 1;
                if (Subcommand == 2) IdentificationMark = 1;
                //具体参数
                object SpecificParameter = obj[3];
                List<byte> rb = new List<byte>();
                //头
                rb.Add(0xA5); rb.Add(0x5A);
                //版本号
                rb.Add(0x02); rb.Add(0x13); rb.Add(0x03);
                //命令字
                rb.Add(0x08);
                //数据长度
                rb.Add(0); rb.Add(0x00);
                //数据区
                byte[] byteArray = System.Text.Encoding.Default.GetBytes(obj[1].ToString());
                rb.AddRange(byteArray);//设备编号
                rb.Add(IdentificationMark);//身份识别方式
                rb.Add(Subcommand);//子命令字
                //具体参数
                if (Subcommand == 0)
                {//功能配置
                    ushort testcom =  Convert.ToUInt16(SpecificParameter.ToString(),2);
                    rb.AddRange(ToolAPI.ValueTypeToByteArray.GetBytes_LittleEndian(testcom));
                    rb[6] = 12;
                }
                else if (Subcommand == 1)
                {//限位配置
                    if (uint.Parse(SpecificParameter.ToString())==0)
                    { 
                        rb.Add(0x00);rb.Add(0x00);rb.Add(0x00);rb.Add(0x00);
                    }
                    else if (uint.Parse(SpecificParameter.ToString())==1)
                    {
                        rb.Add(0x00); rb.Add(0x00); rb.Add(0x00); rb.Add(0x80);
                    }
                    //rb.AddRange(ValueTypeToByteArray.GetBytes_LittleEndian(uint.Parse(SpecificParameter.ToString())));
                    rb[6] = 14;
                }
                else if (Subcommand == 2)
                {//时间校准
                    DateTime dt = DateTime.Now;
                    rb.Add(byte.Parse(dt.Year.ToString().Substring(2, 2), System.Globalization.NumberStyles.HexNumber));
                    rb.Add(byte.Parse(dt.Month.ToString(), System.Globalization.NumberStyles.HexNumber));
                    rb.Add(byte.Parse(dt.Day.ToString(), System.Globalization.NumberStyles.HexNumber));
                    rb.Add(byte.Parse(dt.Hour.ToString(), System.Globalization.NumberStyles.HexNumber));
                    rb.Add(byte.Parse(dt.Minute.ToString(), System.Globalization.NumberStyles.HexNumber));
                    rb.Add(byte.Parse(dt.Second.ToString(), System.Globalization.NumberStyles.HexNumber));
                    rb[6] = 16;
                }
                else return;
                //封装并发送
                TcpSocketClient tcpTemp = obj[5] as TcpSocketClient;
                byte[] sendRev = PackageFrameTail(rb);
                tcpTemp.SendBuffer(sendRev);
                XMLOperation.WriteLogXmlNoTail("控制命令下发：" + Subcommand, ConvertData.ToHexString(sendRev, 0, sendRev.Length));
            }
            catch (Exception) { }

        }
        #endregion

        #region 信息上传
        //模拟接受帧：A5 5A 02 13 03 09 00 00 31 32 33 34 35 36 37 38 17 03 06 10 08 20 00 00 79 26 EE FF
        //应答：A5 5A 02 13 03 09 09 00 31 32 33 34 35 36 37 38 00 b9 96 ee ff 
        private static void OnResolveInformationUpload(byte[] frameDataLoad, byte[] frameHead, TcpSocketClient client, ref DBFrame df)
        {
            try
            {
                InformationUploadV021303 data = new InformationUploadV021303();
                //设备编号
                byte[] sn = new byte[8];   //设备编号
                Array.Copy(frameDataLoad, 0, sn, 0, 8);
                data.EquipmentID = Encoding.ASCII.GetString(sn);  //获取设备编号ASCII
                try
                {
                    //RTC
                    string tStr = ConvertData.ToHexString(frameDataLoad, 8, 6);
                    data.RTC = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
                }
                catch (Exception) { }
                //信息类型
                data.InformationType = frameDataLoad[14];
                //信息码
                data.InformationCode = frameDataLoad[15];
                #region
                //应答
                List<byte> rb1 = new List<byte>();
                rb1.AddRange(frameHead);//头
                rb1.Add(9); rb1.Add(0);//长度
                rb1.AddRange(sn);//设备编号
                rb1.Add(data.InformationType);//信息类型
                client.SendBuffer(PackageFrameTail(rb1));
                //存入数据库
                df.deviceid = data.EquipmentID;
                df.datatype = "informationUpload";
                df.contentjson = JsonConvert.SerializeObject(data);
                #endregion
            }
            catch (Exception) { }
        }
        #endregion

        #region 黑匣子信息
        //模拟接受帧：A5 5A 02 13 03 0A 00 00 31 32 33 34 35 36 37 38 00 00 01 00 03 00 00 00 01 04 00 00 00 00 00 01 02 00 05 06 01 01 01 02 02 02 03 03 03 04 04 04 35 35 35 35 4E 0D EE FF
        //应答：a5 5a 02 13 03 0a 00 00 00 00 ee ff
        private static void OnResolveBlackBox(byte[] frameDataLoad, byte[] frameHead, TcpSocketClient client, ref DBFrame df)
        {
            try
            {
                BlackBoxV021303 data = new BlackBoxV021303();
                //设备编号
                byte[] sn = new byte[8];   //设备编号
                Array.Copy(frameDataLoad, 0, sn, 0, 8);
                data.EquipmentID = Encoding.ASCII.GetString(sn);  //获取设备编号ASCII
                //功能锁定标识
                data.FunctionLockIdentifier = frameDataLoad[8];
                //功能配置
                data.FunctionConfiguration = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", frameDataLoad[10], frameDataLoad[9]), 16);
                #region 经纬度
                byte[] byteTemp = new byte[10];
                Array.Copy(frameDataLoad, 11, byteTemp, 0, 10);
                BlackBoxV021303.CLongitudeAndLatitude ll = new BlackBoxV021303.CLongitudeAndLatitude();
                ll.LatitudeType = byteTemp[0];
                ll.Latitude = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", byteTemp[4], byteTemp[3], byteTemp[2], byteTemp[1]), 16);
                ll.LongitudeType = byteTemp[5];
                ll.Longitude = Convert.ToUInt32(string.Format("{0:X2}{1:X2}{2:X2}{3:X2}", byteTemp[9], byteTemp[8], byteTemp[7], byteTemp[6]), 16);
                data.LongitudeAndLatitude = ll;
                #endregion
                //身份认证方式锁定标识
                data.IdentificationTypeLockIdentifier = frameDataLoad[21];
                //身份认证方式
                data.IdentificationType = frameDataLoad[22];
                //身身份识别周期开关状态
                data.IdentificationCycleSwitchState = frameDataLoad[23];
                //身身份识别周期
                data.IdentificationCycle = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", frameDataLoad[25], frameDataLoad[24]), 16);
                //亮度设置
                data.BrightnessSetting = new byte[] { frameDataLoad[26], frameDataLoad[27] };
                //Zigbee版本
                data.Version_Zigbee = ConvertData.ToHexString(frameDataLoad, 28, 3);
                //继电器版本
                data.Version_Relay = ConvertData.ToHexString(frameDataLoad, 31, 3);
                //从机版本
                data.Version_Counterpart = ConvertData.ToHexString(frameDataLoad, 34, 3);
                //身份证版本
                data.Version_IDCard = ConvertData.ToHexString(frameDataLoad, 37, 3);
                //软件版本
                sn = new byte[frameDataLoad.Length - 40];   //设备编号
                Array.Copy(frameDataLoad, 40, sn, 0, frameDataLoad.Length - 40);
                data.Version_Software = Encoding.ASCII.GetString(sn);
                #region
                //应答
                List<byte> rb1 = new List<byte>();
                rb1.AddRange(frameHead);//头
                rb1.Add(0); rb1.Add(0);//长度
                client.SendBuffer(PackageFrameTail(rb1));
                //存入数据库
                df.deviceid = data.EquipmentID;
                df.datatype = "blackBox";
                df.contentjson = JsonConvert.SerializeObject(data);
                #endregion
            }
            catch (Exception) { }
        }
        #endregion

        #region 塔吊基本参数
        //模拟接受帧：A5 5A 02 13 03 0B 00 00 31 32 33 34 35 36 37 38 00 01 00 02 00 03 00 04 00 05 00 00 02 02 02 01 00 02 00 01 00 03 00 02 02 03 00 04 00 01 00 03 00 02 01 00 02 00 00 02 02 02 01 00 02 00 01 00 03 00 02 02 03 00 04 00 01 00 03 00 01 00 02 00 01 02 02 01 00 02 00 03 00 04 00 02 01 00 02 00 03 00 04 00 8C A9 EE FF
        //应答：a5 5a 02 13 03 0b 00 00 00 00 ee ff
        private static void OnResolveTowerCraneBasicInformation(byte[] frameDataLoad, byte[] frameHead, TcpSocketClient client, ref DBFrame df)
        {
            try
            {
                TowerCraneBasicInformationV021303 data = new TowerCraneBasicInformationV021303();
                //设备编号
                byte[] sn = new byte[8];   //设备编号
                Array.Copy(frameDataLoad, 0, sn, 0, 8);
                data.EquipmentID = Encoding.ASCII.GetString(sn);  //获取设备编号ASCII
                //塔吊类型
                data.TowerCraneType = frameDataLoad[8];
                //起重臂长度
                data.BoomArmLength = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", frameDataLoad[10], frameDataLoad[9]), 16);
                //平衡臂长度
                data.BalanceArmLength = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", frameDataLoad[12], frameDataLoad[11]), 16);
                //塔身高度
                data.TowerBodyHeight = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", frameDataLoad[14], frameDataLoad[13]), 16);
                //塔帽高度（平臂）/最大仰角（动臂）
                data.towerCapORElevation = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", frameDataLoad[16], frameDataLoad[15]), 16);
                //最小仰角
                data.MinBoomAngle = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", frameDataLoad[18], frameDataLoad[17]), 16);
                //力矩曲线类型
                data.MomentCurveType = frameDataLoad[19];
                //力矩曲线数量
                data.MomentCurveCount = frameDataLoad[20];
                int MomentCurveSetDatacount = 0;
                #region 力矩曲线设置数据
                data.MomentCurveSet = new object[data.MomentCurveCount];
                for (int i = 0; i < data.MomentCurveCount; i++)
                {
                    if (data.MomentCurveType == 1)
                    {
                        byte[] byteTemp = new byte[9];
                        //MomentCurveSetDatacount = 9;
                        MomentCurveSetDatacount += 9;
                        Array.Copy(frameDataLoad, 21+9*i, byteTemp, 0, 9);
                        TowerCraneBasicInformationV021303.MomentCurveSet_Curve mc = new TowerCraneBasicInformationV021303.MomentCurveSet_Curve();
                        mc.Rate = byteTemp[0];
                        mc.MaxWeight = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[2], byteTemp[1]), 16);
                        mc.MaxWeightRange = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[4], byteTemp[3]), 16);
                        mc.MaxRange = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[6], byteTemp[5]), 16);
                        mc.MaxRangeWeight = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[8], byteTemp[7]), 16);
                        data.MomentCurveSet[i] = mc;
                    }
                    else if (data.MomentCurveType == 0)
                    {
                        TowerCraneBasicInformationV021303.MomentCurveSet_Icon mi = new TowerCraneBasicInformationV021303.MomentCurveSet_Icon();
                        mi.Rate = frameDataLoad[21 + MomentCurveSetDatacount];
                        mi.CurveSetPointCount = frameDataLoad[22 + MomentCurveSetDatacount];
                        int loopCount = MomentCurveSetDatacount;
                        MomentCurveSetDatacount += 2 + 4 * mi.CurveSetPointCount;
                        if (mi.CurveSetPointCount == 0)
                        {
                            //MomentCurveSetDatacount = 1;
                            mi.MomentCurve_IconAry = null;
                        }
                        else
                        {
                            //MomentCurveSetDatacount = 1 + mi.CurveSetPointCount * 4;
                            mi.MomentCurve_IconAry = new TowerCraneBasicInformationV021303.MomentCurveSet_Icon.MomentCurve_Icon[mi.CurveSetPointCount];
                            for (int j = 0; j < mi.CurveSetPointCount; j++)
                            {
                                try
                                {
                                    byte[] byteTemp = new byte[4];
                                    Array.Copy(frameDataLoad, 23 + loopCount + j * 4, byteTemp, 0, 4);
                                    TowerCraneBasicInformationV021303.MomentCurveSet_Icon.MomentCurve_Icon miT = new TowerCraneBasicInformationV021303.MomentCurveSet_Icon.MomentCurve_Icon();
                                    //起重量
                                    miT.Weight = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[1], byteTemp[0]), 16);
                                    //起重量幅度
                                    miT.WeightRange = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[3], byteTemp[2]), 16);
                                    mi.MomentCurve_IconAry[j] = miT;
                                }
                                catch (Exception) { }
                            }
                        }
                        data.MomentCurveSet[i] = mi;
                    }
                }
                #endregion
                //副臂个数
                data.ViceHookCount = frameDataLoad[21 + MomentCurveSetDatacount];
                #region 副钩参数
                if (data.ViceHookCount == 0)
                {
                    data.ViceHookMessage = null;
                }
                else
                {
                    data.ViceHookMessage = new TowerCraneBasicInformationV021303.ViceHook[data.ViceHookCount];
                    int viceHookcountTemp = MomentCurveSetDatacount;
                    for (int i = 0; i < data.ViceHookCount; i++)
                    {
                        try
                        {
                            int indexTemp = 22 + viceHookcountTemp;
                            TowerCraneBasicInformationV021303.ViceHook viceHookTemp = new TowerCraneBasicInformationV021303.ViceHook();
                            //副臂长
                            viceHookTemp.ViceArmLength = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", frameDataLoad[indexTemp + 1], frameDataLoad[indexTemp + 0]), 16);
                            //副臂夹角
                            viceHookTemp.ViceArmIntersectionAngle = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", frameDataLoad[indexTemp + 3], frameDataLoad[indexTemp + 2]), 16);
                            //力矩曲线类型
                            viceHookTemp.MomentCurveType = frameDataLoad[indexTemp + 4];
                            //力矩曲线数量
                            #region 力矩曲线设置数据
                            viceHookTemp.MomentCurveCount = frameDataLoad[indexTemp + 5];
                            int countFlagC = indexTemp + 6;//标识下面的力矩曲线的起始位
                            int MomentCurveSetDatacountTemp = 0;
                            viceHookTemp.MomentCurveSet = new object[viceHookTemp.MomentCurveCount];
                            for (int j = 0; j < viceHookTemp.MomentCurveCount; j++)
                            {
                                if (viceHookTemp.MomentCurveType == 1)
                                {
                                    byte[] byteTemp = new byte[9];
                                    //MomentCurveSetDatacount = 9;
                                    MomentCurveSetDatacountTemp += 9;
                                    Array.Copy(frameDataLoad, countFlagC + 9 * j, byteTemp, 0, 9);
                                    TowerCraneBasicInformationV021303.MomentCurveSet_Curve mc = new TowerCraneBasicInformationV021303.MomentCurveSet_Curve();
                                    mc.Rate = byteTemp[0];
                                    mc.MaxWeight = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[2], byteTemp[1]), 16);
                                    mc.MaxWeightRange = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[4], byteTemp[3]), 16);
                                    mc.MaxRange = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[6], byteTemp[5]), 16);
                                    mc.MaxRangeWeight = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[8], byteTemp[7]), 16);
                                    viceHookTemp.MomentCurveSet[j] = mc;
                                }
                                else if (viceHookTemp.MomentCurveType == 0)
                                {
                                    TowerCraneBasicInformationV021303.MomentCurveSet_Icon mi = new TowerCraneBasicInformationV021303.MomentCurveSet_Icon();
                                    mi.Rate = frameDataLoad[countFlagC + MomentCurveSetDatacountTemp];
                                    mi.CurveSetPointCount = frameDataLoad[countFlagC + 1 + MomentCurveSetDatacountTemp];
                                    int loopCount = MomentCurveSetDatacountTemp;
                                    MomentCurveSetDatacountTemp += 2 + 4 * mi.CurveSetPointCount;
                                    if (mi.CurveSetPointCount == 0)
                                    {
                                        //MomentCurveSetDatacount = 1;
                                        mi.MomentCurve_IconAry = null;
                                    }
                                    else
                                    {
                                        //MomentCurveSetDatacount = 1 + mi.CurveSetPointCount * 4;
                                        mi.MomentCurve_IconAry = new TowerCraneBasicInformationV021303.MomentCurveSet_Icon.MomentCurve_Icon[mi.CurveSetPointCount];
                                        for (int q = 0; q < mi.CurveSetPointCount; q++)
                                        {
                                            try
                                            {
                                                byte[] byteTemp = new byte[4];
                                                Array.Copy(frameDataLoad, countFlagC + 2 + loopCount + q * 4, byteTemp, 0, 4);
                                                TowerCraneBasicInformationV021303.MomentCurveSet_Icon.MomentCurve_Icon miT = new TowerCraneBasicInformationV021303.MomentCurveSet_Icon.MomentCurve_Icon();
                                                //起重量
                                                miT.Weight = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[1], byteTemp[0]), 16);
                                                //起重量幅度
                                                miT.WeightRange = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[3], byteTemp[2]), 16);
                                                mi.MomentCurve_IconAry[q] = miT;
                                            }
                                            catch (Exception) { }
                                        }
                                    }
                                    viceHookTemp.MomentCurveSet[j] = mi;
                                }
                            }
                            viceHookcountTemp += 6 + MomentCurveSetDatacountTemp;
                            data.ViceHookMessage[i] = viceHookTemp;
                            #endregion
                        }
                        catch (Exception) { }
                    }
                }
                #endregion
                #region
                //应答
                List<byte> rb1 = new List<byte>();
                rb1.AddRange(frameHead);//头
                rb1.Add(0); rb1.Add(0);//长度
                client.SendBuffer(PackageFrameTail(rb1));
                //存入数据库
                df.deviceid = data.EquipmentID;
                df.datatype = "towerCraneBasicInformation";
                df.contentjson = JsonConvert.SerializeObject(data);
                #endregion
            }
            catch (Exception) { }
        }
        #endregion

        #region 防碰撞设置
        //模拟接受帧：A5 5A 02 13 03 0C 00 00 31 32 33 34 35 36 37 38 01 02 03 01 02 03 04 05 06 07 08 00 00 00 00 01 01 00 01 00 02 00 03 00 04 00 05 00 06 00 07 00 08 00 99 61 EE FF
        //应答：a5 5a 02 13 03 0c 00 00 00 00 ee ff
        private static void OnResolvePreventCollision(byte[] frameDataLoad, byte[] frameHead, TcpSocketClient client, ref DBFrame df)
        {
            try
            {
                PreventCollisionV021303 data = new PreventCollisionV021303();
                //设备编号
                byte[] sn = new byte[8];   //设备编号
                Array.Copy(frameDataLoad, 0, sn, 0, 8);
                data.EquipmentID = Encoding.ASCII.GetString(sn);  //获取设备编号ASCII
                //编号
                data.Number = frameDataLoad[8];
                //频道号
                data.ChannelNo = frameDataLoad[9];
                //组号
                data.GroupNo = frameDataLoad[10];
                //通信编号映射
                data.CommunicationNoMapped = new byte[8];
                for(int i=0;i<8;i++) data.CommunicationNoMapped[i] = frameDataLoad[11+i];
                //防碰撞预警
                data.PreventCollisionWarning = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", frameDataLoad[20], frameDataLoad[19]), 16);
                //防碰撞报警  ZT20170320
                data.PreventCollisionAlarm = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", frameDataLoad[22], frameDataLoad[21]), 16);
                //防碰撞设置方式
                data.PreventCollisionSetType = frameDataLoad[23];
                //防碰撞设置信息
                if (data.PreventCollisionSetType == 0) {
                     byte[] byteTemp = new byte[16];
                Array.Copy(frameDataLoad, 24, byteTemp, 0, 16);
                    PreventCollisionV021303.PreventCollisionSet_Zero pz = new
                      PreventCollisionV021303.PreventCollisionSet_Zero();
                    pz.TowerNo = byteTemp[0];
                    pz.TowerType = byteTemp[1];
                    pz.X = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[3], byteTemp[2]), 16);
                    pz.Y = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[5], byteTemp[4]), 16);
                    pz.BoomLength = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[7], byteTemp[6]), 16);
                    pz.BalanceLength = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[9], byteTemp[8]), 16);
                    pz.TowerBodyHeight = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[11], byteTemp[10]), 16);
                    pz.TowerCapHeight = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[13], byteTemp[12]), 16);
                    pz.MinBoomAngle = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[15], byteTemp[14]), 16);
                    data.PreventCollisionSet = pz;
                }
                else if (data.PreventCollisionSetType == 1) {
                    byte[] byteTemp = new byte[18];
                    Array.Copy(frameDataLoad, 24, byteTemp, 0, 18);
                    PreventCollisionV021303.PreventCollisionSet_One po = new
                     PreventCollisionV021303.PreventCollisionSet_One();
                    po.TowerNo = byteTemp[0];
                    po.TowerType = byteTemp[1];
                    po.BoomLength = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[3], byteTemp[2]), 16);
                    po.BalanceLength = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[5], byteTemp[4]), 16);
                    po.TowerBodyHeight = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[7], byteTemp[6]), 16);
                    po.TowerCapHeight = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[9], byteTemp[8]), 16);
                    po.DirectionTargetOppositeLocal = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[11], byteTemp[10]), 16);
                    po.Distance = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[13], byteTemp[12]), 16);
                    po.DirectionLocalOppositeTarget = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[15], byteTemp[14]), 16);
                    po.MinBoomAngle = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[17], byteTemp[16]), 16);
                    data.PreventCollisionSet = po;
                }
                else return;
                #region
                //应答
                List<byte> rb1 = new List<byte>();
                rb1.AddRange(frameHead);//头
                rb1.Add(0); rb1.Add(0);//长度
                client.SendBuffer(PackageFrameTail(rb1));
                //存入数据库
                df.deviceid = data.EquipmentID;
                df.datatype = "preventCollision";
                df.contentjson = JsonConvert.SerializeObject(data);
                #endregion
            }
            catch (Exception) { }
        }
        #endregion

        #region 区域保护设置
        //模拟接受帧：A5 5A 02 13 03 0D 00 00 31 32 33 34 35 36 37 38 01 00 02 00 03 00 01 00 01 00 01 00 01 00 01 00 01 00 01 00 01 00 02 00 02 00 02 00 02 00 02 00 02 00 02 00 02 00 F5 BF EE FF
        //应答：A5 5A 02 13 03 0d 00 00 00 00 ee ff
        private static void OnResolveLocalityProtection(byte[] frameDataLoad, byte[] frameHead, TcpSocketClient client, ref DBFrame df)
        {
            try
            {
                LocalityProtectionV021303 data = new LocalityProtectionV021303();
                //设备编号
                byte[] sn = new byte[8];   //设备编号
                Array.Copy(frameDataLoad, 0, sn, 0, 8);
                data.EquipmentID = Encoding.ASCII.GetString(sn);  //获取设备编号ASCII
                //区域保护预警
                data.LocalityProtectionWarning = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", frameDataLoad[9], frameDataLoad[8]), 16);
                //防碰撞报警
                data.LocalityProtectionAlarm = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", frameDataLoad[11], frameDataLoad[10]), 16);
                //区域保护开关信息
                data.LocalityProtectionSwitch = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", frameDataLoad[13], frameDataLoad[12]), 16);
                //防碰撞设置信息
                int count = (frameDataLoad.Length - 14) / 16;
                data.LocalityProtectionSet = new LocalityProtectionV021303.CLocalityProtectionSet[count];
                for(int i=0;i<count;i++)
                {
                    byte[] byteTemp = new byte[16];
                    Array.Copy(frameDataLoad, 14+i*16, byteTemp, 0, 16);
                    LocalityProtectionV021303.CLocalityProtectionSet lp = new
                      LocalityProtectionV021303.CLocalityProtectionSet();
                    lp.Range_One = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[1], byteTemp[0]), 16);
                    lp.Rotation_One = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[3], byteTemp[2]), 16);
                    lp.Range_Two = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[5], byteTemp[4]), 16);
                    lp.Rotation_Two = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[7], byteTemp[6]), 16);
                    lp.Range_Three = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[9], byteTemp[8]), 16);
                    lp.Rotation_Three = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[11], byteTemp[10]), 16);
                    lp.Range_Four = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[13], byteTemp[12]), 16);
                    lp.Rotation_Four = Convert.ToUInt16(string.Format("{0:X2}{1:X2}", byteTemp[15], byteTemp[14]), 16);
                    data.LocalityProtectionSet[i] = lp;
                }
                #region
                //应答
                List<byte> rb1 = new List<byte>();
                rb1.AddRange(frameHead);//头
                rb1.Add(0); rb1.Add(0);//长度
                client.SendBuffer(PackageFrameTail(rb1));
                //存入数据库
                df.deviceid = data.EquipmentID;
                df.datatype = "localityProtection";
                df.contentjson = JsonConvert.SerializeObject(data);
                #endregion
            }
            catch (Exception) { }
        }
        #endregion

        #region 公用方法
        /// <summary>
        /// 固定长度格式化数组
        /// </summary>
        /// <param name="byteTemp">数组</param>
        /// <param name="lengthTemp">长度</param>
        /// <param name="temp">需要填充得数</param>
        /// <returns></returns>
        public static List<byte> FixedLengthFormat_byte(byte[] byteTemp, int lengthTemp, byte temp)
        {
            List<byte> nameType = new List<byte>();
            nameType.AddRange(byteTemp);
            if (nameType.Count <= lengthTemp)
                for (int i = nameType.Count; i < lengthTemp; i++) nameType.Add(temp);
            else
                nameType.RemoveRange(lengthTemp, nameType.Count - lengthTemp);
            return nameType;
        }
        /// <summary>
        /// 获取数据载荷
        /// </summary>
        /// <returns></returns>
        public static byte[] GetDataLoad(byte[] b, int c)
        {
            try
            {
                //验证校验和
                if (BitConverter.ToUInt16(b, c - 4) != ConvertData.CRC16(b, 8, c - 12))//检验和
                    return null;
                byte[] result = new byte[c - 12];
                Array.Copy(b, 8, result, 0, c - 12);
                return result;
            }
            catch (Exception) { return null; }
        }
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
            byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(byteTemp, 8, rb.Count - 8));
            rb.Add(crc16[0]);
            rb.Add(crc16[1]);
            //结束符
            rb.Add(0xEE);
            rb.Add(0xFF);
            byteTemp = new byte[rb.Count];
            rb.CopyTo(byteTemp);
            return byteTemp;
        }
        /// <summary>
        /// 转换BCD时间格式
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        static byte fomaterByte(int y)
        {
            return BitConverter.GetBytes((y / 10) * 16 + (y % 10))[0];

        }
        #endregion

        #region 报警码处理，主要用于短信发送
        public static void AlarmFlag(string EquipmentIDTemp, string AlarmStr)
        {
            ////吊重 力矩 风速 碰撞   倾角
            ////重量报警
            //string WeightAlarm = AlarmStr[31].ToString();
            ////力矩报警
            //string MomentAlarm = AlarmStr[30].ToString();
            ////风速报警
            //string WindSpeedAlarm = AlarmStr[29].ToString();
            ////碰撞报警
            //string CollisionAlarm = AlarmStr.Substring(4, 4).Contains("1") ? "1" : "0";
            ////倾角报警
            //string DipAngleAlarm = AlarmStr.Substring(8, 4).Contains("1") ? "1" : "0";
            ////有报警时发送短信
            ////
            //if (WeightAlarm == "1" || MomentAlarm == "1" || WindSpeedAlarm == "1" || CollisionAlarm == "1" || DipAngleAlarm == "1")
            //{
            //    try
            //    {
            //        string[] Alarm = { WeightAlarm, MomentAlarm, WindSpeedAlarm, CollisionAlarm, DipAngleAlarm };
            //        int Times = DB_MysqlV021303.GetOnceAlarmTime(EquipmentIDTemp, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Alarm);
            //        if (Times < 60 && Times != -1)
            //            return;
            //        System.Data.DataTable dtProj = DB_MysqlV021303.GetCraneProj(EquipmentIDTemp);
            //        if (dtProj == null || dtProj.Rows.Count < 0)
            //            return;
            //        if (dtProj.Rows[0]["IsMsg"].ToString() != "1")
            //            return;
            //        string Telephone = dtProj.Rows[0]["PrincipalTel"].ToString();
            //        if (Telephone.Length != 11 && Telephone.IndexOf(',') == -1)
            //        {
            //            Telephone = "";
            //        }
            //        string content = Telephone + "@" + "工地:" + dtProj.Rows[0]["ProName"].ToString() + " 设备编号:" + EquipmentIDTemp + " 时间:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 报警类型:" + (CollisionAlarm == "1" ? "碰撞 " : "") + (WeightAlarm == "2" ? "超重 " : "") + (WindSpeedAlarm == "2" ? "风速 " : "") + (MomentAlarm == "2" ? "力矩 " : "") + (DipAngleAlarm == "2" ? "倾角 " : "");
            //        //异步发送短信
            //        //SpecialEquipmentList.InsertCacheLsMsg("13503853751" + "@" + "工地:赵通测试WEB ");
            //        SMS.SendMsg.BeginInvoke(content,null,null);
            //    }
            //    catch (Exception) { }
            //}
        }
        #endregion
    }

}
