using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Architecture;
using ProtocolAnalysis.TowerCrane._021303;
using ProtocolAnalysis.TowerCrane.OE;
using SIXH.DBUtility;
namespace ProtocolAnalysis.FogGun
{
    public class DB_MysqlFogGun
    {
        static Dictionary<DbHelperSQL, string> DbNetAndSn = new Dictionary<DbHelperSQL, string>();
        static DB_MysqlFogGun()
        {
            try
            {
                string connectionString = ToolAPI.INIOperate.IniReadValue("netSqlGroup", "connectionString", MainStatic.Path);
                string[] connectionStringAry = connectionString.Split(';');
                foreach (string connectionStringTemp in connectionStringAry)
                {
                    string[] dbnetAry = connectionStringTemp.Split('&');
                    DbHelperSQL dbNet = new DbHelperSQL(string.Format("Data Source={0};Port={1};Database={2};User={3};Password={4}", dbnetAry[0], dbnetAry[1], dbnetAry[2], dbnetAry[3], dbnetAry[4]), DbProviderType.MySql);
                    DbNetAndSn.Add(dbNet, "");
                }
                DbNetAndSnInit();
                Thread UpdateDbNetAndSnT = new Thread(UpdateDbNetAndSn) { IsBackground = true };
                UpdateDbNetAndSnT.Start();
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlFogGun异常", ex.Message);
            }
        }

        #region 存入本地数据库用的
        public static int SaveFogGun(DBFrame df)
        {
            try
            {
                string sql = string.Format("INSERT INTO foggun (deviceid,datatype,contentjson,contenthex,version) VALUES('{0}','{1}','{2}','{3}','{4}')", df.deviceid, df.datatype, df.contentjson, df.contenthex, df.version);
                int result = DBoperateClass.DBoperateObj.ExecuteNonQuery(sql, null, CommandType.Text);
                return result;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("SaveFogGun异常", ex.Message);
                return 0;
            }
        }
        #endregion

        #region 关于命令下发   交互性频繁，直接就是与网络数据库进行交互
        #region 定时配置
        /// <summary>
        /// 获取定时配置
        /// </summary>
        /// <returns></returns>
        static public DataTable GetTimingConfig()
        {
            try
            {
                DataTable dt = new DataTable();
                foreach (var item in DbNetAndSn)
                {
                    DbHelperSQL DbNet = item.Key;
                    if (DbNet != null)
                    {
                        string sql = "select equipmentNo,openState,timingstarttime,timeout,weeks from equipment_foggun_time_parament where resposestate=0";
                        DataTable dttemp = DbNet.ExecuteDataTable(sql, null, CommandType.Text);
                        if (dttemp != null)
                        {
                            dt.Merge(dttemp);
                        }
                    }
                }
                return dt;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlFogGun.GetTimingConfig异常", ex.Message);
                return null;
            }
        }
        /// <summary>
        /// 更新定时配置应答
        /// </summary>
        /// flag 0更新的是被获取到了，1更新的是应答到了
        /// <returns></returns>
        static public int SaveTimingConfig(Frame_TimingConfig TimingConfig, int flag)
        {
            try
            {
                DbHelperSQL DbNet = GetDbHelperSQL(TimingConfig.DeviceNo);
                if (DbNet != null)
                {
                    string sql = "update equipment_foggun_time_parament set resposestate=" + flag + " where  equipmentNo='" + TimingConfig.DeviceNo + "'";
                    int result = DbNet.ExecuteNonQuery(sql, null, CommandType.Text);
                    return result;
                }
                return 0;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlFogGun.SaveTimingConfig异常", ex.Message);
                return 0;
            }
        }
        #endregion

        #region 手动控制
        /// <summary>
        /// 获取手动控制
        /// </summary>
        /// <returns></returns>
        static public DataTable GetManualControl()
        {
            try
            {
                DataTable dt = new DataTable();
                foreach (var item in DbNetAndSn)
                {
                    DbHelperSQL DbNet = item.Key;
                    if (DbNet != null)
                    {
                        string sql = "select equipmentNo,open_state from equipment_foggun_control where respose_state=0";
                        DataTable dttemp = DbNet.ExecuteDataTable(sql, null, CommandType.Text);
                        if (dttemp != null)
                        {
                            dt.Merge(dttemp);
                        }
                    }
                }
                return dt;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlFogGun.GetManualControl异常", ex.Message);
                return null;
            }
        }
        /// <summary>
        /// 更新手动控制应答
        /// </summary>
        /// flag 0更新的是被获取到了，1更新的是应答到了
        /// <returns></returns>
        static public int SaveManualControl(Frame_ManualControl ManualControl, int flag)
        {
            try
            {
                DbHelperSQL DbNet = GetDbHelperSQL(ManualControl.DeviceNo);
                if (DbNet != null)
                {
                    string sql = "update equipment_foggun_control set respose_state=" + flag + " where  equipmentNo='" + ManualControl.DeviceNo + "'";
                    int result = DbNet.ExecuteNonQuery(sql, null, CommandType.Text);
                    return result;
                }
                return 0;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlFogGun.SaveManualControl异常", ex.Message);
                return 0;
            }
        }
        #endregion

