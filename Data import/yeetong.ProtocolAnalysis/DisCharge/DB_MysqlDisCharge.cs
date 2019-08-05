using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Architecture;

using SIXH.DBUtility;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
namespace ProtocolAnalysis.DisCharge
{
    public class DB_MysqlDisCharge
    {
        static Dictionary<DbHelperSQL, string> DbNetAndSn = new Dictionary<DbHelperSQL, string>();
        static DB_MysqlDisCharge()
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
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlDisCharge异常", ex.Message);
            }
        }

        #region 存入本地数据库用的
        public static int SaveDisCharge(DBFrame df)
        {
            try
            {
                string sql = string.Format("INSERT INTO discharge (deviceid,datatype,contentjson,contenthex,version) VALUES('{0}','{1}','{2}','{3}','{4}')", df.deviceid, df.datatype, df.contentjson, df.contenthex, df.version);
                int result = DBoperateClass.DBoperateObj.ExecuteNonQuery(sql, null, CommandType.Text);
                if (df.datatype == "parameterUpload")
                {
                    SaveLiftLoop(df.deviceid, df.version, df.contentjson);
                }
                return result;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("SaveDisCharge异常", ex.Message);
                return 0;
            }
        }
        /// <summary>
        /// 保存本地一些卸料的参数信息
        /// </summary>
        /// <param name="deviceid"></param>
        /// <param name="version"></param>
        /// <param name="json"></param>
        static void SaveLiftLoop(string deviceid, string version, string json)
        {
            try
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(json);
                string LoadRating = jo["LoadRating"].ToString();
                string EarlyAlarmCoefficient = jo["EarlyAlarmCoefficient"].ToString();
                string AlarmCoefficient = jo["AlarmCoefficient"].ToString();
                string AngleEarlyAlarm = jo["AngleEarlyAlarm"].ToString();
                string AngleAlarm = jo["AngleAlarm"].ToString();

                IList<DbParameter> paraList = new List<DbParameter>();
                paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@sn", deviceid));
                paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@loads", LoadRating));
                paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@earlyalarm", EarlyAlarmCoefficient));
                paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@alarmcoe", AlarmCoefficient));
                paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@angleEarly", AngleEarlyAlarm));
                paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@angleAlarm", AngleAlarm));
                paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@vs", version));
                //DBoperateClass.DBoperateObj.ExecuteNonQuery("pro_disChargeParamAdd", paraList, CommandType.StoredProcedure);
                int result = DBoperateClass.DBoperateObj.ExecuteNonQuery("pro_disChargeParamAdd", paraList, CommandType.StoredProcedure);

             
            }
            catch { }
        }
        #endregion

        #region 关于命令下发   交互性频繁，直接就是与网络数据库进行交互
        #region IP配置
        /// <summary>
        /// 获取ip配置
        /// </summary>
        /// <returns></returns>
        public static DataTable GetIPConfiguration()
        {
            try
            {
                 DataTable dt = new DataTable();
                 foreach (var item in DbNetAndSn)
                 {
                     DbHelperSQL DbNet = item.Key;
                     if (DbNet != null)
                     {
                         string sql = "select equipmentNo,ip_dn,port from equipment_discharge_orderissued where addr_status=0";
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
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlDisCharge.UpdateDataCongfig异常", ex.Message);
                return null;
            }
        }
        /// <summary>
        /// 更新获得IP的标识
        /// </summary>
        /// <returns></returns>
        public static  int UpdateIPConfiguration(Frame_IPConfiguration IPConfiguration, int flag,bool issuccess)
        {
            try 
            {
                int result = 0;
                DbHelperSQL DbNet = null;
                if (DbNetAndSn.Keys.Count > 0)
                {
                    DbNet = DbNetAndSn.Keys.ToList().First();
                }
                if (DbNet != null)
                {
                    string sql = "";
                    if (issuccess)
                    {
                        sql = string.Format("select ip_dn,port from equipment_discharge_orderissued where equipmentNo='{0}'", IPConfiguration.DeviceNo);
                        DataTable dt = DbNet.ExecuteDataTable(sql, null, CommandType.Text);
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            sql = string.Format("update equipment_discharge_orderissued set addr_status='{0}',addr_time=(select UNIX_TIMESTAMP(now())),ip_dn_backup='{1}',port_backup={2} where equipmentNo='{3}'", flag, dt.Rows[0]["ip_dn"].ToString(), Convert.ToInt32(dt.Rows[0]["port"]), IPConfiguration.DeviceNo);
                            result= DbNet.ExecuteNonQuery(sql, null, CommandType.Text);
                        }
                    }
                    else
                    {
                        sql = string.Format("update equipment_discharge_orderissued set addr_status='{0}',addr_time=(select UNIX_TIMESTAMP(now())) where equipmentNo='{1}'", flag, IPConfiguration.DeviceNo);
                        result= DbNet.ExecuteNonQuery(sql, null, CommandType.Text);
                    }
                    return result;
                }
                return 0;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlDisCharge.UpdateDataCongfig异常", ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// 记录ip的下发记录
        /// </summary>
        public static void RecordCommandIssued(string equipmentNo, int status)
        {
            try
            {
                DbHelperSQL DBNet = DbNetAndSn.Keys.ToList().First();
                if (DBNet != null)
                {
                    string sql = string.Format("select creat_time from equipment_discharge_orderissued_record where equipmentNo='{0}' order by creat_time desc limit 1", equipmentNo);
                    DataTable dt = DBNet.ExecuteDataTable(sql, null, CommandType.Text);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        sql = string.Format("update equipment_discharge_orderissued_record set addr_status='{0}' where equipmentNo='{1}' and creat_time={2}", status, equipmentNo, Convert.ToInt32(dt.Rows[0]["creat_time"]));
                        DBNet.ExecuteNonQuery(sql, null, CommandType.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("RecordCommandIssued异常 discharge", ex.Message);
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
                        paraList.Add(item.Key.CreateDbParameter("@ptype", "2"));
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
    }
}
