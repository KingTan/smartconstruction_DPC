using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using TCPAPI;
using ToolAPI;
using Architecture;
using Newtonsoft.Json;
using ProtocolAnalysis.Lift;

namespace ProtocolAnalysis
{
    public static class GprsResolveDataV010206
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
            df.version = (client.External.External as TcpClientBindingExternalClass).TVersion;
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
                OnResolveRealData(b, c, client, ref df);
            }
            //身份验证
            if (typ == 0x02)
            {
                AuthenticationDispose(b, c, client);
            }
            //离线数据
            if (typ == 0x03)
            {
                OnResolveRealData(b, c, client, ref df);
                #region 应答
                //离线数据应答
                byte[] rb = new byte[12];
                //针头
                rb[0] = 0x7A;
                rb[1] = 0x7A;
                //协议版本号
                rb[2] = 0x01;
                rb[3] = 0x02;
                rb[4] = 0x06;
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
                #endregion
            }
            //参数上传
            if (typ == 0x04)
            {
                byte[] rb = OnResolveParamDataUpload(b, c, client, ref df);
                if (rb != null)
                    client.SendBuffer(rb);
            }
            if (typ == 0x05) //IP地址配置
            {
                try
                {
                    DB_MysqlLift.UpdateDataConfig(TcpExtendTemp.EquipmentID,2,true);
                    DB_MysqlLift.UpdateIPCommandIssued(TcpExtendTemp.EquipmentID, 2);
                }
                catch (Exception ex) {
                    ToolAPI.XMLOperation.WriteLogXmlNoTail("GprsResolveDataV010206IP写入数据库标识异常", ex.Message);
                }
            }
            if (typ == 0x06) //限位控制信息
            {

                try
                {
                    DB_MysqlLift.UpdatecontrolDataCongfig(TcpExtendTemp.EquipmentID);
                }
                catch (Exception ex) {
                    ToolAPI.XMLOperation.WriteLogXmlNoTail("GprsResolveDataV010206限位控制写入数据库标识异常", ex.Message);
                }
            }
            if (typ == 0x07) // 设备运行时间
            {
                byte[] rb = OnResolveRunTime(b, c, ref df);
                if (rb != null)
                    client.SendBuffer(rb);
            }
            if (typ == 0x08) //时间校准
            {
                byte[] rb = OnResolveTime(b, c);
                if (rb != null)
                    client.SendBuffer(rb);
            }
            if (typ == 0x09) //掉电通知
            {
                byte[] rb = new byte[12];
                //针头
                rb[0] = 0x7A;
                rb[1] = 0x7A;
                //协议版本号
                rb[2] = 0x01;
                rb[3] = 0x02;
                rb[4] = 0x06;
                //命令字
                rb[5] = 0x09;
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

            
            if (TcpExtendTemp.EquipmentID == null || TcpExtendTemp.EquipmentID.Equals(""))
            {
                TcpExtendTemp.EquipmentID = df.deviceid;
            }
            //存入数据库
            if (df.contentjson != null && df.contentjson != "")
            {
                DB_MysqlLift.SaveLift(df);
            }
            return "";
        }
        #region 设备运行时间
        /// <summary>
        /// 设备运行时间
        /// </summary>
        /// <param name="b"></param>
        /// <param name="bCount"></param>
        /// <returns></returns>
        /// 7A 7A 01 02 06 07 1C 00 31 32 33 34 35 36 37 38 0a 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 0E D9 7B 7B
        /// 7a 7a 01 02 06 07 00 00 00 00 00 7b 7b
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
            rb[3] = 0x02;
            rb[4] = 0x06;
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
        #region 身份验证
        //身份验证解析入口
        //人脸特征码上传
        //7a 7a 01 02 06 02 12 00 30 31 32 33 34 35 36 37 06 02 01 01 34 31 31 34 30 32 31 39 39 32 31 32 30 37 35 35 31 38 92 bb 7b 7b
        //接收 7a 7a 01 02 06 02 0d 00 30 31 32 33 34 35 36 37 06 02 01 01 01 de 6f 7b 7b
        //7a 7a 01 02 06 02 51 00 30 31 32 33 34 35 36 37 06 00 01 31 30 30 30 31 33 37 37 31 30 30 30 31 33 37 37 34 31 31 34 30 32 31 39 39 32 31 32 30 37 35 35 31 38 31 35 38 30 31 36 37 33 32 35 34 d5 d4 cd a8 ff ff ff ff ff ff ff ff ff ff ff ff 20 17 09 25 01 ff ff ff ff b6 ff 7b 7b

        //人员正常考勤
        //发送7a 7a 01 02 06 02 00 00 30 31 32 33 34 35 36 37 06 01 31 30 30 30 31 33 37 37 01 6F 35 7b 7b
        //7a 7a 01 02 06 02 00 00 30 31 32 33 34 35 36 37 06 01 31 30 30 30 31 33 37 37 01 6f 35 7b 7b 
        //7a 7a 01 02 06 02 00 00 30 31 32 33 34 35 36 37 06 01 31 30 30 30 31 33 37 37 01 6f 35 7b 7b
        private static void AuthenticationDispose(byte[] b, int c, TcpSocketClient client)
        {
            try
            {
                string root = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string path = root.Remove(root.LastIndexOf('\\') + 1) + "Config.ini";
                bool IsAuthentication = bool.Parse(FileHelp.IniReadValue("goyo", "IsAuthentication", path));

                Lift_Authentication AuthenticationTemp = AuthenticationAnalysis(b, c);
                if (IsAuthentication)
                {
                    #region 需要验证

                    if (b[17] == 0x05)//司机卡删后设备应答帧   
                    {
                        if (b[16] != 6)
                        {
                            PlatformAuthentication(AuthenticationTemp, 3);
                        }
                    }
                    else if (b[17] == 0x00)//身份认证
                    {
                        byte result = 0;
                        if (b[16] == 6 || b[16] == 4)
                        {
                            result = PlatformAuthentication(AuthenticationTemp, 5);
                        }
                        else
                        {
                            result = PlatformAuthentication(AuthenticationTemp, 2);
                        }
                        byte[] rb = OnResolveAuthentication1(b, 0, result, AuthenticationTemp);
                        if (rb != null)
                            client.SendBuffer(rb);
                    }
                    else if (b[17] == 0x01)//上班或下班
                    {
                        client.SendBuffer(b);         //先原包返回
                        //数据库比对是否存在此司机卡号
                        if (PlatformAuthentication(AuthenticationTemp, 1) == 1)
                        {
                            //打卡上下班
                            byte[] rb;
                            if (AuthenticationTemp.Status == 2)//下班
                            {
                                AuthenticationTemp.Status = 0;
                                byte result = PlatformAuthentication(AuthenticationTemp, 4);
                                rb = OnResolveAuthentication(b, 2, result);
                            }
                            else
                            {
                                byte result = PlatformAuthentication(AuthenticationTemp, 4);
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
                    //特征码上传 在飞瑞斯人脸识别时上传上传特征码进行身份验证
                    else if (b[17] == 0x02)
                    {
                        //进行应答
                        if (AuthenticationTemp == null)
                        {
                            //应答为失败
                            byte[] rb = OnResolveAuthentication1(b, 2, 1, AuthenticationTemp);
                            if (rb != null)
                                client.SendBuffer(rb);
                            //下发司机信息
                            rb = OnResolveAuthentication1(b, 2, 3, AuthenticationTemp);
                            if (rb != null)
                                client.SendBuffer(rb);
                        }
                        else
                        {
                            //身份认证记录添加到对应的数据库里面 20170217
                            AuthenticationTemp.Status = 0;
                            PlatformAuthentication(AuthenticationTemp, 4);
                            //应答成功
                            byte[] rb = OnResolveAuthentication1(b, 2, 1, AuthenticationTemp);
                            if (rb != null)
                                client.SendBuffer(rb);
                            //下发司机信息
                            rb = OnResolveAuthentication1(b, 2, 2, AuthenticationTemp);
                            if (rb != null)
                                client.SendBuffer(rb);
                        }
                    }
                    else
                    {
                        return;
                    }
                    #endregion
                }
                else
                {
                    #region 不需要验证
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

                    #endregion
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception)
            { }
        }
        /// <summary>
        /// 平台数据库进行处理
        /// 命令 1是否存在司机卡号 2身份验证  3 删除卡号 4上下班
        /// </summary>
        /// <param name="AuthenticationTemp">身份验证对象</param>
        /// <returns></returns>
        private static byte PlatformAuthentication(Lift_Authentication AuthenticationTemp, int Commod)
        {
            #region 对应数据库操作
            switch (Commod)
            {
                case 1: return DB_MysqlLift.IsExistCard(AuthenticationTemp);//是否存在司机卡号
                case 2: return DB_MysqlLift.IsExistCard(AuthenticationTemp);//身份验证
                case 3: DB_MysqlLift.UpdateIdentifyCurrent(AuthenticationTemp.SN, AuthenticationTemp.KardID); break;//删除卡号
                case 4: return (byte)DB_MysqlLift.Pro_Authentication(AuthenticationTemp);//上班下班
                case 5: return DB_MysqlLift.IsExistEmpNo(AuthenticationTemp.SN,AuthenticationTemp.empNo);//身份验证 验证工号
            }
            #endregion
            return 0;
        }
        private static byte[] OnResolveAuthentication(byte[] b, int type, byte result)
        {
            byte[] rb = null;

            if (type == 3)//删除卡号下发
            {
                rb = new byte[30];
                for (int i = 0; i < 26; i++)
                    rb[i] = b[i];
                //帧长度
                rb[6] = 0x12;
                rb[17] = 0x05;   //子命令字5
                //校验和;
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(rb, 8, 18));
                rb[26] = crc16[0];
                rb[27] = crc16[1];
                //帧尾
                rb[28] = 0x7B;
                rb[29] = 0x7B;

            }

            if (type == 1)//上班
            {
                rb = b;
            }
            if (type == 2)//下班
            {
                rb = b;
            }
            //数据原路返回
            return rb;


        }
        //
        private static byte[] OnResolveAuthentication1(byte[] b, int type, byte result, Lift_Authentication model)
        {
            byte[] rb = null;
            if (type == 0)//身份验证
            {
                if (result == 0x00)
                {
                    rb = new byte[23];
                    for (int i = 0; i < 18; i++)
                        rb[i] = b[i];
                    //帧长度
                    rb[6] = 0x0B;
                    rb[18] = 0x00;
                    //校验和;
                    byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(rb, 8, 11));
                    rb[19] = crc16[0];
                    rb[20] = crc16[1];
                    //帧尾
                    rb[21] = 0x7B;
                    rb[22] = 0x7B;
                }
                else if (result == 0x01)
                {
                    #region  司机信息
                    rb = new byte[93];
                    for (int i = 0; i < 18; i++)
                        rb[i] = b[i];

                    //帧长度
                    rb[6] = 0x51;
                    rb[18] = 0x01;
                    if (model.empNo.Length == 8)   //工号解析如果没有工号就赋值司机卡号
                    {
                        byte[] byteArray = System.Text.Encoding.Default.GetBytes(model.empNo);
                        for (int i = 0, j = 19; i < byteArray.Length; i++, j++)
                        {
                            rb[j] = byteArray[i];
                        }
                    }
                    else
                    {
                        for (int l = 19; l < 27; l++)   //司机卡号
                        {
                            rb[l] = b[l];
                        }
                    }
                    for (int l = 27, j = 18; l < 35; l++, j++)   //司机卡号
                    {
                        rb[l] = b[j];
                    }

                    if (model.code.Length == 18)      //如果存在身份证加入身份证，否则填充0xff
                    {
                        byte[] byteArray = System.Text.Encoding.Default.GetBytes(model.code);
                        for (int i = 0, j = 35; i < byteArray.Length; i++, j++)
                        {
                            rb[j] = byteArray[i];
                        }
                    }
                    else
                    {
                        for (int i = 35; i < 53; i++)
                        {
                            rb[i] = 0xff;
                        }
                    }
                    if (model.telephone.Length == 11)    //如果存在手机号，否则填充0xff
                    {
                        byte[] byteArray = System.Text.Encoding.Default.GetBytes(model.telephone);
                        for (int i = 0, j = 53; i < byteArray.Length; i++, j++)
                        {
                            rb[j] = byteArray[i];
                        }
                    }
                    else
                    {
                        for (int i = 53; i < 64; i++)
                        {
                            rb[i] = 0xff;
                        }
                    }
                    if (!string.IsNullOrEmpty(model.name))     //姓名
                    {
                        byte[] pp = Encoding.GetEncoding("GBK").GetBytes(model.name);
                        for (int i = 0, j = 64; i < 16; i++, j++)
                        {
                            if (pp.Length > i)
                                rb[j] = pp[i];
                            else
                                rb[j] = 0xff;
                        }
                    }
                    else
                    {
                        for (int j = 64; j < 80; j++)
                        {
                            rb[j] = 0xff;
                        }
                    }
                    DateTime date = System.DateTime.Now;                  //有效期
                    int year = date.Year - 2000;
                    int month = date.Month;
                    int day = date.Day;
                    rb[80] = 0x20;
                    rb[81] = fomaterByte(year);
                    rb[82] = fomaterByte(month);
                    rb[83] = fomaterByte(day);
                    if (!string.IsNullOrEmpty(model.job))               //职位
                    {
                        if (model.job.Contains("塔吊司机卡"))
                            rb[84] = 0x00;
                        if (model.job.Contains("升降机司机"))
                            rb[84] = 0x01;
                        if (model.job.Contains("监理卡"))
                            rb[84] = 0x02;
                        if (model.job.Contains("施工卡"))
                            rb[84] = 0x03;
                    }

                    rb[85] = 0xff;
                    rb[86] = 0xff;
                    rb[87] = 0xff;
                    rb[88] = 0xff;
                    #endregion
                    //校验和;
                    byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(rb, 8, 81));
                    rb[89] = crc16[0];
                    rb[90] = crc16[1];
                    //帧尾
                    rb[91] = 0x7B;
                    rb[92] = 0x7B;
                }
            }
            else if (type == 1)//上下班
            {
                rb = b;
            }
            else if (type == 2)//特征码上传后的应答 当接收到的是特征码上传协议时使用
            {
                if (result == 0 || result == 1)  //就是正常的特征码子命令应答
                {
                    rb = new byte[25];
                    for (int i = 0; i < 18; i++)
                        rb[i] = b[i];
                    //帧长度 13
                    rb[6] = 0x0D;
                    rb[18] = 0x01;
                    rb[19] = 0x01;
                    rb[20] = result;
                    //校验和;
                    byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(rb, 8, 13));
                    rb[21] = crc16[0];
                    rb[22] = crc16[1];
                    //帧尾
                    rb[23] = 0x7B;
                    rb[24] = 0x7B;
                }
                else if (result == 2)//拼接下发司机的信息 成功附带的主动下发，用的是身份验证帧的应答
                {
                    #region  司机信息
                    rb = new byte[93];
                    for (int i = 0; i < 18; i++)
                        rb[i] = b[i];

                    //帧长度
                    rb[6] = 0x51;//数据载荷的长度
                    //身份验证帧
                    rb[17] = 0x00;
                    //数据区
                    rb[18] = 0x01;
                    try
                    {
                        if (model.empNo.Length == 8)   //工号解析如果没有工号就赋值司机卡号
                        {
                            byte[] byteArray = System.Text.Encoding.Default.GetBytes(model.empNo);
                            for (int i = 0, j = 19; i < byteArray.Length; i++, j++)
                            {
                                rb[j] = byteArray[i];
                            }
                        }
                        else
                        {
                            byte[] byteArray = System.Text.Encoding.Default.GetBytes(model.cardNo);
                            for (int i = 0, j = 19; i < byteArray.Length; i++, j++)
                            {
                                rb[j] = byteArray[i];
                            }
                        }
                    }
                    catch (Exception)
                    {
                        for (int j = 19; j < 27; j++)
                        {
                            rb[j] = 0xff;
                        }
                    }
                    try
                    {
                        byte[] byteArrayTemp = System.Text.Encoding.Default.GetBytes(model.cardNo);
                        for (int l = 27, j = 0; l < 35; l++, j++)   //司机卡号
                        {
                            rb[l] = byteArrayTemp[j];
                        }
                    }
                    catch (Exception)
                    {
                        for (int l = 27; l < 35; l++)   //司机卡号
                        {
                            rb[l] = 0xff;
                        }
                    }

                    if (model.code.Length == 18)      //如果存在身份证加入身份证，否则填充0xff
                    {
                        byte[] byteArray = System.Text.Encoding.Default.GetBytes(model.code);
                        for (int i = 0, j = 35; i < byteArray.Length; i++, j++)
                        {
                            rb[j] = byteArray[i];
                        }
                    }
                    else
                    {
                        for (int i = 35; i < 53; i++)
                        {
                            rb[i] = 0xff;
                        }
                    }
                    if (model.telephone.Length == 11)    //如果存在手机号，否则填充0xff
                    {
                        byte[] byteArray = System.Text.Encoding.Default.GetBytes(model.telephone);
                        for (int i = 0, j = 53; i < byteArray.Length; i++, j++)
                        {
                            rb[j] = byteArray[i];
                        }
                    }
                    else
                    {
                        for (int i = 53; i < 64; i++)
                        {
                            rb[i] = 0xff;
                        }
                    }
                    if (!string.IsNullOrEmpty(model.name))     //姓名
                    {
                        byte[] pp = Encoding.GetEncoding("GBK").GetBytes(model.name);
                        for (int i = 0, j = 64; i < 16; i++, j++)
                        {
                            if (pp.Length > i)
                                rb[j] = pp[i];
                            else
                                rb[j] = 0xff;
                        }
                    }
                    else
                    {
                        for (int j = 64; j < 80; j++)
                        {
                            rb[j] = 0xff;
                        }
                    }
                    DateTime date = System.DateTime.Now;                  //有效期
                    int year = date.Year - 2000;
                    int month = date.Month;
                    int day = date.Day;
                    rb[80] = 0x20;
                    rb[81] = fomaterByte(year);
                    rb[82] = fomaterByte(month);
                    rb[83] = fomaterByte(day);
                    if (!string.IsNullOrEmpty(model.job))               //职位
                    {
                        if (model.job.Contains("塔吊司机卡"))
                            rb[84] = 0x00;
                        if (model.job.Contains("升降机司机"))
                            rb[84] = 0x01;
                        if (model.job.Contains("监理卡"))
                            rb[84] = 0x02;
                        if (model.job.Contains("施工卡"))
                            rb[84] = 0x03;
                    }

                    rb[85] = 0xff;
                    rb[86] = 0xff;
                    rb[87] = 0xff;
                    rb[88] = 0xff;
                    #endregion
                    //校验和;
                    byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(rb, 8, 81));
                    rb[89] = crc16[0];
                    rb[90] = crc16[1];
                    //帧尾
                    rb[91] = 0x7B;
                    rb[92] = 0x7B;
                }
                else
                {
                    rb = new byte[23];
                    for (int i = 0; i < 18; i++)
                        rb[i] = b[i];
                    //帧长度
                    rb[6] = 0x0B;
                    //身份验证帧
                    rb[17] = 0x00;
                    rb[18] = 0x00;
                    //校验和;
                    byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(rb, 8, 11));
                    rb[19] = crc16[0];
                    rb[20] = crc16[1];
                    //帧尾
                    rb[21] = 0x7B;
                    rb[22] = 0x7B;
                }
            }
            else if (type == 3)//删除卡号下发
            {
                rb = new byte[30];
                for (int i = 0; i < 26; i++)
                    rb[i] = b[i];
                //帧长度
                rb[6] = 0x12;
                //校验和;
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(rb, 8, 18));
                rb[26] = crc16[1];
                rb[27] = crc16[0];
                //帧尾
                rb[28] = 0x7B;
                rb[29] = 0x7B;
            }
            else if (type == 5)//删除司机信息
            {
                rb = new byte[23];
                for (int i = 0; i < 18; i++)
                    rb[i] = b[i];
                //帧长度
                rb[6] = 0x0B;
                rb[18] = 0x00;
                //校验和;
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(rb, 8, 11));
                rb[19] = crc16[1];
                rb[20] = crc16[0];
                //帧尾
                rb[21] = 0x7B;
                rb[22] = 0x7B;

            }
            else if (type == 6)//监理授权人员信息
            {
                rb = new byte[23];
                for (int i = 0; i < 18; i++)
                    rb[i] = b[i];
                //帧长度
                rb[6] = 0x0B;
                rb[18] = 0x00;
                //校验和;
                byte[] crc16 = BitConverter.GetBytes(ConvertData.CRC16(rb, 8, 11));
                rb[19] = crc16[1];
                rb[20] = crc16[0];
                //帧尾
                rb[21] = 0x7B;
                rb[22] = 0x7B;
            }
           
           
            //数据原路返回
            return rb;
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
        /// <summary>
        /// 协议解析
        /// </summary>
        /// <returns></returns>
        private static Lift_Authentication AuthenticationAnalysis(byte[] b, int bCount)
        {
            try
            {
                #region 解析
                string tStr = ConvertData.ToHexString(b, 0, 2);
                if (tStr != "7A7A")
                    return null;
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
                    if (b[16] == 6 || b[16] == 4)//人脸识别
                    {
                        au.isFace = true;
                        sn = new byte[8];   //司机卡号
                        for (int l = 18, j = 0; l < 26; l++, j++)
                        {
                            sn[j] = b[l];
                        }
                        //司机卡号ASCII
                        au.empNo = Encoding.ASCII.GetString(sn);
                        DataTable dt = DB_MysqlLift.GetDriverInfoByEmpNo(au.SN,au.empNo);  //获取司机相关信息
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
                        DataTable dt = DB_MysqlLift.GetIdentifyInfo(au.SN,au.KardID);  //获取司机相关信息
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
                        DataTable dt = DB_MysqlLift.GetDriverInfoByIDCard(au.SN,au.code);  //获取司机相关信息
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
        #region 心跳
        //7a 7a 01 02 06 00 00 00 4C 41 30 30 30 30 31 31 17 03 14 16 47 12 A5 87 7b 7b
        //7a 7a 01 02 06 00 00 00 58 58 4C 41 30 30 31 31 17 03 14 16 47 12 6D 15 7b 7b
        /// <summary>
        /// 心跳接收数据
        /// </summary>
        /// <param name="b"></param>
        public static byte[] OnResolveHeabert(byte[] b, int bCount,TcpSocketClient client,ref DBFrame df)//1.2.1
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
            byte[] bytes = new byte[19];
            DateTime getdate = new DateTime();
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
                bytes[3] = 0x02;
                bytes[4] = 0x06;
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

                ToolAPI.XMLOperation.WriteLogXmlNoTail("时间校验V1.2.6", "包内解析时间：" + getdate + "当前时间：" + now);
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
            bytes[3] = 0x02;
            bytes[4] = 0x06;
            //命令字
            bytes[5] = 0x00;
            //包装
            df.deviceid = data.SN;
            df.datatype = "heartbeat";
            df.contentjson = JsonConvert.SerializeObject(data);

            string sourId = data.SN;
            //数据库的拷贝
            if (!string.IsNullOrEmpty(MainStatic.DeviceCopy_Lift))
            {
                if (MainStatic.DeviceCopy_Lift.Contains(sourId + "#"))
                {
                    try
                    {
                        string[] strary = MainStatic.DeviceCopy_Lift.Split(';');
                        foreach (string dev in strary)
                        {
                            if (dev.Contains(sourId + "#"))
                            {
                                string[] devcopy = dev.Split('#');
                                data.SN = devcopy[1];
                                DBFrame dfcopy = DBFrame.DeepCopy(df);
                                dfcopy.deviceid = devcopy[1];
                                dfcopy.datatype = "heartbeat";
                                dfcopy.contentjson = JsonConvert.SerializeObject(data);
                                if (dfcopy.contentjson != null && dfcopy.contentjson != "")
                                {
                                    DB_MysqlLift.SaveLift(dfcopy);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                }
            }

            return bytes;
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
            DateTime getdate = DateTime.ParseExact(tStr, "yyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
            DateTime now = System.DateTime.Now;
            double compare = (now - getdate).TotalMinutes;
            byte[] bytes = new byte[19];
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
            bytes[3] = 0x02;
            bytes[4] = 0x06;
            //命令字
            bytes[5] = 0x08;
            return bytes;

        }
        //数据响应
        public static List<byte> OnResolveTimeACK(byte[] b, List<byte> lb)
        {
            return null;
        }
        #endregion
        #region 实时数据
        /// <summary>
        /// 实时数据
        /// </summary>
        /// <param name="b"></param>
        /// 7A7A0102060132004C413030303231343300000000000000000000001710270800000000B80BAE00000004000000400003C11800000000000000D0DD7B7B
        /// 7A 7A 01 02 06 01 32 00 4C 41 30 30 30 32 31 34 33 00 00 00 00 00 00 00 00 00 00 00 17 10 27 08 00 00 00 00 B8 0B AE 00 00 00 04 00 00 00 40 00 03 C1 18 00 00 00 00 00 00 00 D0 DD 7B 7B 
        public static Byte[] OnResolveRealData(byte[] b, int bCount, TcpSocketClient client, ref DBFrame df)
        {
            //if (bCount != 62)//55
            //    return null;
            //if (BitConverter.ToUInt16(b, 51) != ConvertData.CRC16(b, 8, 50))//检验和
            //    return null;
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
            //卡号
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
            data.Weight = (result / 1000.00).ToString("0.000");//设备发送单位：吨  ZT20170329 更改保留后三位
            data.Weight_NoUnitConversions = result.ToString();//ZT20170316 更改，为珠海设计的。
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
            //警告码
            s.bValue1 = b[46];
            s.bValue2 = b[47];
            //2字节： 0-重量预警 1-重量报警 2-顶层预警 3-顶层报警 4-蹲底 5-门打开 6-风速预警 7-风速报警 9-人数报警 10-防坠器报警
            string alarm = Convert.ToString(s.sValue, 2).PadLeft(16, '0');
            data.AlarmType = "00000000000000"+alarm.Substring(alarm.Length-2,2); 

            //把预警全部置零
            char[] cr = data.AlarmType.ToCharArray();
            cr[15] = '0';
            cr[13] = '0';
            cr[9] = '0';
            string alarmType = new string(cr);
            data.AlarmType= alarmType;
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
            ///显示屏当前页面
            data.CurScreen = b[50].ToString();
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
            tStr = ConvertData.ToHexString(b, 60, 2);
            if (tStr != "7B7B")
                return null;
            //包装
            df.deviceid = data.SN;
            df.datatype = "current";
            df.contentjson = JsonConvert.SerializeObject(data);

            string sourId = data.SN;
            //数据库的拷贝
            if (!string.IsNullOrEmpty(MainStatic.DeviceCopy_Lift))
            {
                if (MainStatic.DeviceCopy_Lift.Contains(sourId + "#"))
                {
                    try
                    {
                        string[] strary = MainStatic.DeviceCopy_Lift.Split(';');
                        foreach (string dev in strary)
                        {
                            if (dev.Contains(sourId + "#"))
                            {
                                string[] devcopy = dev.Split('#');
                                data.SN = devcopy[1];
                                DBFrame dfcopy = DBFrame.DeepCopy(df);
                                dfcopy.deviceid = devcopy[1];
                                dfcopy.datatype = "current";
                                dfcopy.contentjson = JsonConvert.SerializeObject(data);
                                if (dfcopy.contentjson != null && dfcopy.contentjson != "")
                                {
                                    DB_MysqlLift.SaveLift(dfcopy);
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
        #endregion
        #region 参数上传
        /// <summary>
        /// 参数上传
        /// </summary>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// 7A 7A 01 02 06 04 68 00 31 32 33 34 35 36 37 38 17 10 27 09 49 00 01 00 06 17 10 27 01 00 02 00 03 00 04 00 05 00 06 00 07 00 08 00 01 02 03 04 05 06 07 08 09 00 01 00 02 00 03 00 04 00 01 00 00 00 02 00 00 00 01 00 02 00 03 00 04 00 01 02 03 04 05 01 00 01 00 00 00 02 00 00 00 08 09 00 00 00 01 0a 00 00 00 01 01 02 01 02 06 31 32 36 69 BB 7b 7b
        /// 7a 7a 01 02 06 04 00 00 00 00 00 7b 7b
        private static byte[] OnResolveParamDataUpload(byte[] b, int bCount, TcpSocketClient client, ref DBFrame df)
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
                data.InstalTime = DateTime.ParseExact(tStr, "yyMMdd", System.Globalization.CultureInfo.InvariantCulture).ToString("yyyy-MM-dd HH:mm:ss");
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

            string sourId = data.SN;
            //数据库的拷贝
            if (!string.IsNullOrEmpty(MainStatic.DeviceCopy_Lift))
            {
                if (MainStatic.DeviceCopy_Lift.Contains(sourId + "#"))
                {
                    try
                    {
                        string[] strary = MainStatic.DeviceCopy_Lift.Split(';');
                        foreach (string dev in strary)
                        {
                            if (dev.Contains(sourId + "#"))
                            {
                                string[] devcopy = dev.Split('#');
                                data.SN = devcopy[1];
                                DBFrame dfcopy = DBFrame.DeepCopy(df);
                                dfcopy.deviceid = devcopy[1];
                                dfcopy.datatype = "parameterUpload";
                                dfcopy.contentjson = JsonConvert.SerializeObject(data);
                                if (dfcopy.contentjson != null && dfcopy.contentjson != "")
                                {
                                    DB_MysqlLift.SaveLift(dfcopy);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }

                }
            }
       

            //应答数据
            byte[] rb = new byte[13];
            //针头
            rb[0] = 0x7A;
            rb[1] = 0x7A;
            //协议版本号
            rb[2] = 0x01;
            rb[3] = 0x02;
            rb[4] = 0x06;
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
    }
}
