using Architecture;
using SIXH.DBUtility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace ProtocolAnalysis.BorderProtection
{
    public class DBMysqlBorderProtection
    {
        static DbHelperSQL dbNetdefault = null;
        static DBMysqlBorderProtection()
        {
            try
            {
                string connectionString = ToolAPI.INIOperate.IniReadValue("netSqlGroup", "connectionString", MainStatic.Path);
                string[] dbnetAr = connectionString.Split('&');
                dbNetdefault = new DbHelperSQL(string.Format("Data Source={0};Port={1};Database={2};User={3};Password={4}", dbnetAr[0], dbnetAr[1], dbnetAr[2], dbnetAr[3], dbnetAr[4]), DbProviderType.MySql);
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("MysqlFogGun_010002异常", ex.Message);
            }
        }

        #region 存入本地数据库用的
        public static int SaveBorderProtection(DBFrame df) 
        { 
            try
            {
                string sql = string.Format("INSERT INTO BorderProtection (deviceid,datatype,contentjson,contenthex,version) VALUES('{0}','{1}','{2}','{3}','{4}')", df.deviceid, df.datatype, df.contentjson, df.contenthex, df.version);
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
    }
}
