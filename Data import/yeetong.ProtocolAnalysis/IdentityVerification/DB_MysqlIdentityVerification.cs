using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Architecture;
using ProtocolAnalysis.IdentityVerification.model;
using ProtocolAnalysis.TowerCrane._021303;
using ProtocolAnalysis.TowerCrane.OE;
using SIXH.DBUtility;
using ToolAPI;

namespace ProtocolAnalysis.IdentityVerification
{
    public class DB_MysqlIdentityVerification
    {
        static DbHelperSQL dbNet = null;
        static DB_MysqlIdentityVerification()
        {
            try
            {
                string connectionString = ToolAPI.INIOperate.IniReadValue("netSqlGroup", "connectionString", MainStatic.Path);
                string[] connectionStringAry = connectionString.Split(';');
                foreach (string connectionStringTemp in connectionStringAry)
                {
                    string[] dbnetAry = connectionStringTemp.Split('&');
                    dbNet = new DbHelperSQL(string.Format("Data Source={0};Port={1};Database={2};User={3};Password={4}", dbnetAry[0], dbnetAry[1], dbnetAry[2], dbnetAry[3], dbnetAry[4]), DbProviderType.MySql);
                    ToolAPI.XMLOperation.WriteLogXmlNoTail("dbNet", connectionStringTemp);
                    break;
                }

            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlTowerCrane异常", ex.Message);
            }
        }

