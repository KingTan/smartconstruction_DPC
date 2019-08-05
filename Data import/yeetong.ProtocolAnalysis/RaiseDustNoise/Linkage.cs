using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using Architecture;
using SIXH.DBUtility;
using System.Data.Common;

namespace ProtocolAnalysis.FogGun
{
    /// <summary>
    /// 扬尘噪音联动
    /// </summary>
    public class Linkage
    {
        static Dictionary<string, Linkage_Object> Linkage_dic = new Dictionary<string, Linkage_Object>();

        static DbHelperSQL DbNet = null;
        static List<DbHelperSQL> DbNetAndSn = new List<DbHelperSQL>();
        static Linkage()
        {
            try
            {
                string connectionString = ToolAPI.INIOperate.IniReadValue("netSqlGroup", "connectionString", MainStatic.Path);
                string[] connectionStringAry = connectionString.Split(';');
                foreach (string connectionStringTemp in connectionStringAry)
                {
                    string[] dbnetAry = connectionStringTemp.Split('&');
                    DbHelperSQL dbNet = new DbHelperSQL(string.Format("Data Source={0};Port={1};Database={2};User={3};Password={4}", dbnetAry[0], dbnetAry[1], dbnetAry[2], dbnetAry[3], dbnetAry[4]), DbProviderType.MySql);
                    if (dbnetAry[2] == "smartconstruction")
                    {
                        DbNet = dbNet;
                    }
                    DbNetAndSn.Add(dbNet);
                }
                if (DbNet == null && DbNetAndSn.Count > 0)
                {
                    DbNet = DbNetAndSn[0];
                }

                //线程做延时
                Thread ProcessThreadT = new Thread(ProcessThread) { Priority = ThreadPriority.Highest, IsBackground = true };
                ProcessThreadT.Start();
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlTowerCrane异常", ex.Message);
            }
        }

