using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Architecture;
using SIXH.DBUtility;
using TCPAPI;
using ToolAPI;

namespace ProtocolAnalysis.FogGun
{
    public class CommandIssued_FogGun
    {
        static Dictionary<string, Frame_TimingConfig> Frame_TimingConfigSet = new Dictionary<string, Frame_TimingConfig>();
        static int INT_Constant = 10000;
        static IList<TcpSocketClient> SocketListFlag = null;
        static bool isTimeOpen = false;
        private static Thread FoggunSettingTimeWork = null;//开启一个线程轮询，每天00：00雾泡的操作标志重置
        #region 向客户端下发请求
        /// <summary>
        /// 遍历数据库获取命令并向客户端请求
        /// </summary>
        public static void FogGun_SetCommond(IList<TcpSocketClient> SocketList)
        {
            SocketListFlag = SocketList;
            if (!isTimeOpen)
            {
                isTimeOpen = true;
                //初始化定时配置集
                GetTimingConfigInit();
                //闹钟实现
                System.Timers.Timer t = new System.Timers.Timer(INT_Constant);//实例化Timer类，设置间隔时间为10000毫秒；
                t.Elapsed += new System.Timers.ElapsedEventHandler(alarmClockProcess);//到达时间的时候执行事件；
                t.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；
                t.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
            }
           // GetTimingConfig(SocketList);//定时配置先关闭
            GetManualControl(SocketList);
            GetUpdateIpPort(SocketList);
            GetFoggunSettingTimeWork(SocketList);//定时雾泡喷淋
            //开启线程 每天的0点更新雾泡喷淋的操作标识
            if (FoggunSettingTimeWork == null)
            {
                FoggunSettingTimeWork = new Thread(FoggunSettingTimeWorkReset) { IsBackground = true, Priority = ThreadPriority.Highest };
                FoggunSettingTimeWork.Start();
            }

        }
        #region 获取定时配置(先关闭)
        //获取定时配置
        static void GetTimingConfig(IList<TcpSocketClient> SocketList)
        {
            DataTable dt = DB_MysqlFogGun.GetTimingConfig();
            if (dt != null)
            {
                int iRows = dt.Rows.Count;
                if (iRows > 0)
                {
                    for (int i = 0; i < iRows; i++)
                    {
                        for (int j = 0; j < SocketList.Count; j++)
                        {
                            string DeviceNo = (SocketList[j].External.External as TcpClientBindingExternalClass).EquipmentID;
                            string DeviceNoServer = dt.Rows[i]["equipmentNo"].ToString();
                            if (DeviceNo != null && DeviceNo.Equals(DeviceNoServer))
                            {
                                Frame_TimingConfig TimingConfig = new Frame_TimingConfig();
                                TimingConfig.DeviceNo = DeviceNoServer;
                                TimingConfig.TimingSwitch = dt.Rows[i]["openState"].ToString();
                                TimingConfig.Time = dt.Rows[i]["timingstarttime"].ToString();
                                TimingConfig.Timeout = dt.Rows[i]["timeout"].ToString();
                                TimingConfig.Week = weekProcess(dt.Rows[i]["weeks"].ToString());//星期的初始化
                                if (Frame_TimingConfigSet.ContainsKey(TimingConfig.DeviceNo))
                                {
                                    Frame_TimingConfigSet[TimingConfig.DeviceNo] = TimingConfig;
                                }
                                else
                                {
                                    Frame_TimingConfigSet.Add(TimingConfig.DeviceNo, TimingConfig);
                                }
                                if ((SocketList[j].External.External as TcpClientBindingExternalClass).TVersion == "010002")
                                {
                                    byte[] message = GprsResolveDataV010002.SendJoint_TimingConfig(TimingConfig);
                                    if (message != null)
                                    { SocketList[j].SendBuffer(message); DB_MysqlFogGun.SaveTimingConfig(TimingConfig, 1); }
                                }
                                else if ((SocketList[j].External.External as TcpClientBindingExternalClass).TVersion == "010003")
                                {
                                    byte[] message = GprsResolveDataV010003.SendJoint_TimingConfig(TimingConfig);
                                    if (message != null)
                                    { SocketList[j].SendBuffer(message); DB_MysqlFogGun.SaveTimingConfig(TimingConfig, 1); }
                                }
                                else if ((SocketList[j].External.External as TcpClientBindingExternalClass).TVersion == "020000")
                                {
                                    byte[] message = GprsResolveDataV020000.SendJoint_TimingConfig(TimingConfig);
                                    if (message != null)
                                    { SocketList[j].SendBuffer(message); DB_MysqlFogGun.SaveTimingConfig(TimingConfig, 1); }
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion
        //手动控制
        static void GetManualControl(IList<TcpSocketClient> SocketList)
        {
            DataTable dt = DB_MysqlFogGun.GetManualControl();
            if (dt != null)
            {
                int iRows = dt.Rows.Count;
                if (iRows > 0)
                {
                    for (int i = 0; i < iRows; i++)
                    {
                        for (int j = 0; j < SocketList.Count; j++)
                        {
                            string DeviceNo = (SocketList[j].External.External as TcpClientBindingExternalClass).EquipmentID;
                            string DeviceNoServer = dt.Rows[i]["equipmentNo"].ToString();
                            if (DeviceNo != null && DeviceNo.Equals(DeviceNoServer))
                            {
                                Frame_ManualControl ManualControl = new Frame_ManualControl();
                                ManualControl.DeviceNo = DeviceNoServer;
                                ManualControl.DeviceState = dt.Rows[i]["open_state"].ToString();
                                if ((SocketList[j].External.External as TcpClientBindingExternalClass).TVersion == "010002")
                                {
                                    byte[] message = GprsResolveDataV010002.SendJoint_ManualControl(ManualControl);
                                    if (message != null)
                                    { SocketList[j].SendBuffer(message); DB_MysqlFogGun.SaveManualControl(ManualControl, 1); }
                                }
                                else if ((SocketList[j].External.External as TcpClientBindingExternalClass).TVersion == "010003")
                                {
                                    byte[] message = GprsResolveDataV010003.SendJoint_ManualControl(ManualControl);
                                    if (message != null)
                                    { SocketList[j].SendBuffer(message); DB_MysqlFogGun.SaveManualControl(ManualControl, 1); }
                                }
                                else if ((SocketList[j].External.External as TcpClientBindingExternalClass).TVersion == "020000")
                                {
                                    byte[] message = GprsResolveDataV020000.SendJoint_ManualControl(ManualControl);
                                    if (message != null)
                                    { SocketList[j].SendBuffer(message); DB_MysqlFogGun.SaveManualControl(ManualControl, 1); }
                                }
                            }
                        }
                    }
                }
            }
        }
        //更改IP
        static void GetUpdateIpPort(IList<TcpSocketClient> SocketList)
        {

            DataTable dt = DB_MysqlFogGun.GetUpdateIpPort();
            if (dt != null)
            {
                int iRows = dt.Rows.Count;
                if (iRows > 0)
                {
                    for (int i = 0; i < iRows; i++)
                    {
                        for (int j = 0; j < SocketList.Count; j++)
                        {
                            string DeviceNo = (SocketList[j].External.External as TcpClientBindingExternalClass).EquipmentID;
                            string DeviceNoServer = dt.Rows[i]["equipmentNo"].ToString();
                            if (DeviceNo != null && DeviceNo.Equals(DeviceNoServer))
                            {
                                Frame_UpdateIpPort UpdateIpPort = new Frame_UpdateIpPort();
                                UpdateIpPort.DeviceNo = DeviceNoServer;
                                UpdateIpPort.IP = dt.Rows[i]["ip_dn"].ToString();
                                UpdateIpPort.Port = dt.Rows[i]["port"].ToString();
                                UpdateIpPort.State = 1;
                                UpdateIpPort.IPLength = (byte)UpdateIpPort.IP.Length;
                                UpdateIpPort.PortLength = (byte)UpdateIpPort.Port.Length;
                                UpdateIpPort.issuccess = false;
                                if ((SocketList[j].External.External as TcpClientBindingExternalClass).TVersion == "020000")
                                {
                                    byte[] message = GprsResolveDataV020000.SendJoint_UpdateIpPort(UpdateIpPort);
                                    if (message != null)
                                    {
                                        SocketList[j].SendBuffer(message);
                                        DB_MysqlFogGun.SaveUpdateIpPort(UpdateIpPort);
                                        DB_MysqlFogGun.RecordCommandIssued(DeviceNoServer, 1);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion

        static string weekProcess(string weekTemp)
        {
            weekTemp = weekTemp.Replace("1", "星期一");
            weekTemp = weekTemp.Replace("2", "星期二");
            weekTemp = weekTemp.Replace("3", "星期三");
            weekTemp = weekTemp.Replace("4", "星期四");
            weekTemp = weekTemp.Replace("5", "星期五");
            weekTemp = weekTemp.Replace("6", "星期六");
            weekTemp = weekTemp.Replace("7", "星期日");
            return weekTemp;
        }
        /// <summary>
        /// 闹钟
        /// </summary>
        static void alarmClockProcess(object source, System.Timers.ElapsedEventArgs e)
        {
            GetTimingConfigInit();
            try
            {
                string[] weekdays = { "星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六" };
                string nowweek = weekdays[Convert.ToInt32(DateTime.Now.DayOfWeek)];
                string nowTime = DateTime.Now.ToString("HH:mm");
                for (int j = 0; j < SocketListFlag.Count; j++)
                {
                    string DeviceNo = (SocketListFlag[j].External.External as TcpClientBindingExternalClass).EquipmentID;
                    if (Frame_TimingConfigSet.ContainsKey(DeviceNo))
                    {
                        Frame_TimingConfig TimedConfig = Frame_TimingConfigSet[DeviceNo];
                        //进行判断是否有条件进行定时下发
                        //时间正确，且openState定时功能配置开关是打开状态
                        DateTime dtemp = DateTime.Parse(TimedConfig.Time);
                        string ttemp = dtemp.ToString("HH:mm");
                        if (TimedConfig.Week.Contains(nowweek) && ttemp.Equals(nowTime) && TimedConfig.TimingSwitch.Equals("1"))
                        {
                            Frame_TimedControl TimedControl = new Frame_TimedControl() { DeviceNo = TimedConfig.DeviceNo, Timeout = TimedConfig.Timeout };
                            byte[] message = null;
                            if ((SocketListFlag[j].External.External as TcpClientBindingExternalClass).TVersion == "010002")
                            {
                                message = GprsResolveDataV010002.SendJoint_TimedControl(TimedControl);
                            }
                            else if ((SocketListFlag[j].External.External as TcpClientBindingExternalClass).TVersion == "010003")
                            {
                                message = GprsResolveDataV010003.SendJoint_TimedControl(TimedControl);
                            }
                            else if ((SocketListFlag[j].External.External as TcpClientBindingExternalClass).TVersion == "020000")
                            {   
                                message = GprsResolveDataV020000.SendJoint_TimedControl(TimedControl);
                            }
                            if (message != null)
                            {
                                SocketListFlag[j].SendBuffer(message);
                                DB_MysqlFogGun.SaveTimedControl(TimedControl, 1);
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("闹钟执行出现异常", ex.Message + ex.StackTrace);
            }
        }
        //获取定时配置的开机初始化
        static void GetTimingConfigInit()
        {
            DataTable dt = DB_MysqlFogGun.GetTimingConfig();
            if (dt != null)
            {
                int iRows = dt.Rows.Count;
                if (iRows > 0)
                {
                    for (int i = 0; i < iRows; i++)
                    {
                        Frame_TimingConfig TimingConfig = new Frame_TimingConfig();
                        TimingConfig.DeviceNo = dt.Rows[i]["equipmentNo"].ToString();//设备编号
                        TimingConfig.TimingSwitch = dt.Rows[i]["openState"].ToString();//开关状态
                        TimingConfig.Time = dt.Rows[i]["timingstarttime"].ToString();//定时时间
                        TimingConfig.Timeout = dt.Rows[i]["timeout"].ToString();//持续时间
                        TimingConfig.Week = weekProcess(dt.Rows[i]["weeks"].ToString());//星期的初始化
                        if (Frame_TimingConfigSet.ContainsKey(TimingConfig.DeviceNo))
                        {
                            Frame_TimingConfigSet[TimingConfig.DeviceNo] = TimingConfig;
                        }
                        else
                        {
                            Frame_TimingConfigSet.Add(TimingConfig.DeviceNo, TimingConfig);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///雾泡定时喷淋 
        /// </summary>
        private static void GetFoggunSettingTimeWork(IList<TcpSocketClient> SocketList)
        {
            try
            {
                DataTable dt = DB_MysqlFogGun.GetFoggunSettingTimeWork();
                //取得今天周几
                string weekday = ((int)DateTime.Now.DayOfWeek).ToString();
                if (weekday.Trim() == "0") //当是0的时候表示今天周日
                {
                    weekday = "7";
                }

                if (dt != null && dt.Rows.Count > 0)
                {
                    List<FoggunSettingTimeWorkModel> foggunSettingTimeWorkLst = new List<FoggunSettingTimeWorkModel>();
                    foreach (DataRow item in dt.Rows)
                    {
                        string repeatConfig = item["repeatConfig"].ToString();
                        if (repeatConfig.Contains("0") || repeatConfig.Contains(weekday) || repeatConfig.Contains("8"))
                        {
                            FoggunSettingTimeWorkModel model = new FoggunSettingTimeWorkModel();
                            model.equipmentNo = item["equipmentNo"].ToString();
                            try { model.workCycle = Convert.ToInt32(item["workCycle"]); }
                            catch { model.workCycle = 1; }
                            model.workTime = GetTime(item["workTime"].ToString());
                            model.repeatConfig = item["repeatConfig"].ToString();
                            model.OperationMarkTime = item["OperationMarkTime"].ToString();
                            model.uuid = item["uuid"].ToString();
                            foggunSettingTimeWorkLst.Add(model);
                        }
                    }
                    foggunSettingTimeWorkLst.ForEach(x =>
                    {
                        for (int j = 0; j < SocketList.Count; j++)
                        {
                            string DeviceNo = (SocketList[j].External.External as TcpClientBindingExternalClass).EquipmentID;
                            string DeviceNoServer = x.equipmentNo;
                            if (DeviceNo != null && DeviceNo.Trim().Equals(DeviceNoServer.Trim()))
                            {
                                if (x.workTime.Hour == DateTime.Now.Hour && x.workTime.Minute == DateTime.Now.Minute)
                                {
                                    if ((SocketList[j].External.External as TcpClientBindingExternalClass).TVersion == "010002")
                                    {
                                        byte[] message = GprsResolveDataV010002.FoggunSettingTimeWork(x);
                                        string data = ConvertData.ToHexString(message, 0, message.Length);
                                        if (message != null)
                                        {
                                           (SocketList[j].External.External as TcpClientBindingExternalClass).EquipmentTag = 3;
                                            (SocketList[j].External.External as TcpClientBindingExternalClass).uuid = x.uuid;
                                            SocketList[j].SendBuffer(message);
                                            DB_MysqlFogGun.UpdateFoggunSettingTimeWork(DeviceNo, 1,x.uuid);
                                            ToolAPI.XMLOperation.WriteLogXmlNoTail("雾泡定时喷淋发送给设备的数据",data);
                                            if (x.repeatConfig.Contains("0"))
                                            {
                                                DB_MysqlFogGun.UpdateAlarmIsEffective(DeviceNo, x.uuid);
                                            }
                                        }
                                    }
                                    else if ((SocketList[j].External.External as TcpClientBindingExternalClass).TVersion == "010003")
                                    {
                                        byte[] message = GprsResolveDataV010003.FoggunSettingTimeWork(x);
                                        string data = ConvertData.ToHexString(message, 0, message.Length);
                                        if (message != null)
                                        {
                                            (SocketList[j].External.External as TcpClientBindingExternalClass).EquipmentTag = 3;
                                            (SocketList[j].External.External as TcpClientBindingExternalClass).uuid = x.uuid;

                                            SocketList[j].SendBuffer(message);
                                            DB_MysqlFogGun.UpdateFoggunSettingTimeWork(DeviceNo, 1,x.uuid);
                                            ToolAPI.XMLOperation.WriteLogXmlNoTail("雾泡定时喷淋发送给设备的数据", data);
                                            if (x.repeatConfig.Contains("0"))
                                            {
                                                DB_MysqlFogGun.UpdateAlarmIsEffective(DeviceNo, x.uuid);
                                            }
                                        }
                                    }
                                    else if ((SocketList[j].External.External as TcpClientBindingExternalClass).TVersion == "020000")
                                    {
                                        byte[] message = GprsResolveDataV020000.FoggunSettingTimeWork(x);
                                        string data = ConvertData.ToHexString(message, 0, message.Length);
                                        if (message != null)
                                        {
                                            (SocketList[j].External.External as TcpClientBindingExternalClass).EquipmentTag = 3;
                                            (SocketList[j].External.External as TcpClientBindingExternalClass).uuid = x.uuid;

                                            SocketList[j].SendBuffer(message);
                                            DB_MysqlFogGun.UpdateFoggunSettingTimeWork(DeviceNo, 1,x.uuid);
                                            ToolAPI.XMLOperation.WriteLogXmlNoTail("雾泡定时喷淋发送给设备的数据", data);
                                            if (x.repeatConfig.Contains("0"))
                                            {
                                                DB_MysqlFogGun.UpdateAlarmIsEffective(DeviceNo, x.uuid);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    });
                }
            }
            catch(Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("GetFoggunSettingTimeWork异常", ex.Message);
            }
        }
        /// <summary>
        /// 时间戳转化为时间
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        private static DateTime GetTime(string timeStamp)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }
        /// <summary>
        /// 雾泡喷淋时钟 每天00：00把操作状态重置
        /// </summary>
        private static void FoggunSettingTimeWorkReset()
        {
            try
            {
                bool isNewDay = false; 
                while (true)
                {
                    int cycleTime = 0;
                    int hour = DateTime.Now.Hour;
                    if (hour != 23)
                    {
                        cycleTime = 1000 * 60 * 20;  
                    }
                    else
                    {
                        int minute = DateTime.Now.Minute;
                        if (minute > 45)
                        {
                            cycleTime = 1000 * 20;//当进入晚上11点45后循环周期改为20s一次
                        }
                        else
                        {
                            cycleTime = 1000 * 60 * 5;//进入11点改为5分钟一个周期
                        }
                    }

                    if (hour == 0&&isNewDay)
                    {
                        isNewDay = false;
                        DB_MysqlFogGun.UpdateFoggunSettingTimeWork("", 1,"");
                    }
                    else
                    {
                        isNewDay = true;
                    }

                    Thread.Sleep(cycleTime);
                }
            }
            catch
            {
                while (FoggunSettingTimeWork != null&&FoggunSettingTimeWork.IsAlive)
                {
                    FoggunSettingTimeWork.Abort();
                    FoggunSettingTimeWork = null;
                }
            }
        }
    }
}