        #region 心跳
        public static int SaveHeartbeat(Heartbeat_IdentityVerification hi)
        {
            try
            {
                if (dbNet != null)
                {
                    // DataTable dt = dbNet.ExecuteDataTable("select * from equipment_basics", null, CommandType.Text);


                    IList<DbParameter> paraList = new List<DbParameter>();
                    paraList.Add(dbNet.CreateDbParameter("@p_Equipment", hi.Equipment));
                    paraList.Add(dbNet.CreateDbParameter("@p_RTC", hi.RTC));
                    paraList.Add(dbNet.CreateDbParameter("@p_Identity_card", hi.Identity_card));
                    int y = dbNet.ExecuteNonQuery("pro_iv_heartbeat", paraList, CommandType.StoredProcedure);
                    return y;
                }
                return 0;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("SaveHeartbeat异常", ex.Message);
                return 0;
            }
        }
        #endregion
        #region 保存实时数据
        /// <summary>
        /// 保存实时数据
        /// </summary>
        /// <param name="ci"></param>
        /// <returns></returns>
        public static int SaveCurrent(Current_IdentityVerification ci)
        {
            //IN `p_equipment` varchar(16),IN `p_Rtc` datetime,IN `p_ID` varchar(18),IN `p_InterId` int,IN `p_Nc` int,IN `p_SensorSet` varchar(10),IN `p_Height` varchar(10)
            try
            {
                if (dbNet != null)
                {
                    IList<DbParameter> paraList = new List<DbParameter>();
                    paraList.Add(dbNet.CreateDbParameter("@p_equipment", ci.Equipment));
                    paraList.Add(dbNet.CreateDbParameter("@p_Rtc", ci.RTC));
                    paraList.Add(dbNet.CreateDbParameter("@p_ID", ci.ID));
                    paraList.Add(dbNet.CreateDbParameter("@p_InterId", ci.InterId));
                    paraList.Add(dbNet.CreateDbParameter("@p_Nc", ci.Nc));
                    paraList.Add(dbNet.CreateDbParameter("@p_SensorSet", ci.SensorSet));
                    paraList.Add(dbNet.CreateDbParameter("@p_Height", ci.Height));
                    int y = dbNet.ExecuteNonQuery("pro_iv_Current", paraList, CommandType.StoredProcedure);
                    return y;
                }
                return 0;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("SaveCurrent异常", ex.Message);
                return 0;
            }
        }
        #endregion
        #region 保存信息上传的数据
        /// <summary>
        /// 保存信息上传的数据
        /// </summary>
        /// <param name="infor"></param>
        /// <returns></returns>
        public static int SaveInformation(information_IdentityVerification infor)
        {//IN `p_equipment` varchar(20),IN `p_RTC` datetime,IN `p_SoftVersion` varchar(50),IN `p_IdentifyWay` int,IN `p_IdentifyClcle` int,IN `p_HeightMin` varchar(20),IN `p_HeightMax` varchar(20),IN `p_HeightDistance` varchar(20)
            try
            {
                if (dbNet != null)
                {
                    IList<DbParameter> paraList = new List<DbParameter>();
                    paraList.Add(dbNet.CreateDbParameter("@p_equipment", infor.Equipment));
                    paraList.Add(dbNet.CreateDbParameter("@p_Rtc", infor.RTC));
                    paraList.Add(dbNet.CreateDbParameter("@p_SoftVersion", infor.SoftVersion));
                    paraList.Add(dbNet.CreateDbParameter("@p_IdentifyWay", infor.IdentifyWay));
                    paraList.Add(dbNet.CreateDbParameter("@p_IdentifyClcle", infor.IdentifyClcle));
                    paraList.Add(dbNet.CreateDbParameter("@p_HeightMin", infor.HeightMin));
                    paraList.Add(dbNet.CreateDbParameter("@p_HeightMax", infor.HeightMax));
                    paraList.Add(dbNet.CreateDbParameter("@p_HeightDistance", infor.HeightDistance));
                    paraList.Add(dbNet.CreateDbParameter("@p_dataType", infor.dataType));
                    int y = dbNet.ExecuteNonQuery("pro_iv_Information", paraList, CommandType.StoredProcedure);
                    return y;
                }
                return 0;
            }
            catch(Exception ex) {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("SaveInformation异常", ex.Message);
                return 0;
            }
        }
        #endregion
        #region 身份验证
        public static int SaveIdentityVerification(IdentityVerificationC hi)
        {
            try
            {
                if (dbNet != null)
                {
                    //IN `Equipment` varchar(20),IN `Order_flag` varchar(20),IN `RTC` datetime,IN `Identity_card` varchar(20),IN `Status` varchar(20),IN `Result` varchar(20),IN `Package_current` varchar(20),IN `Characteristic_type` varchar(20),IN `Name` varchar(20),,IN `Characteristic_data` text,IN `Creat_time` datetime
                    IList<DbParameter> paraList = new List<DbParameter>();
                    paraList.Add(dbNet.CreateDbParameter("@p_Equipment", hi.Equipment));
                    paraList.Add(dbNet.CreateDbParameter("@p_Order_flag", hi.Order_flag));
                    paraList.Add(dbNet.CreateDbParameter("@p_RTC", hi.RTC));
                    paraList.Add(dbNet.CreateDbParameter("@p_Identity_card", hi.Identity_card));
                    paraList.Add(dbNet.CreateDbParameter("@p_InOrOutStatus", hi.Status));
                    string flag = "";
                    if (hi.Result == "0") flag = "3";
                    else flag = "2";
                    paraList.Add(dbNet.CreateDbParameter("@p_Result", flag));
                    paraList.Add(dbNet.CreateDbParameter("@p_Package_current", hi.Package_current));
                    paraList.Add(dbNet.CreateDbParameter("@p_Characteristic_type", hi.Characteristic_type));
                    paraList.Add(dbNet.CreateDbParameter("@p_Iv_Name", hi.Name));
                    paraList.Add(dbNet.CreateDbParameter("@p_Characteristic_data", ConvertData.ToHexString(hi.Characteristic_data, 0, hi.Characteristic_data.Length)));
                    paraList.Add(dbNet.CreateDbParameter("@p_Creat_time", hi.Creat_time));
                    int y = dbNet.ExecuteNonQuery("pro_iv_Identity_Verification", paraList, CommandType.StoredProcedure);
                    return y;
                }
                return 0;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("SaveIdentityVerification异常", ex.Message);
                return 0;
            }
        }
        #endregion
        #region 下发虹膜
        public static DataTable GetIris()
        {
            try
            {
                if (dbNet != null)
                {
                    string sql = string.Format("select ep.equipment,ep.issued_status,p.iv_name,p.identity_card,p.characteristic_data0,p.characteristic_data1,p.characteristic_data2 from equipment_iv_boundperson as ep left join equipment_iv_person as p on ep.identity_card = p.identity_card where ep.issued_status='0'");
                    DataTable result = dbNet.ExecuteDataTable(sql, null, CommandType.Text);
                    return result;
                }
                return null;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("GetOrder异常", ex.Message);
                return null;
            }
        }
        #endregion
        #region 更新下发状态为1
        public static int UpdateIris(string equipment, string identity_card, string flag)
        {
            try
            {
                if (dbNet != null)
                {
                    string sql = string.Format("update equipment_iv_boundperson set issued_status='{2}' where equipment='{0}' and identity_card='{1}'", equipment, identity_card, flag);
                    int result = dbNet.ExecuteNonQuery(sql, null, CommandType.Text);
                    return result;
                }
                return 0;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("UpdateOrder异常", ex.Message);
                return 0;
            }
        }
        #endregion
        #region 删除虹膜
        public static DataTable GetIrisdelete()
        {
            try
            {
                if (dbNet != null)
                {
                    string sql = string.Format("select equipment,identity_card from equipment_iv_boundperson where is_delete='0'");
                    DataTable result = dbNet.ExecuteDataTable(sql, null, CommandType.Text);
                    return result;
                }
                return null;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("GetIrisdelete异常", ex.Message);
                return null;
            }
        }
        /// <summary>
        /// 根据设备号获取实时数据的时间
        /// </summary>
        /// <param name="sn"></param>
        /// <returns></returns>
        public static string GetCurrentToSn(string sn)
        {
            string result = "";
            try
            {
                if (dbNet != null)
                {
                    string sql = "select RTC from equipment_iv_current where SendCount=0 and NOW()> DATE_ADD(RTC,INTERVAL 1 MINUTE) and equipment='" + sn + "' LIMIT 1";
                    DataTable dt = dbNet.ExecuteDataTable(sql, null, CommandType.Text);
                    if (dt != null)
                        if (dt.Rows.Count > 0)
                            result = dt.Rows[0]["RTC"].ToString();
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("GetCurrentToSn异常", ex.Message);
            }
            return result;
        }
        #endregion
        #region 更新下发状态为1
        public static int UpdateIrisdelete(string equipment, string identity_card, string flag)
        {
            try
            {
                if (dbNet != null)
                {
                    string sql = string.Format("update equipment_iv_boundperson set is_delete='{2}' where equipment='{0}' and identity_card='{1}'", equipment, identity_card, flag);
                    int result = dbNet.ExecuteNonQuery(sql, null, CommandType.Text);
                    return result;
                }
                return 0;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("UpdateIrisdelete异常", ex.Message);
                return 0;
            }
        }
        #endregion
        /// <summary>
        /// 记录向设备发送应答
        /// </summary>
        /// <param name="equipment"></param>
        /// <returns></returns>
        public static int UpdateIsSendAnswer(string equipment)
        {
            try
            {
                if (dbNet != null)
                {
                    string sql = " update equipment_iv_current set SendCount=1 where equipment='" + equipment + "'";
                    return dbNet.ExecuteNonQuery(sql, null, CommandType.Text);
                }
                return 0;
            }
            catch(Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("UpdateIsSendAnswer异常", ex.Message);
                return 0;
            }
        }
        #region 命令下发的应答
        public static int SaveOrder(Orderissued_IdentityVerification hi)
        {
            try
            {
                if (dbNet != null)
                {
                    IList<DbParameter> paraList = new List<DbParameter>();
                    paraList.Add(dbNet.CreateDbParameter("@p_Equipment", hi.Equipment));
                    string flag = "";
                    if (hi.Confirm_flag == "0") flag = "2";
                    else if (hi.Confirm_flag == "1") flag = "2";
                    else flag = "3";
                    paraList.Add(dbNet.CreateDbParameter("@p_Flag", flag));
                    int y = dbNet.ExecuteNonQuery("pro_iv_order_confirm", paraList, CommandType.StoredProcedure);
                    return y;
                }
                return 0;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("SaveOrder异常", ex.Message);
                return 0;
            }
        }
        #endregion
        #region 获取命令下发
        public static DataTable GetOrder()
        {
            try
            {
                if (dbNet != null)
                {
                    string sql = string.Format("select equipment,equipment_addr,equipment_port,addr_confirm_flag,addr_status from equipment_iv_parameter where addr_status='0'");
                    DataTable result = dbNet.ExecuteDataTable(sql, null, CommandType.Text);
                    return result;
                }
                return null;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("GetOrder异常", ex.Message);
                return null;
            }
        }
        #endregion
        #region 更新下发状态为1
        public static int UpdateOrder(string equipment, string flag)
        {
            try
            {
                if (dbNet != null)
                {
                    string sql = string.Format("update equipment_iv_parameter set addr_status='{1}' where equipment='{0}'", equipment, flag);
                    int result = dbNet.ExecuteNonQuery(sql, null, CommandType.Text);
                    return result;
                }
                return 0;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("UpdateOrder异常", ex.Message);
                return 0;
            }
        }
        #endregion
        /// <summary>
        /// 获取人员姓名
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetNameToId(string id)
        {
            string result = "";
            try
            {
                if (dbNet != null)
                {
                    string sql = "select iv_name from equipment_iv_person where identity_card='" + id + "'";
                    DataTable dt = dbNet.ExecuteDataTable(sql, null, CommandType.Text);
                    if (dt.Rows.Count > 0)
                    {
                        result = dt.Rows[0]["iv_name"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("GetNameToId异常", ex.Message);
                return "";
            }
            return result;
        }
    }
}