        /// <summary>
        ///时间转换时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp()
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0));
            DateTime nowTime = DateTime.Now;
            long unixTime = (long)System.Math.Round((nowTime - startTime).TotalSeconds, MidpointRounding.AwayFromZero);
            return unixTime.ToString();
        }
        static void ProcessThread()
        {
            while (true)
            {
                try
                {
                    //先获取未联动的设备
                    string equipmentList = "," + GetfogNoLinkage() + ",";
                    List<Linkage_Object> values = Linkage_dic.Values.ToList();
                    List<Linkage_Object> valuesCopy = new List<Linkage_Object>();
                    values.ForEach(item => 
                    {
                        string lastOnLineTime = item.LastOnLineTime;
                        if (lastOnLineTime.Trim().Length > 0)
                        {
                            int sub =Convert.ToInt32(GetTimeStamp())-Convert.ToInt32(item.LastOnLineTime);  //sub是最后上线时间和当前时间的差值
                            if (sub > 120)
                            {
                                item.dicFogLst.Values.ToList().ForEach(x => 
                                {
                                    ManualControlFog(x.Equipment_fog, 0);
                                    ToolAPI.XMLOperation.WriteLogXmlNoTail("无联动移除前关闭", x.Equipment_fog);
                                });
                                Linkage_dic.Remove(item.Equipment_dust);
                            }
                            else
                            {
                                valuesCopy.Add(item);
                            }
                        }
                        else
                        {
                            //应该走不进来
                            Linkage_dic.Remove(item.Equipment_dust);
                        }
                    });

                    valuesCopy.ForEach(item =>
                    {
                       List<FogModel> models = item.dicFogLst.Values.ToList();
                        models.ForEach(x =>
                        {
                            if (equipmentList.Contains(x.Equipment_fog))
                            {
                                ManualControlFog(x.Equipment_fog, 0);
                                ToolAPI.XMLOperation.WriteLogXmlNoTail("无联动移除前关闭", x.Equipment_fog);
                                Linkage_dic[item.Equipment_dust].dicFogLst.Remove(x.Equipment_fog);
                            }
                            else
                            {

                                switch (x.Stage_type)
                                {
                                    case 0: //报警延时阶段
                                        double timeSpan = (DateTime.Now - x.Overalarm_delay_time).TotalSeconds;
                                        if ((DateTime.Now - x.Overalarm_delay_time).TotalSeconds > x.Linkage_overalarm_delay)
                                        {
                                            ManualControlFog(x.Equipment_fog, 1);

                                            //历史记录
                                            RecordFog_linkage_History(x.Equipment_fog, item.Equipment_dust, 1);
                                            ToolAPI.XMLOperation.WriteLogXmlNoTail("打开雾泡", x.Equipment_fog);
                                            Linkage_dic[item.Equipment_dust].dicFogLst[x.Equipment_fog].Single_time = DateTime.Now;
                                            Linkage_dic[item.Equipment_dust].dicFogLst[x.Equipment_fog].Stage_type = 1;
                                        }
                                        break;
                                    case 1: //把开启和关闭的间隔都算在一起了
                                        timeSpan = (DateTime.Now - x.Single_time).TotalSeconds;
                                        if ((DateTime.Now - x.Single_time).TotalSeconds > (x.Linkage_single_time))
                                        {
                                            ManualControlFog(x.Equipment_fog, 0);
                                            ToolAPI.XMLOperation.WriteLogXmlNoTail("无报警移除前关闭", x.Equipment_fog);

                                            //历史记录
                                            RecordFog_linkage_History(x.Equipment_fog,item.Equipment_dust,0);

                                            Linkage_dic[item.Equipment_dust].dicFogLst[x.Equipment_fog].Interval_time = DateTime.Now;
                                            Linkage_dic[item.Equipment_dust].dicFogLst[x.Equipment_fog].Stage_type = 2;
                                        }
                                        break;
                                    case 2:
                                        timeSpan= (DateTime.Now - x.Interval_time).TotalSeconds;
                                        if ((DateTime.Now - x.Interval_time).TotalSeconds > (x.Linkage_interval_time))
                                        {
                                            ManualControlFog(x.Equipment_fog, 1);
                                            ToolAPI.XMLOperation.WriteLogXmlNoTail("打开雾泡", x.Equipment_fog);
                                            Linkage_dic[item.Equipment_dust].dicFogLst[x.Equipment_fog].Single_time = DateTime.Now;
                                            Linkage_dic[item.Equipment_dust].dicFogLst[x.Equipment_fog].Stage_type = 1;
                                        }
                                        break;
                                }
                            }
                        });
                        if (Linkage_dic[item.Equipment_dust].dicFogLst.Count <= 0)
                        {
                            Linkage_dic.Remove(item.Equipment_dust);
                        }
                    });
                }
                catch (Exception ex)
                {
                    ToolAPI.XMLOperation.WriteLogXmlNoTail("雾炮联动线程异常", ex.Message);
                }
                Thread.Sleep(30000);
            }
        }

        static public void Dust_data_Process(Linkage_dust linkage_dust)
        {
            DataTable dt = GetDust(linkage_dust.Equipment);
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    bool isAlarm = false;
                    float temp = 0;
                    if (dr["pm25Warn"] != null && float.TryParse(dr["pm25Warn"].ToString(), out temp))
                    {
                        if (linkage_dust.PM25 > temp)//判断开始联动
                        {
                            isAlarm = true;
                        }
                    }
                    if (dr["pm10Warn"] != null && float.TryParse(dr["pm10Warn"].ToString(), out temp))
                    {
                        if (linkage_dust.PM10 > temp)//判断开始联动
                        {
                            isAlarm = true;
                        }
                    }
                    //判断是否报警
                    if (isAlarm)//报警
                    {
                        DataTable fogDt = GetDustLinkFog(linkage_dust.Equipment);
                        if (!Linkage_dic.ContainsKey(linkage_dust.Equipment))
                        {
                            if (fogDt != null && fogDt.Rows.Count > 0)
                            {
                                Linkage_Object linkage_Object = new Linkage_Object();
                                linkage_Object.Equipment_dust = linkage_dust.Equipment;
                                try
                                {
                                    string time= dr["lastOnLineTime"].ToString(); 
                                    linkage_Object.LastOnLineTime = dr["lastOnLineTime"].ToString();
                                }
                                catch(Exception EX)
                                {
                                    linkage_Object.LastOnLineTime = "";
                                }
                                foreach (DataRow item in fogDt.Rows)
                                {
                                    FogModel model = new FogModel();
                                    model.Equipment_fog = item["equipmentNo"].ToString();
                                    try
                                    {
                                        model.Linkage_overalarm_delay = Convert.ToInt32(item["linkage_overalarm_delay"]);
                                    }
                                    catch
                                    {
                                        model.Linkage_overalarm_delay = 0;
                                    }
                                    try
                                    {
                                        model.Linkage_single_time = Convert.ToInt32(item["linkage_single_time"]);
                                    }
                                    catch
                                    {
                                        model.Linkage_single_time = 0;
                                    }
                                    try
                                    {
                                        model.Linkage_interval_time = Convert.ToInt32(item["linkage_interval_time"]);
                                    }
                                    catch
                                    {
                                        model.Linkage_interval_time = 0;
                                    }
                                    model.Linkage_alarm_reference_mode = item["linkage_alarm_reference_mode"].ToString();
                                    if (!linkage_Object.dicFogLst.ContainsKey(model.Equipment_fog))
                                    {
                                        linkage_Object.dicFogLst.Add(model.Equipment_fog, model);
                                    } 
                                }
                                Linkage_dic.Add(linkage_Object.Equipment_dust, linkage_Object);
                            }
                        }
                        else
                        {
                            if (fogDt != null && fogDt.Rows.Count > 0)
                            {
                                try
                                {
                                    Linkage_dic[linkage_dust.Equipment].LastOnLineTime = dr["lastOnLineTime"].ToString();
                                }
                                catch
                                {
                                    Linkage_dic[linkage_dust.Equipment].LastOnLineTime = "";
                                }

                                Linkage_dic[linkage_dust.Equipment].dicFogLst.Values.ToList().ForEach(fog =>
                                {
                                   
                                    foreach (DataRow row in fogDt.Rows)
                                    {
                                        if (row["equipmentNo"].ToString() == fog.Equipment_fog)
                                        {
                                            try
                                            {
                                                fog.Linkage_overalarm_delay = Convert.ToInt32(row["linkage_overalarm_delay"]);
                                            }
                                            catch
                                            {
                                                fog.Linkage_overalarm_delay = 0;
                                            }
                                            try
                                            {
                                                fog.Linkage_single_time = Convert.ToInt32(row["linkage_single_time"]);
                                            }
                                            catch
                                            {
                                                fog.Linkage_single_time = 0;
                                            }
                                            try
                                            {
                                                fog.Linkage_interval_time = Convert.ToInt32(row["linkage_interval_time"]);
                                            }
                                            catch
                                            {
                                                fog.Linkage_interval_time = 0;
                                            }
                                            fog.Linkage_alarm_reference_mode = row["linkage_alarm_reference_mode"].ToString();
                                        }
                                    }
                                });
                            }
                        }
                    }
                    else
                    {
                        if (Linkage_dic.ContainsKey(linkage_dust.Equipment))//不存在不进行处理  存在就要把设备关闭掉且移除这一项
                        {
                            Linkage_dic[linkage_dust.Equipment].dicFogLst.Values.ToList().ForEach(item => 
                            {
                                ManualControlFog(item.Equipment_fog, 0);
                                ToolAPI.XMLOperation.WriteLogXmlNoTail("无报警移除前关闭",item.Equipment_fog);
                            }); 
                            //先关闭再移除
                            Linkage_dic.Remove(linkage_dust.Equipment);
                        }
                    }
                }
            }
        }


        #region 数据库操作
        //根据设备编号获得扬尘设备的信息
        public static DataTable GetDust(string equipmentNo)
        {
            try
            {
                if (DbNet != null)
                {
                    IList<DbParameter> paraList = new List<DbParameter>();
                    paraList.Add(DbNet.CreateDbParameter("@p_equipmentNo", equipmentNo));
                    DataTable o = DbNet.ExecuteDataTable("pro_DustLinkFoggun", paraList, CommandType.StoredProcedure);
                    return o;
                }
                return null;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("IsExistCard异常", ex.Message+"设备号："+equipmentNo);
                return null;
            }
        }
        /// <summary>
        /// 得到未关联的雾炮
        /// </summary>
        /// <param name="equipmentNo"></param>
        /// <returns></returns>
        public static string GetfogNoLinkage()
        {
            try
            {
                if (DbNet != null)
                {
                    string sql = string.Format(@"select GROUP_CONCAT(a.equipmentNo) equipmentNo  FROM (SELECT equipmentNo FROM `equipment_basics` LEFT JOIN equipment_link on equipment_basics.uuid=equipment_link.wuuid
 where equipment_link.linkage_is='false' and equipment_basics.type=4  GROUP BY equipmentNo ) a");
                    DataTable o = DbNet.ExecuteDataTable(sql, null);
                    if (o != null && o.Rows.Count > 0)
                    {
                        return o.Rows[0]["equipmentNo"].ToString();
                    }
                    return "";
                }
                return "";
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("GetfogNoLinkage异常", ex.Message);
                return "";
            }
        }

        /// <summary>
        /// 控制雾炮的喷淋
        /// </summary>
        /// <param name="type"> 0表示</param>
        /// <returns></returns>
        public static int ControlFog(string devno, string timeout)
        {
            try
            {
               
                return 0;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlFogGun.ControlFog异常", ex.Message);
                return 0;
            }
        }
        /// <summary>
        /// 控制雾炮 用的是手动控制
        /// </summary>
        /// <param name="devno"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static int ManualControlFog(string devno, int type)
        {
            try
            {
                int result = 0;
                return result;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlFogGun.ControlFog异常", ex.Message);
                return 0;
            }
        }
        #endregion
        /// <summary>
        /// 通过
        /// </summary>
        /// <param name="equipmentNo"></param>
        /// <returns></returns>
        public static DataTable GetDustLinkFog(string equipmentNo)
        {
            try
            {
                if (DbNet != null)
                {
                    /*select wuuid, b.equipmentNo ,linkage_overalarm_delay,linkage_single_time,linkage_interval_time,linkage_alarm_reference_mode from equipment_link
                    LEFT JOIN equipment_basics b ON(equipment_link.wuuid = b.uuid)
                    where yuuid= (select uuid from equipment_basics where equipmentNo = 'YC554433')
                   */
            
                    DataTable table = new DataTable();
                    string sql = string.Format("select wuuid,b.equipmentNo,linkage_overalarm_delay,linkage_single_time,linkage_interval_time,linkage_alarm_reference_mode from  " +
                                               "equipment_link  LEFT JOIN equipment_basics b ON(equipment_link.wuuid = b.uuid)   where yuuid=(select uuid from equipment_basics where equipmentNo='{0}')", equipmentNo);
                    table= DbNet.ExecuteDataTable(sql, null);
                    return table;
                }
                return null;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("GetDustLinkFog", ex.Message + "设备号：" + equipmentNo);
                return null;
            }
        }
        public static void RecordFog_linkage_History(string fog_equipmentNo,string dust_equipmentNo,int status) 
        {
            try
            {
                if (DbNet != null)
                {
                    IList<DbParameter> paraList = new List<DbParameter>();
                    paraList.Add(DbNet.CreateDbParameter("@fog_equipmentNo_pro", fog_equipmentNo));
                    paraList.Add(DbNet.CreateDbParameter("@dust_equipmentNo_pro", dust_equipmentNo));
                    paraList.Add(DbNet.CreateDbParameter("status_pro", status)); 
                    DataTable o = DbNet.ExecuteDataTable("pro_Fog_Linkage_pro", paraList, CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("IsExistCard异常", ex.Message + "雾泡设备号：" + fog_equipmentNo+"扬尘设备号："+ dust_equipmentNo);
            }
        }
    }

    /// <summary>
    /// 扬尘噪音联动对象
    /// </summary>
    public class Linkage_Object
    {
        /// <summary>
        /// 扬尘的设备编号
        /// </summary>
        public string Equipment_dust { get; set; }
        public string LastOnLineTime { get; set; } 
        /// <summary>
        /// 是否联动
        /// </summary>
        public string Linkage_is { get; set; }
        /// <summary>
        /// 扬尘设备关联的雾泡
        /// </summary>
        public Dictionary<string,FogModel> dicFogLst = new Dictionary<string,FogModel>();

        public Linkage_Object()
        {
            Linkage_is = "true";
        }
    }

    /// <summary>
    /// 雾泡的实体对象
    /// </summary>
    public class FogModel
    {
        /// <summary>
        /// 雾炮的设备编号
        /// </summary>
        public string Equipment_fog { get; set; }
        /// <summary>
        /// 报警延时时间 秒
        /// </summary>
        public int Linkage_overalarm_delay { get; set; }
        /// <summary>
        /// 单次工作时长 秒
        /// </summary>
        public int Linkage_single_time { get; set; }
        /// <summary>
        /// 联动间隔时长 秒
        /// </summary>
        public int Linkage_interval_time { get; set; }
        /// <summary>
        /// 报警延时计时初始
        /// </summary>
        public DateTime Overalarm_delay_time { get; set; }
        /// <summary>
        /// 工作开始时间初始
        /// </summary>
        public DateTime Single_time { get; set; }
        /// <summary>
        /// 结束工作时间初始
        /// </summary>
        public DateTime Interval_time { get; set; }
        /// <summary>
        /// 工作阶段标识
        /// 0报警延时阶段  1正在工作  2循环等待间隔
        /// </summary>
        public int Stage_type { get; set; }
        /// <summary>
        /// 报警参考方式
        /// </summary>
        public string Linkage_alarm_reference_mode { get; set; }
        public FogModel()
        {
            Overalarm_delay_time = DateTime.Now;
            Single_time = DateTime.Now;
            Interval_time = DateTime.Now;
            Stage_type = 0;
        }
    }

    /// <summary>
    /// 联动需要扬尘数据源
    /// </summary>
    public class Linkage_dust
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        public string Equipment { set; get; }
        /// <summary>
        /// PM2.5
        /// </summary>
        public float PM25 { set; get; }
        /// <summary>
        /// PM10
        /// </summary>
        public float PM10 { set; get; }
    }
}
