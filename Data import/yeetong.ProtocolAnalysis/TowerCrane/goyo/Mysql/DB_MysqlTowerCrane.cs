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
namespace ProtocolAnalysis.TowerCrane
{
    public class DB_MysqlTowerCrane
    {
        static  DbHelperSQL DBNet = null;
        static Dictionary<DbHelperSQL, string> DbNetAndSn = new Dictionary<DbHelperSQL, string>();
        static DB_MysqlTowerCrane()
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
                    DBNet = dbNet;
                }
                DbNetAndSnInit();
                Thread UpdateDbNetAndSnT = new Thread(UpdateDbNetAndSn) { IsBackground = true };
                UpdateDbNetAndSnT.Start();
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlTowerCrane异常", ex.Message);
            }
        }

        #region 存入本地数据库用的
        public static int SaveTowerCrane(DBFrame df)
        {
            try
            {
                string sql = string.Format("INSERT INTO towerCrane (deviceid,datatype,contentjson,contenthex,version) VALUES('{0}','{1}','{2}','{3}','{4}')", df.deviceid, df.datatype, df.contentjson, df.contenthex, df.version);
                int result = DBoperateClass.DBoperateObj.ExecuteNonQuery(sql, null, CommandType.Text);
                return result;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("SaveTowerCrane异常", ex.Message);
                return 0;
            }
        }
        #endregion

        #region 关于命令下发和身份验证   交互性频繁，直接就是与网络数据库进行交互
        #region 0E
        //是否存在识别卡
        public static byte IsExistCard(string Sn, string cardid)
        {
            try
            {
                DbHelperSQL DbNet = GetDbHelperSQL(Sn);
                if (DbNet != null)
                {
                    string sql = string.Format("select COUNT(1) as  numBer from p_person p INNER JOIN p_iccard  c ON p.empNo=c.empNo  and c.cardNo='{0}'", cardid);
                    DataTable o = DbNet.ExecuteDataTable(sql, null);

                    return (byte)int.Parse(o.Rows[0][0].ToString());
                }
                return 0;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("IsExistCard异常", ex.Message);
                return 0;
            }
        }
        //更改司机卡记录删除状态
        public static void UpdateIdentifyCurrent(string CraneNo, string IcCode)
        {
            try
            {
                DbHelperSQL DbNet = GetDbHelperSQL(CraneNo);
                if (DbNet != null)
                {
                    string sql = "update p_iccard_crane set isDel='2',DelTime='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'  where devNo='" + CraneNo + "'";
                    DbNet.ExecuteNonQuery(sql, null, CommandType.Text);
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("UpdateIdentifyCurrent异常", ex.Message);
            }
        }
        //考勤记录
        public static int Pro_Authentication(Authentication o)
        {
            try
            {
                DbHelperSQL DbNet = GetDbHelperSQL(o.SN);
                if (DbNet != null)
                {
                    if (o.isFace)
                    {
                        IList<DbParameter> paraList = new List<DbParameter>();
                        paraList.Add(DbNet.CreateDbParameter("@device", o.SN));
                        paraList.Add(DbNet.CreateDbParameter("@cardNo", o.empNo));
                        paraList.Add(DbNet.CreateDbParameter("@upTime", DateTime.Now));
                        paraList.Add(DbNet.CreateDbParameter("@inOrOut", o.Status));//考勤上班、下班
                        return DbNet.ExecuteNonQuery("pro_insertrecord_face", paraList, CommandType.StoredProcedure);
                    }
                    else
                    {
                        IList<DbParameter> paraList = new List<DbParameter>();
                        paraList.Add(DbNet.CreateDbParameter("@device", o.SN));
                        paraList.Add(DbNet.CreateDbParameter("@cardNo", o.KardID));
                        paraList.Add(DbNet.CreateDbParameter("@upTime", o.OnlineTime));
                        paraList.Add(DbNet.CreateDbParameter("@inOrOut", o.Status));//考勤上班、下班
                        return DbNet.ExecuteNonQuery("pro_insertrecord", paraList, CommandType.StoredProcedure);
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("Pro_Authentication异常", ex.Message);
                return 0;
            }
        }
        //通过身份证获取相关信息
        public static DataTable GetDriverInfoByIDCard(string sn, string card)
        {
            try
            {
                DbHelperSQL DbNet = GetDbHelperSQL(sn);
                if (DbNet != null)
                {
                    string sql = string.Format("select empNo,empNo as cardNo,`code`,telephone as tel,`name`,'塔吊司机卡' as cardType  from p_person where  `code`='{0}'", card);
                    DataTable dt = DbNet.ExecuteDataTable(sql, null, CommandType.Text);
                    if (dt == null)
                    {
                        return null;
                    }
                    return dt;
                }
                return null;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("GetDriverInfoByIDCard异常", ex.Message);
                return null;
            }
        }
        //更改ip后的应答
        /// <summary>
        /// 
        /// </summary>
        /// <param name="CraneNo"></param>
        /// <param name="status"></param>
        /// <param name="issuccess"> 是否下发成功</param>
        public static void UpdateDataCongfig(string CraneNo,int status,bool issuccess)
        {
            try
            {
                if (DBNet != null)
                {
                    string sql = "";
                    if (issuccess)
                    {
                        sql = string.Format("select ip_dn,port from equipment_towercrane_orderissued where equipmentNo='{0}'", CraneNo);
                        DataTable dt = DBNet.ExecuteDataTable(sql, null, CommandType.Text);
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            sql = string.Format("update equipment_towercrane_orderissued set addr_status='{0}',addr_time=(select UNIX_TIMESTAMP(now())),ip_dn_backup='{1}',port_backup={2} where equipmentNo='{3}'", status, dt.Rows[0]["ip_dn"].ToString(), Convert.ToInt32(dt.Rows[0]["port"]), CraneNo);
                            DBNet.ExecuteNonQuery(sql, null, CommandType.Text);
                        }
                    }
                    else
                    {
                        sql = string.Format("update equipment_towercrane_orderissued set addr_status='{0}',addr_time=(select UNIX_TIMESTAMP(now())) where equipmentNo='{1}'", status,CraneNo);
                        DBNet.ExecuteNonQuery(sql, null, CommandType.Text);
                    }
                   
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("UpdateDataCongfig异常", ex.Message);
            }
        }
        /// <summary>
        /// 更新ip命令下发记录表的状态
        /// </summary>
        /// <param name="equipmentNo"></param>
        /// <param name="status"></param>
        public static void UpdateIPCommandIssued(string equipmentNo,int status)
        {
            try
            {
                if (DBNet != null)
                {
                    string sql = string.Format("select creat_time from equipment_towercrane_orderissued_record where equipmentNo='{0}' order by creat_time desc limit 1",equipmentNo);
                    DataTable dt = DBNet.ExecuteDataTable(sql, null, CommandType.Text);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        sql = string.Format("update equipment_towercrane_orderissued_record set addr_status='{0}' where equipmentNo='{1}' and creat_time={2}", status, equipmentNo,Convert.ToInt32(dt.Rows[0]["creat_time"]));
                        DBNet.ExecuteNonQuery(sql, null, CommandType.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("UpdateIPCommandIssued异常", ex.Message);
            }
        }
        //更改控制配置状态 应答用
        public static void UpdateControlCongfig(string CraneNo)
        {
            try
            {
                DbHelperSQL DbNet = GetDbHelperSQL(CraneNo);
                if (DbNet != null)
                {
                    string sql = " update  equipment_towercrane_orderissued set limit_status='1' ,limit_time=(select UNIX_TIMESTAMP(now())) where equipmentNo='" + CraneNo + "' ";
                    DbNet.ExecuteNonQuery(sql, null, CommandType.Text);
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("UpdateControlCongfig异常", ex.Message);
            }
        }
        // 获取IP端口配置信息（2014-7-15）
        public static DataTable GetDataCongfig()
        {
            try
            {
                DataTable dt = new DataTable();
                DbHelperSQL DbNet = DbNetAndSn.Keys.ToList().First();
                if (DbNet != null)
                {
                    string sql = " select equipmentNo,ip_dn,port from equipment_towercrane_orderissued where addr_status='0'";
                    DataTable dttemp = DbNet.ExecuteDataTable(sql, null, CommandType.Text);
                    if (dttemp != null && dttemp.Rows.Count > 0)
                    {
                        dt.Merge(dttemp);
                    }
                }
                return dt;
            }
            catch (Exception ex)
            {
                //ToolAPI.XMLOperation.WriteLogXmlNoTail("GetDataCongfig异常", ex.Message);
                return null;
            }
        }
        // 获取控制配置信息（2014-7-15）
        public static DataTable GetControlCongfig()
        {
            try
            {
                DataTable dt = new DataTable();

                DbHelperSQL DbNet = DbNetAndSn.Keys.ToList().First();
                if (DbNet != null)
                {
                    string sql = " select equipmentNo,limit_value from equipment_towercrane_orderissued where limit_status='0'";
                    DataTable dttemp = DbNet.ExecuteDataTable(sql, null, CommandType.Text);
                    if (dttemp != null && dttemp.Rows.Count > 0)
                    {
                        dt.Merge(dttemp);
                    }
                }
                
                return dt;
            }
            catch (Exception ex)
            {
                //ToolAPI.XMLOperation.WriteLogXmlNoTail("GetControlCongfig异常", ex.Message);
                return null;
            }
        }
        #endregion

        #region 021303
        #region 身份验证相关
        // 获取司机相关信息  通过工号 人脸得时候
        public static DataTable GetDriverInfoByEmpNo(string sn, string empNo)
        {
            try
            {

                DbHelperSQL DbNet = DbNetAndSn.Keys.ToList().First();
                if (DbNet != null)
                {
                    string sql = string.Format("select *  from personnel_card where empNo='{0}'", empNo);
                    DataTable dt = DbNet.ExecuteDataTable(sql, null, CommandType.Text);
                    if (dt == null)
                    {
                        return null;
                    }
                    return dt;
                }
                return null;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("GetDriverInfoByEmpNo异常", ex.Message);
                return null;
            }
        }
        // 获取司机相关信息  通过身份证 人脸得时候
        public static DataTable GetDriverInfoByIDCARD(string sn, string IDCARD)
        {
            try
            {
                DbHelperSQL DbNet = DbNetAndSn.Keys.ToList().First();
                if (DbNet != null)
                {
                    string sql = string.Format("select *  from p_person_driverface where code='{0}'", IDCARD);
                    DataTable dt = DbNet.ExecuteDataTable(sql, null, CommandType.Text);
                    if (dt == null)
                    {
                        return null;
                    }
                    return dt;
                }
                return null;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("GetDriverInfoByIDCARD异常", ex.Message);
                return null;
            }
        }
        // 获取司机相关信息 通过卡号
        public static DataTable GetIdentifyInfo(string sn, string card)
        {
            try
            {
                DbHelperSQL DbNet = DbNetAndSn.Keys.ToList().First();
                if (DbNet != null)
                {
                    string sql = string.Format("select p.`name`,p.`code`,c.cardNo,c.telephone,job,c.empNo  from p_person p INNER JOIN p_iccard  c ON p.empNo=c.empNo  and c.cardNo='{0}'", card);
                    DataTable dt = DbNet.ExecuteDataTable(sql, null, CommandType.Text);
                    if (dt == null)
                    {
                        return null;
                    }
                    return dt;
                }
                return null;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("GetIdentifyInfo异常", ex.Message);
                return null;
            }
        }
        // 获取司机相关信息 通过工号
        public static DataTable GetIdentifyInfoByEmpNo(string sn, string empNo)
        {
            try
            {
                DbHelperSQL DbNet = DbNetAndSn.Keys.ToList().First();
                if (DbNet != null)
                {
                    string sql = string.Format("select *  from p_person empNo='{0}'", empNo);
                    DataTable dt = DbNet.ExecuteDataTable(sql, null, CommandType.Text);
                    if (dt == null)
                    {
                        return null;
                    }
                    return dt;
                }
                return null;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("GetIdentifyInfoByEmpNo异常", ex.Message);
                return null;
            }
        }
        // 考勤记录（只要有验证动作就做记录）
        public static int Pro_Authentication(AuthenticateV021303 o)
        {
            try
            {
                DbHelperSQL DbNet = DbNetAndSn.Keys.ToList().First();
                if (DbNet != null)
                {
                    string cardNo = "";
                    int state = 0;
                    if (o.Subcommand == 1)
                    {
                        cardNo = (o.SubcommandDistrict as AuthenticateV021303.OneSubcommandEP).DriverCardNo;
                        state = (o.SubcommandDistrict as AuthenticateV021303.OneSubcommandEP).State;
                    }
                    else
                    {
                        cardNo = (o.SubcommandDistrict as AuthenticateV021303.TwoAndFourSubcommandEP).DriverCardNo;
                        state = 1;
                    }
                    if (o.IdentificationType == 6)
                    {
                        IList<DbParameter> paraList = new List<DbParameter>();
                        paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@device", o.EquipmentID));
                        paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@cardNo", cardNo));
                        paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@upTime", DateTime.Now));
                        paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@inOrOut", state));
                        return DbNet.ExecuteNonQuery("pro_insertrecord_face", paraList, CommandType.StoredProcedure);
                    }
                    else
                    {
                        IList<DbParameter> paraList = new List<DbParameter>();
                        paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@device", o.EquipmentID));
                        paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@cardNo", cardNo));
                        paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@upTime", DateTime.Now));
                        paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@inOrOut", state));
                        return DbNet.ExecuteNonQuery("pro_insertrecord", paraList, CommandType.StoredProcedure);
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("Pro_Authentication异常", ex.Message);
                return 0;
            }
        }
        #endregion

        #region IP相关配置
        public static int SaveIPConfigure(IPConfigureV021303 o)
        {
            try
            {
                DbHelperSQL DbNet = DbNetAndSn.Keys.ToList().First();
                if (DbNet != null)
                {
                    string sql = " update  t_ipControl_tower_v2_13 set i_state='" + o.ResultStatus + "' ,i_updateTime='" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "'" +
                             " where craneNo='" + o.EquipmentID + "' ";
                    return DbNet.ExecuteNonQuery(sql, null, CommandType.Text);
                }
                return 0;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("SaveIPConfigure异常", ex.Message);
                return 0; }
        }
        #endregion

        #region 命令下发
        public static int SaveCommandIssued(CommandIssuedV021303 o)
        {
            try
            {
                DbHelperSQL DbNet = DbNetAndSn.Keys.ToList().First();
                if (DbNet != null)
                {
                    string flag = "2";
                    if (o.IdentificationMark == 0) flag = "2";
                    else if (o.IdentificationMark == 1) flag = "5";
                    string sql = " update  t_ipControl_tower_v2_13 set ct_state='" + flag + "' ,ct_updateTime='" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "'" +
                             " where craneNo='" + o.EquipmentID + "' ";
                    return DbNet.ExecuteNonQuery(sql, null, CommandType.Text);
                }
                return 0;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("SaveCommandIssued异常", ex.Message);
                return 0; }
        }
        #endregion

        #region 其它公用方法
        /// <summary>
        /// IP配置得获取
        /// </summary>
        /// <returns></returns>
        public static DataTable GetIPCongfig021303(bool isIPFrist)
        {
            try
            {
                DataTable dt = new DataTable();
                foreach (var item in DbNetAndSn)
                {
                    DbHelperSQL DbNet = item.Key;
                    if (DbNet != null)
                    {
                        string sql = "";
                        if (isIPFrist)
                        {
                            sql = " select craneNo,i_ip,i_port from t_ipControl_tower_v2_13 where i_state='0' or i_state='1'";
                            isIPFrist = false;
                        }
                        else sql = " select craneNo,i_ip,i_port from t_ipControl_tower_v2_13 where i_state='0'";
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
                //ToolAPI.XMLOperation.WriteLogXmlNoTail("GetIPCongfig021303异常", ex.Message);
                return null;
            }
        }
        /// <summary>
        /// ip重发确认
        /// </summary>
        /// <param name="DevNo">设备号</param>
        /// <returns></returns>
        public static int GetIPRepeatSend021303(string DevNo)
        {
            try
            {
                DbHelperSQL DbNet = GetDbHelperSQL(DevNo);
                if (DbNet != null)
                {
                    string sql = string.Format("select COUNT(id) from t_ipControl_tower_v2_13 where i_state='1' and craneNo='{0}'", DevNo);
                    object dt = DbNet.ExecuteScalar(sql, null, CommandType.Text);
                    return int.Parse(dt.ToString());
                }
                return 0;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("GetIPRepeatSend021303异常", ex.Message);
                return 0;
            }
        }

        /// IP配置获取的标志职位
        public static int SetIPStatus021303(string EquipmentID, string status)
        {
            try
            {
                DbHelperSQL DbNet = GetDbHelperSQL(EquipmentID);
                if (DbNet != null)
                {
                    string sql = " update  t_ipControl_tower_v2_13 set i_state='" + status + "' ,i_updateTime='" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "'" +
                              " where craneNo='" + EquipmentID + "' ";
                    return DbNet.ExecuteNonQuery(sql, null, CommandType.Text);
                }
                return 0;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("SetIPStatus021303异常", ex.Message);
                return 0;
            }
        }
        /// <summary>
        /// 命令得获取
        /// </summary>
        /// <returns></returns>
        public static DataTable GetCommandIssued021303(bool isComFrist)
        {
            try
            {
                DataTable dt = new DataTable();
                foreach (var item in DbNetAndSn)
                {
                    DbHelperSQL DbNet = item.Key;
                    if (DbNet != null)
                    {
                        string sql = "";
                        if (isComFrist)
                        {
                            sql = " select craneNo,ct_cmdValue,ct_paramConfig,ct_state from t_ipControl_tower_v2_13 where ct_state='0' or ct_state='1' or ct_state='3' or ct_state='4'";
                            isComFrist = false;
                        }
                        else sql = " select craneNo,ct_cmdValue,ct_paramConfig,ct_state from t_ipControl_tower_v2_13 where ct_state='0' or ct_state='3'";
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
                //ToolAPI.XMLOperation.WriteLogXmlNoTail("GetCommandIssued021303异常", ex.Message);
                return null;
            }
        }
        /// <summary>
        /// 控制命令重发确认
        /// </summary>
        /// <param name="DevNo">设备号</param>
        /// <returns></returns>
        public static int GetCommandIssuedRepeatSend021303(string DevNo)
        {
            try
            {
                DbHelperSQL DbNet = GetDbHelperSQL(DevNo);
                if (DbNet != null)
                {
                    string sql = string.Format("select COUNT(id) from t_ipControl_tower_v2_13 where  craneNo='{0}' and (ct_state='1' or ct_state='4') ", DevNo);
                    object dt = DbNet.ExecuteScalar(sql, null, CommandType.Text);
                    return int.Parse(dt.ToString());
                }
                return 0;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("GetCommandIssuedRepeatSend021303异常", ex.Message);
                return 0;
            }
        }
        /// 命令得获取标志职位
        public static int SetCommandIssuedStatus021303(string EquipmentID, string status)
        {
            try
            {
                DbHelperSQL DbNet = GetDbHelperSQL(EquipmentID);
                if (DbNet != null)
                {
                    string sql = " update  t_ipControl_tower_v2_13 set ct_state='" + status + "' ,ct_updateTime='" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + "'" +
                              " where craneNo='" + EquipmentID + "' ";
                    return DbNet.ExecuteNonQuery(sql, null, CommandType.Text);
                }
                return 0;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("SetCommandIssuedStatus021303异常", ex.Message);
                return 0;
            }
        }
        #endregion
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
                        paraList.Add(item.Key.CreateDbParameter("@ptype", "1"));
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

        #region 执行插入更新特种人员操作记录存储过程
        public static int SaveAndUpdataProOperation_record(string deviceNum, string code, long startTime, long endTIme, int state)
        {
            int flag = 0;
            try
            {

                Dictionary<DbHelperSQL, string> DbNetAndSnTemp = new Dictionary<DbHelperSQL, string>();
                foreach (var item in DbNetAndSn)
                {
                    if (item.Key != null)
                    {
                        DbHelperSQL DbNet = item.Key;
                        IList<DbParameter> paraList = new List<DbParameter>();
                        paraList.Add(item.Key.CreateDbParameter("@p_deviceNum", deviceNum));
                        paraList.Add(item.Key.CreateDbParameter("@p_personneCode", code));
                        paraList.Add(item.Key.CreateDbParameter("@p_startTime", startTime));
                        paraList.Add(item.Key.CreateDbParameter("@p_endTIme", endTIme));
                        paraList.Add(item.Key.CreateDbParameter("@p_state", state));
                        int y = DbNet.ExecuteNonQuery("pro_personnel_equipment_operation_record", paraList, CommandType.StoredProcedure);
                        flag = y;
                    }

                }
            }
            catch
            {
                flag = 0;
            }
            return flag;

        }
        #endregion
    }
}
