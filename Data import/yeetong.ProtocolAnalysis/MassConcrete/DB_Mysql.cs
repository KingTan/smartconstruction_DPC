using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Architecture;
using ProtocolAnalysis.TowerCrane._021303;
using ProtocolAnalysis.TowerCrane.OE;
using SIXH.DBUtility;
namespace ProtocolAnalysis.MassConcrete
{
    public class DB_MysqlMassConcrete
    {
        #region 存入本地数据库用的
        public static int SaveMassConcrete(DBFrame df)
        {
            try
            {
                string sql = string.Format("INSERT INTO massConcrete (deviceid,datatype,contentjson,contenthex,version) VALUES('{0}','{1}','{2}','{3}','{4}')", df.deviceid, df.datatype, df.contentjson, df.contenthex, df.version);
                int result = DBoperateClass.DBoperateObj.ExecuteNonQuery(sql, null, CommandType.Text);
                return result;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("SaveMassConcrete异常", ex.Message);
                return 0;
            }
        }


        #endregion

        static DbHelperSQL dbNetdefault = null;
        static DB_MysqlMassConcrete()
        {
            try
            {
                string connectionString = ToolAPI.INIOperate.IniReadValue("netSqlGroup", "connectionString", MainStatic.Path);

                string[] dbnetAr = connectionString.Split('&');
                dbNetdefault = new DbHelperSQL(string.Format("Data Source={0};Port={1};Database={2};User={3};Password={4}", dbnetAr[0], dbnetAr[1], dbnetAr[2], dbnetAr[3], dbnetAr[4]), DbProviderType.MySql);
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("MysqlMassConcrete_010002异常", ex.Message);
            }
        }
        public static DataTable GetMcControlCongfig()
        {
            try
            {
                DataTable dt = new DataTable();
                if (dbNetdefault != null)
                {
                    string sql = "select equipmentNo,returntime,alarmmax,alarmmin,spraytim,spraypl from equipment_massconcrete_parameter where updatestate=0";
                    DataTable dttemp = dbNetdefault.ExecuteDataTable(sql, null, CommandType.Text);
                    if (dttemp != null && dttemp.Rows.Count > 0)
                    {
                        dt.Merge(dttemp);
                    }
                }

                return dt;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("GetMcControlCongfig", ex.Message);
                return null;
            }
        }
        public static void UpdateMcControlCongfig(string sn)
        {
            try
            {
                if (dbNetdefault != null)
                {
                    string sql = "update  equipment_massconcrete_parameter set updatestate=1  where equipmentNo='" + sn + "' ";
                    dbNetdefault.ExecuteNonQuery(sql, null, CommandType.Text);
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("UpdateMcControlCongfig", ex.Message);
            }
        }

    }
}
