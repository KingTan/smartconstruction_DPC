using SIXH.DBUtility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DPC
{
    /// <summary>
    /// 塔吊操作类
    /// </summary>
    public static class Tower_operation
    {
        static Tower_operation()
        {
            Sync_equipment_project_T = new Thread(Sync_equipment_project) { IsBackground = true };
            Sync_equipment_project_T.Start();
        }
        #region 设备项目同步
        //获取设备项目同步字典线程
        static Thread Sync_equipment_project_T;
        /// <summary>
        /// 设备项目字典
        /// </summary>
        private static Dictionary<string, string> Equipment_project;
        /// <summary>
        /// 同步设备项目
        /// </summary>
        static void Sync_equipment_project()
        {
            
            while (true)
            {
                try
                {
                    DbHelperSQL dbNet = new DbHelperSQL(string.Format("Data Source={0};Port={1};Database={2};User={3};Password={4}", "39.104.20.2", "3306", "gd_db_v2", "wisdom_root", "JIwLi5j40SY#o1Et"), DbProviderType.MySql);
                    Dictionary<string, string> Equipment_project_temp = new Dictionary<string, string>() ;
                    string sql = "select distinct  equipment_sn,project_id from biz_project_equipment where equipment_type ='01_01'";
                    DataTable dt = dbNet.ExecuteDataTable(sql, null);
                    if(dt!=null&&dt.Rows.Count>0)
                    {
                        foreach(DataRow dr in dt.Rows)
                        {
                            string equipment_sn = dr["equipment_sn"].ToString();
                            string project_id = dr["project_id"].ToString();
                            if (!Equipment_project_temp.ContainsKey(equipment_sn))
                                Equipment_project_temp.Add(equipment_sn, project_id);
                        }
                        //开始替换字典
                        lock(Equipment_project)
                        {
                            Equipment_project = Equipment_project_temp;
                        }
                    }
                }
                catch(Exception ex)
                {
                    ToolAPI.XMLOperation.WriteLogXmlNoTail("塔吊Sync_equipment_project异常", ex.Message);
                }
                Thread.Sleep(300000);//延时5分钟
            }
        }
        #endregion

        #region 获取塔吊推送对象
        /// <summary>
        /// 获取塔吊实时数据对象
        /// </summary>
        /// <param name="sn">设备序列码</param>
        /// <returns></returns>
        public static Zhgd_iot_tower_current Get_Zhgd_iot_tower_current(string sn)
        {
            try
            {
                if (Equipment_project != null && Equipment_project.ContainsKey(sn))
                {
                    Zhgd_iot_tower_current zhgd_Iot_Tower_Current = new Zhgd_iot_tower_current();
                    zhgd_Iot_Tower_Current.create_time = GetTimeStamp();
                    zhgd_Iot_Tower_Current.project_id = Equipment_project[sn];
                    zhgd_Iot_Tower_Current.sn = sn;
                    zhgd_Iot_Tower_Current.equipment_type = Equipment_type.塔机;
                    //这里面应该还有工作运行的判断
                    return zhgd_Iot_Tower_Current;
                }
                else
                    return null;
            }
            catch(Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("塔吊Get_Zhgd_iot_tower_current异常", ex.Message);
                return null;
            }
        }
        /// <summary>
        /// 获取塔吊运行数据对象
        /// </summary>
        /// <param name="sn">设备序列码</param>
        /// <returns></returns>
        public static Zhgd_iot_tower_current Get_Zhgd_iot_tower_working(string sn)
        {
            try
            {
                if (Equipment_project != null && Equipment_project.ContainsKey(sn))
                {
                    Zhgd_iot_tower_working zhgd_Iot_Tower_Current = new Zhgd_iot_tower_working();
                    zhgd_Iot_Tower_Current.create_time = GetTimeStamp();
                    zhgd_Iot_Tower_Current.project_id = Equipment_project[sn];
                    zhgd_Iot_Tower_Current.sn = sn;
                    zhgd_Iot_Tower_Current.equipment_type = Equipment_type.塔机;
                    return zhgd_Iot_Tower_Current;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("塔吊Get_Zhgd_iot_tower_working异常", ex.Message);
                return null;
            }
        }
        #endregion

        public static long GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds);
        }
    }
}
