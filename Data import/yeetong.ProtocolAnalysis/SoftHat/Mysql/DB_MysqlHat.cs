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

namespace ProtocolAnalysis.SoftHat.Mysql
{
   public class DB_MysqlHat
    {

        static Dictionary<DbHelperSQL, string> DbNetAndSn = new Dictionary<DbHelperSQL, string>();
        static DbHelperSQL dbNetFace = null;


        static DB_MysqlHat()
        {
            try
            {
                string connectionString = ToolAPI.INIOperate.IniReadValue("netSqlGroup", "connectionString", MainStatic.Path);

                string[] connectionStringAry = connectionString.Split(';');
                foreach (string connectionStringTemp in connectionStringAry)
                {
                    string[] dbnetAry = connectionStringTemp.Split('&');
                    DbHelperSQL dbNet = new DbHelperSQL(string.Format("Data Source={0};Port={1};Database={2};User={3};Password={4}", dbnetAry[0], dbnetAry[1], dbnetAry[2], dbnetAry[3], dbnetAry[4]), DbProviderType.MySql);
                    //dbNetFace = new DbHelperSQL(string.Format("Data Source={0};】={1};Database={2};User={3};Password={4}", dbnetAry[0], dbnetAry[1], dbnetAry[2], dbnetAry[3], dbnetAry[4]), DbProviderType.MySql);
                    dbNetFace = new DbHelperSQL(string.Format("Data Source={0};Port ={1};Database={2};User={3};Password={4}", dbnetAry[0], dbnetAry[1], dbnetAry[2], dbnetAry[3], dbnetAry[4]), DbProviderType.MySql);
                    DbNetAndSn.Add(dbNet, "");
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqHat异常", ex.Message);
            }
            
        }
        #region 存入本地数据库用的
        public static int SaveSoftHat(DBFrame df)
       {
           try
           {
               string sql = string.Format("INSERT INTO softHat (deviceid,datatype,contentjson,contenthex,version) VALUES('{0}','{1}','{2}','{3}','{4}')", df.deviceid, df.datatype, df.contentjson, df.contenthex, df.version);
               int result = DBoperateClass.DBoperateObj.ExecuteNonQuery(sql, null, CommandType.Text);
                return result;
           }
           catch (Exception ex)
           {
               ToolAPI.XMLOperation.WriteLogXmlNoTail("SaveSoftHat异常", ex.Message);
               return 0;
           }
       }
        #endregion



        #region IP相关
        public static DataTable GetIpCongfig()
        {
            try
            {
                DataTable dt = new DataTable();
                if (DbNetAndSn.Keys.Count > 0)
                {
                    DbHelperSQL DbNet = DbNetAndSn.Keys.ToList().First();
                    {
                        string sql = " select equipmentNo,ip_dn,port from equipment_softhat_period_orderissued where addr_status='0'";
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
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlLift.GetIpCongfig异常", ex.Message);
                return null;
            }
        }


        /// <summary>
        /// 更改ip后的回答
        /// </summary>
        /// <param name="SoftHatNo"></param>
        /// <param name="status"></param>
        /// <param name="issuccess"></param>
        public static void UpdateDataConfig(string SoftHatNo, int status, bool issuccess)
        {
            try
            {
                DbHelperSQL DBNet = DbNetAndSn.Keys.ToList().First();
                if (DBNet != null)
                {
                    string sql = "";
                    if (issuccess)
                    {
                        sql = string.Format("select ip_dn,port from equipment_softhat_period_orderissued where equipmentNo='{0}'", SoftHatNo);
                        DataTable dt = DBNet.ExecuteDataTable(sql, null, CommandType.Text);
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            sql = string.Format("update equipment_softhat_period_orderissued set addr_status='{0}',addr_time=(select UNIX_TIMESTAMP(now())),ip_dn_backup='{1}',port_backup={2} where equipmentNo='{3}'", status, dt.Rows[0]["ip_dn"].ToString(), Convert.ToInt32(dt.Rows[0]["port"]), SoftHatNo);
                            DBNet.ExecuteNonQuery(sql, null, CommandType.Text);
                        }
                    }
                    else
                    {
                        sql = string.Format("update equipment_softhat_period_orderissued set addr_status='{0}',addr_time=(select UNIX_TIMESTAMP(now())) where equipmentNo='{1}'", status, SoftHatNo);
                        DBNet.ExecuteNonQuery(sql, null, CommandType.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("UpdateDataCongfig异常", ex.Message);
            }
        }

        #endregion

        #region 上传周期
        /// <summary>
        /// 获取需要设置上传周期的数据
        /// </summary>
        /// <returns></returns>
        public static DataTable GetUploadPeriodCongfig()
        {
            try
            {
                DataTable dt = new DataTable();
                if (DbNetAndSn.Keys.Count > 0)
                {
                    DbHelperSQL DbNet = DbNetAndSn.Keys.ToList().First();
                    {
                        string sql = " select equipmentNo,period from equipment_softhat_period_orderissued where period_status='0'";
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
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlLift.GetIpCongfig异常", ex.Message);
                return null;
            }
        }


        /// <summary>
        /// 更改上传周期后的回答
        /// </summary>
        /// <param name="SoftHatNo"></param>
        /// <param name="status"></param>
        /// <param name="issuccess"></param>
        public static void UpdatePeriodDataConfig(string SoftHatNo, int status, bool issuccess)
        {
            try
            {
                DbHelperSQL DBNet = DbNetAndSn.Keys.ToList().First();
                if (DBNet != null)
                {
                    string sql = "";
                    if (issuccess)
                    {
                        sql = string.Format("select period from equipment_softhat_period_orderissued where equipmentNo='{0}'", SoftHatNo);
                        DataTable dt = DBNet.ExecuteDataTable(sql, null, CommandType.Text);
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            sql = string.Format("update equipment_softhat_period_orderissued set period_status='{0}',addr_time=(select UNIX_TIMESTAMP(now())),period_backup='{1}' where equipmentNo='{2}'", status, dt.Rows[0]["period"].ToString(), SoftHatNo);
                            DBNet.ExecuteNonQuery(sql, null, CommandType.Text);
                        }
                    }
                    else
                    {
                        sql = string.Format("update equipment_softhat_period_orderissued set period_status='{0}',addr_time=(select UNIX_TIMESTAMP(now())) where equipmentNo='{1}'", status, SoftHatNo);
                        DBNet.ExecuteNonQuery(sql, null, CommandType.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("UpdatePeriodDataConfig异常", ex.Message);
            }
        }

        #endregion


        /// <summary>
        /// 获取该条包的IP下发状态
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int GetIssueStatus(string equipmentNo, string type)
        {
            string sql = "select period_status,addr_status from equipment_softhat_period_orderissued where equipmentNo='" + equipmentNo + "' limit 1";
            DataTable dt = dbNetFace.ExecuteDataTable(sql, null, CommandType.Text);

            int statu = 0;
            if (type == "IP")
            {
                statu = int.Parse(dt.Rows[0]["addr_status"].ToString());
            }
            else if (type == "period")
            {
                statu = int.Parse(dt.Rows[0]["period_status"].ToString());
            }
            return statu;
        }
    }
}
