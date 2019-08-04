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

namespace ProtocolAnalysis.Smoke
{
  public class DB_MysqlSmoke
  {
      public static int SaveSmoke(DBFrame df)
      {
          try
          {
              string sql = string.Format("INSERT INTO smoke (deviceid,datatype,contentjson,contenthex,version) VALUES('{0}','{1}','{2}','{3}','{4}')", df.deviceid, df.datatype, df.contentjson, df.contenthex, df.version);
              int result = DBoperateClass.DBoperateObj.ExecuteNonQuery(sql, null, CommandType.Text);
              return result;
          }
          catch (Exception ex)
          {
              ToolAPI.XMLOperation.WriteLogXmlNoTail("SaveSmoke异常", ex.Message);
              return 0;
          }
      }
    }
}