        #region 定时控制
        /// <summary>
        /// 更新定时控制应答
        /// </summary>
        /// flag 0更新的是被获取到了，1更新的是应答到了
        /// <returns></returns>
        static public int SaveTimedControl(Frame_TimedControl TimedControl, int flag)
        {
            //IN `equipmentNo_p` varchar(20),IN `equipmentTime_p` datetime,IN `timeOut_p` int,IN `resposestate_p` int
            try
            {
                DbHelperSQL DbNet = GetDbHelperSQL(TimedControl.DeviceNo);
                if (DbNet != null)
                {
                    string sql = "update equipment_foggun_control set respose_state=" + flag + " where  equipmentNo='" + TimedControl.DeviceNo + "'";
                    int result = DbNet.ExecuteNonQuery(sql, null, CommandType.Text);
                    return result;
                    //IList<DbParameter> paraList = new List<DbParameter>();
                    //paraList.Add(DbNet.CreateDbParameter("@equipmentNo_p", TimedControl.DeviceNo));
                    //paraList.Add(DbNet.CreateDbParameter("@equipmentTime_p", DateTime.Now));
                    //paraList.Add(DbNet.CreateDbParameter("@timeOut_p", TimedControl.Timeout));
                    //paraList.Add(DbNet.CreateDbParameter("@resposestate_p", flag));
                    //return DbNet.ExecuteNonQuery("Fog_SaveTimeControl", paraList, CommandType.StoredProcedure);
                }
                return 0;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlFogGun.SaveTimedControl异常", ex.Message);
                return 0;
            }
        }
        #endregion

