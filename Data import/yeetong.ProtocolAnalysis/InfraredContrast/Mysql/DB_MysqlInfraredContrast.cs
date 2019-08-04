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

namespace ProtocolAnalysis.InfraredContrast.Mysql
{
    public class DB_MysqlInfraredContrast
    {
       #region 存入本地数据库用的
        public static int SaveInfraredContrast(DBFrame df)
       {
           try
           {
               string sql = string.Format("INSERT INTO infraredcontrast (deviceid,datatype,contentjson,contenthex,version) VALUES('{0}','{1}','{2}','{3}','{4}')", df.deviceid, df.datatype, df.contentjson, df.contenthex, df.version);
               int result = DBoperateClass.DBoperateObj.ExecuteNonQuery(sql, null, CommandType.Text);
               return result;
           }
           catch (Exception ex)
           {
               ToolAPI.XMLOperation.WriteLogXmlNoTail("InfraredContrast异常", ex.Message);
               return 0;
           }
       }
       #endregion
    }
}
