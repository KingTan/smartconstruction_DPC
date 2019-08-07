using Newtonsoft.Json;
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
    /// 扬尘噪音操作类
    /// </summary>
    public static class Dust_noise_operation
    {
        static Dust_noise_operation()
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
                string sql = "select bpe.equipment_sn,bpe.project_id,bwn.pm25_warn_value,bwn.pm10_warn_value,bwn.noise_warn_value from biz_warn_config_dust_noise as bwn,biz_project_equipment as bpe where bwn.equipment_id = bpe.equipment_id and bpe.equipment_type='" + Equipment_type.扬尘噪音 + "'";
                DataTable dt = dbNet.ExecuteDataTable(sql, null);
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string equipment_sn = dr["equipment_sn"].ToString();
                        string project_id = dr["project_id"].ToString();
                        string pm25_warn_value = dr["pm25_warn_value"].ToString();
                        string pm10_warn_value = dr["pm10_warn_value"].ToString();
                        string noise_warn_value = dr["noise_warn_value"].ToString();
                        string value = project_id + "&" + pm25_warn_value + "&" + pm10_warn_value + "&" + noise_warn_value;
                        //存入redis中
                        string key = "equipment:projectid:" + Equipment_type.扬尘噪音 + ":" + equipment_sn;
                        TimeSpan timeSpan = new TimeSpan(0, 0, 300);
                        RedisCacheHelper.Add(key, value, timeSpan);
                    }
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("扬尘噪音Sync_equipment_project异常", ex.Message);
            }
        }
        #endregion

        #region 获取扬尘噪音推送对象
        /// <summary>
        /// 进行数据发送
        /// </summary>
        /// <param name="sn">设备序列码</param>
        /// <returns></returns>
        public static void Send_dust_noise_Current(Zhgd_iot_dust_noise_current zhgd_Iot_dust_noise_Current)
        {
            try
            {
                //获取redis中的项目
                string key = "equipment:projectid:" + Equipment_type.扬尘噪音 + ":" + zhgd_Iot_dust_noise_Current.sn;
                string value = RedisCacheHelper.Get<string>(key);
                if (value != null)
                {
                    string[] item = value.Split('&');
                    zhgd_Iot_dust_noise_Current.create_time = DPC_Tool.GetTimeStamp();
                    zhgd_Iot_dust_noise_Current.project_id = item[0];
                    zhgd_Iot_dust_noise_Current.equipment_type = Equipment_type.扬尘噪音;
                    //报警判断
                    zhgd_Iot_dust_noise_Current.is_warning = "N";
                    List<string> vs = new List<string>();
                    if(zhgd_Iot_dust_noise_Current.pm2_5>double.Parse(item[1]))
                    {
                        vs.Add(Warning_type.PM2_5报警);
                        zhgd_Iot_dust_noise_Current.is_warning = "Y";
                    }
                    if (zhgd_Iot_dust_noise_Current.pm10 > double.Parse(item[2]))
                    {
                        vs.Add(Warning_type.PM10报警);
                        zhgd_Iot_dust_noise_Current.is_warning = "Y";
                    }
                    if (zhgd_Iot_dust_noise_Current.noise > double.Parse(item[3]))
                    {
                        vs.Add(Warning_type.噪音告警);
                        zhgd_Iot_dust_noise_Current.is_warning = "Y";
                    }
                    zhgd_Iot_dust_noise_Current.warning_type = vs.ToArray();
                    //进行AQI计算
                    zhgd_Iot_dust_noise_Current.aqi = 1;//明天我找刘凡要一个AQI的计算方法
                    //执行put方法，把实时数据推走
                    Put_dust_noise_current(zhgd_Iot_dust_noise_Current);
                    //更新在线时间
                    Update_equminet_last_online_time(zhgd_Iot_dust_noise_Current.sn, zhgd_Iot_dust_noise_Current.timestamp);
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("扬尘噪音Send_dust_noise_Current异常", ex.Message);
            }
        }
        #endregion

        #region put ES Data
        /// <summary>
        /// put实时数据
        /// </summary>
        /// <param name="zhgd_Iot_dust_noise_Current"></param>
        static void Put_dust_noise_current(Zhgd_iot_dust_noise_current zhgd_Iot_dust_noise_Current)
        {
            try
            {
                string url = "https://111.56.13.177:52001/zhgd_iot-" + DateTime.Now.ToString("yyyyMMdd") + "/_doc/";
                string senddata = JsonConvert.SerializeObject(zhgd_Iot_dust_noise_Current);
                Restful.Post(url, senddata);
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("扬尘噪音Put_dust_noise_current异常", ex.Message);
            }
        }
        #endregion

        #region 更新设备在线时间
        public static void Update_equminet_last_online_time(string sn,long time)
        {
            string key = "equipment:online_time:" + Equipment_type.扬尘噪音 + ":" + sn;
            RedisCacheHelper.Add(key, time);
        }
        #endregion
    }
}
