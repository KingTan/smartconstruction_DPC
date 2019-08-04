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
namespace ProtocolAnalysis.Lift
{
    public class DB_MysqlLift
    {
        static Dictionary<DbHelperSQL, string> DbNetAndSn = new Dictionary<DbHelperSQL, string>();
        static DbHelperSQL dbNetFace = null;
        static string liftloopOpenOrClose = "false";
        static DB_MysqlLift()
        {
            try
            {

                liftloopOpenOrClose = ToolAPI.INIOperate.IniReadValue("sqlMsg", "liftloop", MainStatic.Path);
                string connectionString = ToolAPI.INIOperate.IniReadValue("netSqlGroup", "connectionString", MainStatic.Path);

                string[] connectionStringAry = connectionString.Split(';');
                foreach (string connectionStringTemp in connectionStringAry)
                {
                    string[] dbnetAry = connectionStringTemp.Split('&');
                    DbHelperSQL dbNet = new DbHelperSQL(string.Format("Data Source={0};Port={1};Database={2};User={3};Password={4}", dbnetAry[0], dbnetAry[1], dbnetAry[2], dbnetAry[3], dbnetAry[4]), DbProviderType.MySql);
                    dbNetFace = new DbHelperSQL(string.Format("Data Source={0};Port={1};Database={2};User={3};Password={4}", dbnetAry[0], dbnetAry[1], dbnetAry[2], dbnetAry[3], dbnetAry[4]), DbProviderType.MySql);
                    DbNetAndSn.Add(dbNet, "");
                }
                DbNetAndSnInit();
                Thread UpdateDbNetAndSnT = new Thread(UpdateDbNetAndSn) { IsBackground = true };
                UpdateDbNetAndSnT.Start();
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlLift异常", ex.Message);
            }
        }

        #region 存入本地数据库用的
        public static int SaveLift(DBFrame df)
        {
            try
            {
                string sql = string.Format("INSERT INTO lift (deviceid,datatype,contentjson,contenthex,version) VALUES('{0}','{1}','{2}','{3}','{4}')", df.deviceid, df.datatype, df.contentjson, df.contenthex, df.version);
                int result = DBoperateClass.DBoperateObj.ExecuteNonQuery(sql, null, CommandType.Text);
                if (liftloopOpenOrClose.Equals("true") && df.datatype == "current")  //升降机工作循环开关
                {
                    SaveLiftLoop(df.deviceid, df.version, df.contentjson);  //升降机工作循环添加
                }
                return result;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("SaveLift异常", ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// 升降机工作循环插入
        /// </summary>
        /// <param name="deviceid"></param>
        /// <param name="version"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        static void SaveLiftLoop(string deviceid, string version, string json)
        {
            try
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(json);
                string height = jo["Height"].ToString();
                string speed = jo["Speed"].ToString();

                IList<DbParameter> paraList = new List<DbParameter>();
                paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@id", deviceid));
                paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@hi", height));
                paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@Ri", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@Si", speed));
                paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@Vi", version));
                DBoperateClass.DBoperateObj.ExecuteNonQuery("pro_Liftloop", paraList, CommandType.StoredProcedure);
            }
            catch { }
        }
        #endregion

        #region 关于命令下发和身份验证   交互性频繁，直接就是与网络数据库进行交互

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
                        string sql = " select equipmentNo,ip_dn,port from equipment_lift_orderissued where addr_status='0'";
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
        #endregion