        #region 更改IP
        /// <summary>
        /// 获取IP
        /// </summary>
        /// <returns></returns>
        static public DataTable GetUpdateIpPort()
        {
            try
            {
                DataTable dt = new DataTable();
                foreach (var item in DbNetAndSn)
                {
                    DbHelperSQL DbNet = item.Key;
                    if (DbNet != null)
                    {
                        string sql = "select equipmentNo,ip_dn,port,addr_status from equipment_foggun_orderissued where addr_status=0";
                        DataTable dttemp = DbNet.ExecuteDataTable(sql, null, CommandType.Text);
                        if (dttemp != null)
                        {
                            dt.Merge(dttemp);
                        }
                    }
                }
                return dt;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlFogGun.GetUpdateIpPort异常", ex.Message);
                return null;
            }
        }
        /// <summary>
        /// IP的更改应答
        /// </summary>
        /// <param name="UpdateIpPort"></param>
        /// <returns></returns>
        static public int SaveUpdateIpPort(Frame_UpdateIpPort UpdateIpPort)
        {
            try
            {
                DbHelperSQL DBNet = DbNetAndSn.Keys.ToList().First();
                int result = 0;
                if (DBNet != null)
                {
                    string sql = "";
                    if (UpdateIpPort.issuccess)
                    {
                        sql = string.Format("select ip_dn,port from equipment_foggun_orderissued where equipmentNo='{0}'", UpdateIpPort.DeviceNo);
                        DataTable dt = DBNet.ExecuteDataTable(sql, null, CommandType.Text);
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            sql = string.Format("update equipment_foggun_orderissued set addr_status='{0}',addr_time=(select UNIX_TIMESTAMP(now())),ip_dn_backup='{1}',port_backup={2} where equipmentNo='{3}'", UpdateIpPort.State, dt.Rows[0]["ip_dn"].ToString(), Convert.ToInt32(dt.Rows[0]["port"]), UpdateIpPort.DeviceNo);
                            result = DBNet.ExecuteNonQuery(sql, null, CommandType.Text);
                        }
                    }
                    else
                    {
                        sql = string.Format("update equipment_foggun_orderissued set addr_status='{0}',addr_time=(select UNIX_TIMESTAMP(now())) where equipmentNo='{1}'", UpdateIpPort.State, UpdateIpPort.DeviceNo);
                        result = DBNet.ExecuteNonQuery(sql, null, CommandType.Text);
                    }
                    return result;
                }
                return 0;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlFogGun.SaveUpdateIpPort异常", ex.Message);
                return 0;
            }
        }
        /// <summary>
        /// 雾泡IP地址更改记录
        /// </summary>
        /// <param name="equipmentNo"></param>
        /// <param name="status"></param>
        public static void RecordCommandIssued(string equipmentNo, int status)
        {
            try
            {
                DbHelperSQL DBNet = DbNetAndSn.Keys.ToList().First();
                if (DBNet != null)
                {
                    string sql = string.Format("select creat_time from equipment_foggun_orderissued_record where equipmentNo='{0}' order by creat_time desc limit 1", equipmentNo);
                    DataTable dt = DBNet.ExecuteDataTable(sql, null, CommandType.Text);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        sql = string.Format("update equipment_foggun_orderissued_record set addr_status='{0}' where equipmentNo='{1}' and creat_time={2}", status, equipmentNo, Convert.ToInt32(dt.Rows[0]["creat_time"]));
                        DBNet.ExecuteNonQuery(sql, null, CommandType.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("UpdateIPCommandIssued异常", ex.Message);
            }
        }
        static public int SaveUpdateIpPortSuccess(string sn)
        {
            try
            {
                DbHelperSQL DbNet = GetDbHelperSQL(sn);
                if (DbNet != null)
                {
                    string sql = "update equipment_foggun_orderissued set addr_status=2 where  equipmentNo='" + sn + "'";
                    int result = DbNet.ExecuteNonQuery(sql, null, CommandType.Text);
                    return result;
                }
                return 0;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlFogGun.SaveUpdateIpPort异常", ex.Message);
                return 0;
            }
        }
        #endregion
        #endregion

        #region 设备列表的更新
        public static void UpdateDbNetAndSn()
        {
            while (true)
            {
                Thread.Sleep(180000);//3分钟循环一次
                DbNetAndSnInit();
            }
        }
        static void DbNetAndSnInit()
        {
            try
            {
                int flag = 0;
                Dictionary<DbHelperSQL, string> DbNetAndSnTemp = new Dictionary<DbHelperSQL, string>();
                foreach (var item in DbNetAndSn)
                {
                    if (item.Key != null)
                    {
                        IList<DbParameter> paraList = new List<DbParameter>();
                        paraList.Add(item.Key.CreateDbParameter("@ptype", "3"));
                        DataTable o = item.Key.ExecuteDataTable("pro_MosaicStr", paraList, CommandType.StoredProcedure);
                        string value = "";
                        if (o != null && o.Rows.Count > 0)
                            value = o.Rows[0]["classIdsAll"].ToString();
                        DbNetAndSnTemp.Add(item.Key, value);
                    }
                    flag++;
                }
                DbNetAndSn = DbNetAndSnTemp;
            }
            catch { }
        }
        static DbHelperSQL GetDbHelperSQL(string CraneNo)
        {
            try
            {
                foreach (var item in DbNetAndSn)
                {
                    if (!string.IsNullOrEmpty(item.Value) && !string.IsNullOrEmpty(CraneNo))
                    {
                        if (item.Value.Contains(CraneNo))
                            return item.Key;
                    }
                }
                return null;
            }
            catch (Exception ex)
            { return null; }
        }
        #endregion

        //手动控制的配置
        static public int ManualControl(string devno, int openstate_p, int resposestate_p)
        {
            //IN `equipmentNo_p` varchar(20),IN `equipmentTime_p` datetime,IN `timeOut_p` int,IN `resposestate_p` int
            try
            {
                DbHelperSQL DbNet = GetDbHelperSQL(devno);
                if (DbNet != null)
                {
                    IList<DbParameter> paraList = new List<DbParameter>();
                    paraList.Add(DbNet.CreateDbParameter("@equipmentNo_p", devno));
                    paraList.Add(DbNet.CreateDbParameter("@openstate_p", openstate_p));
                    paraList.Add(DbNet.CreateDbParameter("@resposestate_p", resposestate_p));
                    return DbNet.ExecuteNonQuery("Fog_SaveManua_lControl", paraList, CommandType.StoredProcedure);
                }
                return 0;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlFogGun.ManualControl异常", ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// 获取可以下发的雾泡定时控制
        /// </summary>
        /// <param name="weekday"></param>
        /// <returns></returns>
        public static DataTable GetFoggunSettingTimeWork()
        {
            try
            {
                DataTable dt = new DataTable();
                if (DbNetAndSn.Keys.Count > 0)
                {
                    DbHelperSQL DbNet = DbNetAndSn.First().Key;
                    string sql = string.Format("select * from equipment_foggun_settime_working where OperationMark=0 and IsEffective=1 and respose_state=0");
                    dt= DbNet.ExecuteDataTable(sql, null, CommandType.Text);
                }
                return dt;
            }
            catch(Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlFogGun.GetFoggunSettingTimeWork", ex.Message);
                return null;
            }
        }
        /// <summary>
        /// 更新定时喷淋的状态
        /// </summary>
        /// <param name="equmentID"></param>
        /// <param name="status"></param>
        public static void UpdateFoggunSettingTimeWork(string equmentID,int status,string uuid)
        {
            try
            {
                if (DbNetAndSn.Keys.Count > 0)
                {
                    DbHelperSQL DbNet = DbNetAndSn.First().Key;
                    string sql = "";
                    if (equmentID.Trim().Length > 0)
                    {
                        if (status == 1)//捕捉到命令准备下发
                        {
                            sql = string.Format("update equipment_foggun_settime_working set respose_state={0},OperationMarkTime= UNIX_TIMESTAMP(NOW()) where equipmentNo='{1}' and uuid='{2}'", status, equmentID,uuid);
                        }
                        else if(status==2)//下发后的回复
                        {
                            sql = string.Format("update equipment_foggun_settime_working set respose_state={0},OperationMark=1,OperationMarkTime= UNIX_TIMESTAMP(NOW()) where equipmentNo='{1}' and uuid='{2}'", status, equmentID,uuid);
                            RecordFoggunSettingTimeWorkHistory(equmentID, uuid);
                        }
                    }
                    else//每天00：00的标识更新
                    {
                       sql = string.Format("update equipment_foggun_settime_working set OperationMark=0,respose_state=0,OperationMarkTime= UNIX_TIMESTAMP(NOW()) where IsEffective=1 and repeatConfig!=0");
                    }
                    DbNet.ExecuteNonQuery(sql, null, CommandType.Text);
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlFogGun.UpdateFoggunSettingTimeWork", ex.Message);
            }
        }
        /// <summary>
        /// 雾泡定时喷淋记录
        /// </summary>
        /// <param name="equipmentID"></param>
        public static void RecordFoggunSettingTimeWorkHistory(string equipmentID,string uuid)
        {
            try
            {
                if (DbNetAndSn.Keys.Count > 0)
                {
                    ToolAPI.XMLOperation.WriteLogXmlNoTail("定时喷淋添加历史记录",equipmentID);
                    DbHelperSQL DbNet = DbNetAndSn.First().Key;
                    string sql = string.Format("INSERT INTO equipment_fog_settime_work_history (fog_equipmentNo,starttime,worktime) VALUES ('{0}',UNIX_TIMESTAMP(NOW()),(SELECT workCycle FROM equipment_foggun_settime_working WHERE equipmentNo='{0}' and uuid='{1}'))", equipmentID,uuid);
                    DbNet.ExecuteNonQuery(sql, null, CommandType.Text);
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail(".RecordFoggunSettingTimeWorkHistory 异常", ex.Message);
            
            }
        }
        /// <summary>
        /// 闹钟仅响一次时
        /// </summary>
        /// <param name="equipmentID"></param>
        /// <param name="uuid"></param>
        public static void UpdateAlarmIsEffective(string equipmentID, string uuid)
        {
            try
            {
                if (DbNetAndSn.Keys.Count > 0)
                {
                    DbHelperSQL DbNet = DbNetAndSn.First().Key;
                   string sql = string.Format("update equipment_foggun_settime_working set IsEffective=0,OperationMarkTime= UNIX_TIMESTAMP(NOW()) where equipmentNo='{0}' and uuid='{1}'", equipmentID,uuid);
                    DbNet.ExecuteNonQuery(sql, null, CommandType.Text);
                }
            }
            catch(Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail(".UpdateAlarmIsEffective 异常", ex.Message);
            }
        }
    }
}
