using Newtonsoft.Json;
using SIXH.DBUtility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DPC
{
    /// <summary>
    /// 人员管理操作类
    /// </summary>
    public static class Personnel_operation
    {
        static Personnel_operation()
        {
            Sync_equipment_project_func();
            Sync_equipment_project_T = new Thread(Sync_equipment_project) { IsBackground = true };
            Sync_equipment_project_T.Start();
        }
        #region 设备项目同步
        //获取设备项目同步字典线程
        static Thread Sync_equipment_project_T;
        /// <summary>
        /// 同步设备项目
        /// </summary>
        static void Sync_equipment_project()
        {
            while (true)
            {
                Thread.Sleep(280000);//延时5分钟
                Sync_equipment_project_func();
            }
        }

        static void Sync_equipment_project_func()
        {
            try
            {
                DbHelperSQL dbNet;
                var connectionString = ToolAPI.INIOperate.IniReadValue("netSqlGroup", "connectionString", Application.StartupPath + "\\Config.ini");
                string[] connectionStringtearm = connectionString.Split('&');
                if (connectionStringtearm != null && connectionStringtearm.Length == 5)
                    dbNet = new DbHelperSQL(string.Format("Data Source={0};Port={1};Database={2};User={3};Password={4}", connectionStringtearm[0], connectionStringtearm[1], connectionStringtearm[2], connectionStringtearm[3], connectionStringtearm[4]), DbProviderType.MySql);
                else
                    dbNet = new DbHelperSQL(string.Format("Data Source={0};Port={1};Database={2};User={3};Password={4}", "172.24.108.167", "3306", "gd_db_v2", "wisdom_root", "JIwLi5j40SY#o1Et"), DbProviderType.MySql);
                Dictionary<string, string> Equipment_project_temp = new Dictionary<string, string>();
                string sql = "select project_code,project_id from biz_project";
                DataTable dt = dbNet.ExecuteDataTable(sql, null);
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string project_personnel_id = dr["project_code"].ToString();
                        string project_id = dr["project_id"].ToString();
                        //存入redis中
                        string key = "equipment:projectid:" + Equipment_type.人员管理 + ":" + project_personnel_id;
                        TimeSpan timeSpan = new TimeSpan(0, 0, 300);
                        RedisCacheHelper.Add(key, project_id, timeSpan);
                    }
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("人员管理Sync_equipment_project异常", ex.Message);
            }
        }
        #endregion

        #region 获取人员管理推送对象
        /// <summary>
        /// 进行数据发送
        /// </summary>
        /// <returns></returns>
        public static void Send_personnel_records(Zhgd_iot_personnel_records zhgd_Iot_Personnel_Records)
        {
            try
            {
                //获取redis中的项目
                string key = "equipment:projectid:" + Equipment_type.人员管理 + ":" + zhgd_Iot_Personnel_Records.project_code;
                string value = RedisCacheHelper.Get<string>(key);
                if (value != null)
                {
                    zhgd_Iot_Personnel_Records.create_time = DPC_Tool.GetTimeStamp();
                    zhgd_Iot_Personnel_Records.project_id = value;
                    zhgd_Iot_Personnel_Records.equipment_type = Equipment_type.人员管理;
                    //执行put方法，把实时数据推走
                    Put_Send_personnel_records(zhgd_Iot_Personnel_Records);
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("人员管理Send_personnel_records异常", ex.Message);
            }
        }
        #endregion

        #region put ES Data
        /// <summary>
        /// put实时数据
        /// </summary>
        /// <param name="zhgd_Iot_Personnel_Records"></param>
        static void Put_Send_personnel_records(Zhgd_iot_personnel_records zhgd_Iot_Personnel_Records)
        {
            try
            {
                string url = "https://111.56.13.177:52001/zhgd_iot-" + DateTime.Now.ToString("yyyyMMdd") + "/_doc/";
                string senddata = JsonConvert.SerializeObject(zhgd_Iot_Personnel_Records);
                Restful.Post(url, senddata);
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("人员管理Put_Send_personnel_records异常", ex.Message);
            }
        }
        #endregion
    }
}