        #region 身份验证
        /// <summary>
        /// 是否存在司机卡号
        /// </summary>
        /// <param name="cardid">司机卡号</param>
        /// <returns></returns>
        public static byte IsExistCard(Lift_Authentication lifto)
        {
            try
            {
                if (DbNetAndSn.Keys.Count > 0)
                {
                    DbHelperSQL DbNet = DbNetAndSn.Keys.ToList().First();
                    {
                        if (lifto.isFace)
                        {
                            byte result;
                            string sql = string.Format("select COUNT(1) as  numBer from p_person where  empNo='{0}'", lifto.empNo);
                            DataTable o = DbNet.ExecuteDataTable(sql, null, CommandType.Text);
                            if (int.Parse(o.Rows[0][0].ToString()) >= 1)
                                result = 1;
                            else
                                result = 0;
                            return result;
                        }
                        else
                        {
                            byte result;
                            string sql = string.Format("select COUNT(1) as  numBer from p_person p INNER JOIN p_iccard  c ON p.empNo=c.empNo  and c.cardNo='{0}'", lifto.cardNo);
                            DataTable o = DbNet.ExecuteDataTable(sql, null, CommandType.Text);
                            if (int.Parse(o.Rows[0][0].ToString()) >= 1)
                                result = 1;
                            else
                                result = 0;
                            return result;
                        }
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlLift.IsExistCard异常", ex.Message);
                return 0;
            }
        }
        /// <summary>
        /// 更改司机卡记录删除状态
        /// </summary>
        /// <param name="CraneNo"></param>
        public static void UpdateIdentifyCurrent(string CraneNo, string IcCode)
        {
            try
            {
                if (DbNetAndSn.Keys.Count > 0)
                {
                    DbHelperSQL DbNet = DbNetAndSn.Keys.ToList().First();
                    {
                        string sql = "update p_iccard_crane set isDel='2',DelTime='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'  where devNo='" + CraneNo + "'";
                        DbNet.ExecuteNonQuery(sql, null, CommandType.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlLift.UpdateIdentifyCurrent异常", ex.Message);
            }
        }
        /// <summary>
        /// 记录身份识别时间
        /// </summary>
        public static int Pro_Authentication(Lift_Authentication o)
        {
            try
            {
                if (DbNetAndSn.Keys.Count > 0)
                {
                    DbHelperSQL DbNet = DbNetAndSn.Keys.ToList().First();
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
                            paraList.Add(DbNet.CreateDbParameter("@upTime", DateTime.Now));
                            paraList.Add(DbNet.CreateDbParameter("@inOrOut", o.Status));//考勤上班、下班
                            return DbNet.ExecuteNonQuery("pro_insertrecord", paraList, CommandType.StoredProcedure);
                        }
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlLift.Pro_Authentication异常", ex.Message);
                return 0;
            }
        }
        /// <summary>
        /// 是否存在这个工号的司机
        /// 20161220
        /// </summary>
        /// <param name="cardid">司机工号</param>
        /// <returns></returns>
        public static byte IsExistEmpNo(string sn, string EmpNo)
        {
            try
            {
                if (DbNetAndSn.Keys.Count > 0)
                {
                    DbHelperSQL DbNet = DbNetAndSn.Keys.ToList().First();
                    {
                        byte result;
                        string sql = string.Format("select COUNT(1) as  numBer from p_person where  empNo='{0}'", EmpNo);
                        DataTable o = DbNet.ExecuteDataTable(sql, null, CommandType.Text);
                        if (int.Parse(o.Rows[0][0].ToString()) >= 1)
                            result = 1;
                        else
                            result = 0;
                        return result;
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlLift.IsExistEmpNo异常", ex.Message);
                return 0;
            }
        }
        /// <summary>
        /// 获取司机相关信息
        /// </summary>
        /// <param name="cord"></param>
        /// <returns></returns>
        public static DataTable GetIdentifyInfo(string sn, string card)
        {
            try
            {
                if (DbNetAndSn.Keys.Count > 0)
                {
                    DbHelperSQL DbNet = DbNetAndSn.Keys.ToList().First();
                    {
                        string sql = string.Format("select p.`name`,p.`code`,c.cardNo,c.telephone,job,c.empNo  from p_person p INNER JOIN p_iccard  c ON p.empNo=c.empNo  and c.cardNo='{0}'", card);
                        DataTable dt = DbNet.ExecuteDataTable(sql, null, CommandType.Text);
                        if (dt == null)
                        {
                            return null;
                        }
                        return dt;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlLift.GetIdentifyInfo异常", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 获取司机相关信息  通过身份证号
        /// </summary>
        /// <param name="cord"></param>
        /// <returns></returns>
        public static DataTable GetDriverInfoByIDCard(string sn, string card)
        {
            try
            {
                if (DbNetAndSn.Keys.Count > 0)
                {
                    DbHelperSQL DbNet = DbNetAndSn.Keys.ToList().First();
                    {
                        string sql = string.Format("select empNo,empNo as cardNo,`code`,telephone as tel,`name`,'升降机司机' as cardType  from p_person where  `code`='{0}'", card);
                        DataTable dt = DbNet.ExecuteDataTable(sql, null, CommandType.Text);
                        if (dt == null)
                        {
                            return null;
                        }
                        return dt;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlLift.GetDriverInfoByIDCard异常", ex.Message);
                return null;
            }
        }
        /// <summary>
        /// 获取司机相关信息  通过工号
        /// </summary>
        /// <param name="cord"></param>
        /// <returns></returns>
        public static DataTable GetDriverInfoByEmpNo(string sn, string empNo)
        {
            try
            {
                if (DbNetAndSn.Keys.Count > 0)
                {
                    DbHelperSQL DbNet = DbNetAndSn.Keys.ToList().First();
                    if (DbNet != null)
                    {
                        string sql = string.Format("select empNo,empNo as cardNo,`code`,telephone as tel,`name`,'升降机司机' as cardType  from personnel_real_name_system where empNo='{0}'", empNo);
                        DataTable dt = DbNet.ExecuteDataTable(sql, null, CommandType.Text);
                        if (dt == null)
                        {
                            return null;
                        }
                        return dt;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlLift.GetDriverInfoByEmpNo异常", ex.Message);
                return null;
            }
        }
        #endregion

        #region 限位控制
        /// <summary>
        /// 更改控制状态  2017 09 21
        /// </summary>
        /// <param name="sn"></param>
        public static void UpdatecontrolDataCongfig(string sn)
        {
            try
            {
                DbHelperSQL DbNet = GetDbHelperSQL(sn);
                if (DbNet != null)
                {
                    string sql = " update  equipment_lift_orderissued set limit_status='1' ,limit_time='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'" +
                             " where equipmentNo='" + sn + "' ";//lyf升级mysql
                    DbNet.ExecuteNonQuery(sql, null, CommandType.Text);
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlLift.UpdatecontrolDataCongfig异常", ex.Message);
            }
        }

        /// <summary>
        /// 获取对应的控制状态  2017 09 21
        /// </summary>
        /// <returns></returns>
        public static DataTable GetcontrolCongfig()
        {
            try
            {
                DataTable dt = new DataTable();
                foreach (var item in DbNetAndSn)
                {
                    DbHelperSQL DbNet = item.Key;
                    if (DbNet != null)
                    {
                        string sql = " select equipmentNo,limit_value,limit_status from equipment_lift_orderissued where limit_status='0'";//lyf升级mysql
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
                ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlLift.GetcontrolCongfig异常", ex.Message);
                return null;
            }
        }
        #endregion

        /// <summary>
        /// 保存设备上传上来的人脸特征库
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int SaveListFaceFeaturePack(LiftFeature data)
        {
            int result = 0;
            try
            {
                if (dbNetFace != null)
                {
                    IList<DbParameter> paraList = new List<DbParameter>();
                    paraList.Add(dbNetFace.CreateDbParameter("@p_equipmentNo", data.equipmentNo));
                    paraList.Add(dbNetFace.CreateDbParameter("@p_userid", data.userid));
                    paraList.Add(dbNetFace.CreateDbParameter("@p_TotalPack", data.TotalPack));
                    paraList.Add(dbNetFace.CreateDbParameter("@p_CurrentPack", data.CurrentPack));
                    paraList.Add(dbNetFace.CreateDbParameter("@p_FeaturePack", data.FeaturePack));
                    paraList.Add(dbNetFace.CreateDbParameter("@p_sumFeaturePack", data.SumFeaturePack));
                    result = dbNetFace.ExecuteNonQuery("pro_Lift_facesave", paraList, CommandType.StoredProcedure);
                }
            }
            catch (Exception ex)
            { ToolAPI.XMLOperation.WriteLogXmlNoTail("DB_MysqlLift.SaveListFaceFeaturePack异常", ex.Message); }
            return result;
        }
        /// <summary>
        /// 更新下发状态
        /// </summary>
        /// <param name="data"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static int UpdateFaceFeaturePack(LiftFeature data,string status)
        {
            string sql = "update equipment_lift_feature set isrecv=" + status + " where equipmentNo='" + data.equipmentNo + "' and userid='" + data.userid + "' and CurrentPack=" + data.CurrentPack;
            return dbNetFace.ExecuteNonQuery(sql, null, CommandType.Text);
        }
        /// <summary>
        /// 获取要下发的人脸模板
        /// </summary>
        /// <returns></returns>
        public static DataTable GetIssuedfeature()
        {
            try
            {
                if (dbNetFace != null)
                {
                    string sql = "select * from equipment_lift_feature_issued where issued_status=0 ";
                    return dbNetFace.ExecuteDataTable(sql, null, CommandType.Text);
                }
            }
            catch { return null; }
            return null;
        }
        /// <summary>
        /// 获取该条包的下发状态
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int GetIssueStatus(string equipmentNo, string userid, int CurrentPack)
        {
            string sql = "select isrecv from equipment_lift_feature where equipmentNo='" + equipmentNo + "' and userid='" + userid + "' and CurrentPack=" + CurrentPack + " limit 1";
            DataTable dt = dbNetFace.ExecuteDataTable(sql, null, CommandType.Text);
            return int.Parse(dt.Rows[0]["isrecv"].ToString());
        }
        /// <summary>
        /// 获取要下发的人脸数据
        /// </summary>
        /// <param name="equipmentNo"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public static IList<LiftFeatureIssued> GetIListIssued(string equipmentNo, string userid)
        {
            try
            {
                if (dbNetFace != null)
                {
                    string sql = "select equipmentNo,userid,TotalPack,CurrentPack,SumFeaturePack from equipment_lift_feature where equipmentNo='" + equipmentNo + "' and userid='" + userid + "' order by CurrentPack asc";
                    DataTable dt = dbNetFace.ExecuteDataTable(sql, null, CommandType.Text);
                    UpdateFaceSendStatus(equipmentNo, userid, 1);
                    return EntityReader.GetEntities<LiftFeatureIssued>(dt);
                }
            }
            catch { return null; }
            return null;
        }
        /// <summary>
        /// 更新下发状态为被服务器抓取
        /// </summary>
        /// <param name="equipmentNo"></param>
        /// <param name="userid"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static int UpdateFaceSendStatus(string equipmentNo, string userid,int status)
        {
            string sql = "update equipment_lift_feature_issued set issued_status="+status+" where equipmentNo='" + equipmentNo + "' and userid='" + userid + "'";
            UpdateFacePackStatus(equipmentNo, userid);
            return dbNetFace.ExecuteNonQuery(sql, null, CommandType.Text);
        }
        /// <summary>
        /// 更新要下发的所有包的状态为为执行状态
        /// </summary>
        /// <param name="equipmentNo"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        static int UpdateFacePackStatus(string equipmentNo, string userid)
        {
            string sql = "update equipment_lift_feature set isrecv=3 where equipmentNo='" + equipmentNo + "' and userid='" + userid + "'";
            return dbNetFace.ExecuteNonQuery(sql, null, CommandType.Text);
        }
        /// <summary>
        /// 最后查看一次
        /// </summary>
        /// <param name="equipmentNo"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public static int GetAllPackIsError(string equipmentNo, string userid)
        {
            string sql = "select count(1) from equipment_lift_feature where equipmentNo='" + equipmentNo + "' and userid='" + userid + "' and isrecv=0";
            DataTable dt = dbNetFace.ExecuteDataTable(sql, null, CommandType.Text);
            return int.Parse(dt.Rows[0][0].ToString());
        }
        /// <summary>
        /// 更改ip后的回答
        /// </summary>
        /// <param name="liftNo"></param>
        /// <param name="status"></param>
        /// <param name="issuccess"></param>
        public static void UpdateDataConfig(string liftNo, int status, bool issuccess) 
        {
            try
            {
                DbHelperSQL DBNet = DbNetAndSn.Keys.ToList().First();
                if (DBNet != null)
                {
                    string sql = "";
                    if (issuccess)
                    {
                        sql = string.Format("select ip_dn,port from equipment_lift_orderissued where equipmentNo='{0}'",liftNo);
                        DataTable dt = DBNet.ExecuteDataTable(sql, null, CommandType.Text);
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            sql = string.Format("update equipment_lift_orderissued set addr_status='{0}',addr_time=(select UNIX_TIMESTAMP(now())),ip_dn_backup='{1}',port_backup={2} where equipmentNo='{3}'", status, dt.Rows[0]["ip_dn"].ToString(), Convert.ToInt32(dt.Rows[0]["port"]), liftNo);
                            DBNet.ExecuteNonQuery(sql, null, CommandType.Text);
                        }
                    }
                    else
                    {
                        sql = string.Format("update equipment_lift_orderissued set addr_status='{0}',addr_time=(select UNIX_TIMESTAMP(now())) where equipmentNo='{1}'", status, liftNo);
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
        public static void UpdateIPCommandIssued(string equipmentNo, int status)
        {
            try
            {
                DbHelperSQL DBNet = DbNetAndSn.Keys.ToList().First();
                if (DBNet != null)
                {
                    string sql = string.Format("select creat_time from equipment_lift_orderissued_record where equipmentNo='{0}' order by creat_time desc limit 1", equipmentNo);
                    DataTable dt = DBNet.ExecuteDataTable(sql, null, CommandType.Text);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        sql = string.Format("update equipment_lift_orderissued_record set addr_status='{0}' where equipmentNo='{1}' and creat_time={2}", status, equipmentNo, Convert.ToInt32(dt.Rows[0]["creat_time"]));
                        DBNet.ExecuteNonQuery(sql, null, CommandType.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("UpdateIPCommandIssued异常", ex.Message);
            }
        }


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
    }
}
