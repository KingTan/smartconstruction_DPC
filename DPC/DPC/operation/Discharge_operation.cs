﻿using Newtonsoft.Json;
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
    /// 卸料操作类
    /// </summary>
    public static class Discharge_operation
    {
        static Discharge_operation()
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
                DbHelperSQL dbNet = new DbHelperSQL(string.Format("Data Source={0};Port={1};Database={2};User={3};Password={4}", "39.104.20.2", "3306", "gd_db_v2", "wisdom_root", "JIwLi5j40SY#o1Et"), DbProviderType.MySql);
                Dictionary<string, string> Equipment_project_temp = new Dictionary<string, string>();
                string sql = "select distinct  equipment_sn,project_id from biz_project_equipment where equipment_type ='"+ Equipment_type.卸料平台 + "'";
                DataTable dt = dbNet.ExecuteDataTable(sql, null);
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string equipment_sn = dr["equipment_sn"].ToString();
                        string project_id = dr["project_id"].ToString();
                        //存入redis中
                        string key = "equipment:projectid:" + Equipment_type.卸料平台 + ":" + equipment_sn;
                        TimeSpan timeSpan = new TimeSpan(0, 0, 300);
                        RedisCacheHelper.Add(key, project_id, timeSpan);
                    }
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("卸料Sync_equipment_project异常", ex.Message);
            }
        }
        #endregion

        #region 获取卸料推送对象
        /// <summary>
        /// 设备项目字典
        /// </summary>
        private static Dictionary<string, Zhgd_iot_discharge_working_state> working_state = new Dictionary<string, Zhgd_iot_discharge_working_state>();
        /// <summary>
        /// 进行数据发送
        /// </summary>
        /// <param name="sn">设备序列码</param>
        /// <returns></returns>
        public static void Send_discharge_Current(Zhgd_iot_discharge_current zhgd_Iot_discharge_Current)
        {
            try
            {
                //获取redis中的项目
                string key = "equipment:projectid:" + Equipment_type.卸料平台 + ":" + zhgd_Iot_discharge_Current.sn;
                string value = RedisCacheHelper.Get<string>(key);
                if (value != null)
                {
                    zhgd_Iot_discharge_Current.create_time = DPC_Tool.GetTimeStamp();
                    zhgd_Iot_discharge_Current.project_id = value;
                    zhgd_Iot_discharge_Current.equipment_type = Equipment_type.卸料平台;
                    //这里面应该还有工作运行的判断以及运行序列码得赋值
                    if (working_state.ContainsKey(zhgd_Iot_discharge_Current.sn))
                        zhgd_Iot_discharge_Current.work_cycles_no = working_state[zhgd_Iot_discharge_Current.sn].Get_work_cycles_no(zhgd_Iot_discharge_Current);
                    else
                    {
                        working_state.Add(zhgd_Iot_discharge_Current.sn, new Zhgd_iot_discharge_working_state(zhgd_Iot_discharge_Current.sn));
                        zhgd_Iot_discharge_Current.work_cycles_no = working_state[zhgd_Iot_discharge_Current.sn].Get_work_cycles_no(zhgd_Iot_discharge_Current);
                    }
                    //执行put方法，把实时数据推走
                    Put_discharge_current(zhgd_Iot_discharge_Current);
                    //更新redis
                    DPC.Discharge_operation.Update_equminet_last_online_time(zhgd_Iot_discharge_Current.sn, zhgd_Iot_discharge_Current.timestamp);
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("卸料Send_discharge_Current异常", ex.Message);
            }
        }
        #endregion

        #region put ES Data
        /// <summary>
        /// put实时数据
        /// </summary>
        /// <param name="zhgd_Iot_discharge_Current"></param>
        static void Put_discharge_current(Zhgd_iot_discharge_current zhgd_Iot_discharge_Current)
        {
            try
            {
                string url = "https://111.56.13.177:52001/zhgd_iot-" + DateTime.Now.ToString("yyyyMMdd") + "/_doc/";
                string senddata = JsonConvert.SerializeObject(zhgd_Iot_discharge_Current);
                Restful.Post(url, senddata);
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("卸料Put_discharge_current异常", ex.Message);
            }
        }

        public static Action<Zhgd_iot_discharge_working> Put_work_cycles_event = Put_work_cycles;
        /// <summary>
        /// put卸料运行数据
        /// </summary>
        /// <param name="zhgd_Iot_Tower_Working"></param>
        public static void Put_work_cycles(Zhgd_iot_discharge_working zhgd_Iot_discharge_Working)
        {
            try
            {
                string url = "https://111.56.13.177:52001/zhgd_iot-" + DateTime.Now.ToString("yyyyMMdd") + "/_doc/";
                string senddata = JsonConvert.SerializeObject(zhgd_Iot_discharge_Working);
                Restful.Post(url, senddata);
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("卸料Put_work_cycles异常", ex.Message);
            }
        }
        #endregion

        #region 更新设备在线时间
        public static void Update_equminet_last_online_time(string sn,long time)
        {
            string key = "equipment:online_time:" + Equipment_type.卸料平台 + ":" + sn;
            RedisCacheHelper.Add(key, time);
        }
        #endregion
    }
}
