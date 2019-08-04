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
namespace ProtocolAnalysis.RaiseDustNoise
{
    public class DB_MysqlRaiseDustNoise
    {
        static Dictionary<DbHelperSQL, string> DbNetAndSn = new Dictionary<DbHelperSQL, string>();
        static DB_MysqlRaiseDustNoise()
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
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlRaiseDustNoise异常", ex.Message);
            }
        }

        #region 存入本地数据库用的
        public static int SaveRaiseDustNoise(DBFrame df)
        {
            try
            {
                string sql = string.Format("INSERT INTO dust (deviceid,datatype,contentjson,contenthex,version) VALUES('{0}','{1}','{2}','{3}','{4}')", df.deviceid, df.datatype, df.contentjson, df.contenthex, df.version);
                int result = DBoperateClass.DBoperateObj.ExecuteNonQuery(sql, null, CommandType.Text);
                return result;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("SaveRaiseDustNoise异常", ex.Message);
                return 0;
            }
        }
        #endregion
        /// <summary>
        /// 读取要配置的ip
        /// </summary>
        /// <returns></returns>
        public static DataTable GetIpCongfig()
        {
            try
            {
                DataTable dt = new DataTable();
                DbHelperSQL DbNet= DbNetAndSn.Keys.ToList().First();
                if (DbNet != null)
                {
                    string sql = "select equipmentNo,ip_dn,port from equipment_dustnoise_orderissued where addr_status=0";
                    DataTable dttemp = DbNet.ExecuteDataTable(sql, null, CommandType.Text);
                    if (dttemp != null)
                    {
                        dt.Merge(dttemp);
                    }
                }
                return dt;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlNoise.GetIpCongfig异常", ex.Message);
                return null;
            }
        }
        /// <summary>
        /// 扬尘参数配置
        /// </summary>
        /// <returns></returns>
        public static DataTable GetParam()
        {
            try
            {
                DataTable dt = new DataTable();
                foreach (var item in DbNetAndSn)
                {
                    DbHelperSQL DbNet = item.Key;
                    if (DbNet != null)
                    {
                        string sql = " select equipmentNo,pm2_5_Alarm,pm10_Alarm,noise_pattern,noise_cycle,noise_oc,pm2_5_factor,pm_10_factor,tsp_factor from equipment_dustnoise_parameter where noise_status=0";
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
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlNoise.GetParam异常", ex.Message);
                return null;
            }
        }
        /// <summary>
        /// 更改ip状态
        /// </summary>
        /// <param name="sn"></param>
        public static int UpdateDataCongfig(string sn,string tag,bool issuccess)
        {
            try
            {
                DbHelperSQL DbNet = DbNetAndSn.Keys.ToList().First();
                int result = 0;
                if (DbNet != null)
                {
                    string sql = "";
                    if (issuccess)
                    {
                        sql = string.Format("select ip_dn,port from equipment_dustnoise_orderissued where equipmentNo='{0}'",sn);
                        DataTable dt = DbNet.ExecuteDataTable(sql, null, CommandType.Text);
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            sql = string.Format("update equipment_dustnoise_orderissued set addr_status='{0}',addr_time=(select UNIX_TIMESTAMP(now())),ip_dn_backup='{1}',port_backup={2} where equipmentNo='{3}'", tag, dt.Rows[0]["ip_dn"].ToString(), Convert.ToInt32(dt.Rows[0]["port"]),sn);
                            result = DbNet.ExecuteNonQuery(sql, null, CommandType.Text);
                        }
                    }
                    else
                    {
                           sql = string.Format("update equipment_dustnoise_orderissued set addr_status='{0}',addr_time=(select UNIX_TIMESTAMP(now())) where equipmentNo='{1}'", tag,sn);
                           result = DbNet.ExecuteNonQuery(sql, null, CommandType.Text);
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlNoise.UpdateDataCongfig异常", ex.Message);
            }
            return 0;
        }
        /// <summary>
        /// 记录IP更改下发的记录
        /// </summary>
        public static void RecordIPCommandIssued(string equipmentNo, int status)
        {
            try
            {
                DbHelperSQL DBNet = DbNetAndSn.Keys.ToList().First();
                if (DBNet != null)
                {
                    string sql = string.Format("select creat_time from equipment_dustnoise_orderissued_record where equipmentNo='{0}' order by creat_time desc limit 1", equipmentNo);
                    DataTable dt = DBNet.ExecuteDataTable(sql, null, CommandType.Text);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        sql = string.Format("update equipment_dustnoise_orderissued_record set addr_status='{0}' where equipmentNo='{1}' and creat_time={2}", status, equipmentNo, Convert.ToInt32(dt.Rows[0]["creat_time"]));
                        DBNet.ExecuteNonQuery(sql, null, CommandType.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("UpdateIPCommandIssued异常", ex.Message);
            }
        }
        /// <summary>
        /// 更改参数
        /// </summary>
        /// <param name="sn"></param>
        public static int UpdateDataParam(string sn,string TAG)
        {
            try
            {
                if (DbNetAndSn.Keys.Count > 0)
                {
                    DbHelperSQL DbNet = DbNetAndSn.Keys.First();
                    if (DbNet != null)
                    {
                        string sql = string.Format("update equipment_dustnoise_parameter set noise_status='{0}' where equipmentNo='{1}'", TAG, sn);
                        return DbNet.ExecuteNonQuery(sql, null, CommandType.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlNoise.UpdateDataParam异常", ex.Message);
            }
            return 0;
        }
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
                        paraList.Add(item.Key.CreateDbParameter("@ptype", "4"));
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
