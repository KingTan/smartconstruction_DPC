using Architecture;
using SIXH.DBUtility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;

namespace ProtocolAnalysis
{
    public class DB_MysqlElectric
    {
        static Dictionary<DbHelperSQL, string> DbNetAndSn = new Dictionary<DbHelperSQL, string>();
        static DbHelperSQL dbNet = null;
        static DB_MysqlElectric()
        {
            try
            {
                string connectionString = ToolAPI.INIOperate.IniReadValue("netSqlGroup", "connectionString", MainStatic.Path);
                string[] connectionStringAry = connectionString.Split(';');
                foreach (string connectionStringTemp in connectionStringAry)
                {
                    string[] dbnetAry = connectionStringTemp.Split('&');
                     dbNet = new DbHelperSQL(string.Format("Data Source={0};Port={1};Database={2};User={3};Password={4}", dbnetAry[0], dbnetAry[1], dbnetAry[2], dbnetAry[3], dbnetAry[4]), DbProviderType.MySql);
                    DbNetAndSn.Add(dbNet, "");
                }
                // DbNetAndSnInit();
                //  Thread UpdateDbNetAndSnT = new Thread(UpdateDbNetAndSn) { IsBackground = true };
                //UpdateDbNetAndSnT.Start();
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlElectric异常", ex.Message);
            }
        }
        /// <summary>
        /// 保存实时数据
        /// </summary>
        /// <param name="df"></param>
        /// <returns></returns>
        public static int SaveElectricCurrent(DBFrame df)
        {
            try
            {
                string sql = string.Format("INSERT INTO electric (deviceid,datatype,contentjson,contenthex,version) VALUES('{0}','{1}','{2}','{3}','{4}')", df.deviceid, df.datatype, df.contentjson, df.contenthex, df.version);
                int result = DBoperateClass.DBoperateObj.ExecuteNonQuery(sql, null, CommandType.Text);

                return result;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("SaveElectricCurrent异常", ex.Message);
                return 0;
            }
        }
        /// <summary>
        /// 根据设备号更新电表的状态
        /// </summary>
        /// <param name="equipmentNo"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static int UpdateElectricStatus(string equipmentNo, string status)
        {
            string sql = "update equipment_electric_energy_meter_orderissued set openstate='" + status + "' where equipmentNo='" + equipmentNo + "'";
            return dbNet.ExecuteNonQuery(sql, null, CommandType.Text);
        }

        public static int UpdateElectricAnswer(string equipmentNo)
        {
            string sql = "update equipment_electric_energy_meter_orderissued set resposestate=1 where equipmentNo='" + equipmentNo + "'";
            return 1;
        }
        /// <summary>
        /// 根据设备号找到要执行的设备
        /// </summary>
        /// <param name="sn"></param>
        /// <returns></returns>
        public static DataTable GetsndataToSn(string sn)
        {
            string sql = "select equipmentNo from equipment_electric_energy_meter_orderissued where gatewaysn='" + sn + "'";
            return dbNet.ExecuteDataTable(sql, null, CommandType.Text);

        }
      
        /// <summary>
        /// 获取平台开关电表
        /// </summary>
        /// <param name="gatewaysn"></param>
        /// <returns></returns>
        public static DataTable GetOpenOrClose(string gatewaysn)
        {
            string sql = "select equipmentNo as sn,openstate from equipment_electric_energy_meter_orderissued where resposestate='0' and gatewaysn='" + gatewaysn + "'";
            return dbNet.ExecuteDataTable(sql, null, CommandType.Text);
        }
        /// <summary>
        /// 更新闸状态
        /// </summary>
        /// <param name="sn"></param>
        /// <returns></returns>
        public static int UpdateResponseStatus(string sn)
        {
            string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string sql = "update equipment_electric_energy_meter_orderissued set resposestate=1,update_time=UNIX_TIMESTAMP(NOW()) where equipmentNo='" + sn + "'";
            return dbNet.ExecuteNonQuery(sql, null, CommandType.Text);
        }
        #region 星期转换数字
        public string getstring(string dt)
        {
            string week = "1";
            switch (dt)
            {
                case "Monday":
                    week = "1";
                    break;
                case "Tuesday":
                    week = "2";
                    break;
                case "Wednesday":
                    week = "3";
                    break;
                case "Thursday":
                    week = "4";
                    break;
                case "Friday":
                    week = "5";
                    break;
                case "Saturday":
                    week = "6";
                    break;
                case "Sunday":
                    week = "7";
                    break;
            }
            return week;
        }
        #endregion
    }
}
