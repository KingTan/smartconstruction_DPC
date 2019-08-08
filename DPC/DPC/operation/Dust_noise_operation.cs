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
                    zhgd_Iot_dust_noise_Current.aqi = Get_aqi(zhgd_Iot_dust_noise_Current.pm2_5);
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

        #region aqi算法
        static double Get_aqi(double PM25)
        {
            try
            {
                double iaqiMin = 0;
                double iaqiMax = 0;
                double pm25Min = 0;
                double pm25Max = 0;
                if (PM25 >= 0 && PM25 < 35)
                {
                    pm25Min = 0;
                    pm25Max = 35;
                    iaqiMin = 0;
                    iaqiMax = 50;
                }
                if (PM25 >= 35 && PM25 < 75)
                {
                    pm25Min = 35;
                    pm25Max = 75;
                    iaqiMin = 50;
                    iaqiMax = 100;
                }
                if (PM25 >= 75 && PM25 < 115)
                {
                    pm25Min = 75;
                    pm25Max = 115;
                    iaqiMin = 100;
                    iaqiMax = 150;
                }
                if (PM25 >= 115 && PM25 < 150)
                {
                    pm25Min = 115;
                    pm25Max = 150;
                    iaqiMin = 150;
                    iaqiMax = 200;
                }
                if (PM25 >= 150 && PM25 < 250)
                {
                    pm25Min = 150;
                    pm25Max = 250;
                    iaqiMin = 200;
                    iaqiMax = 300;
                }
                if (PM25 >= 250 && PM25 < 350)
                {
                    pm25Min = 250;
                    pm25Max = 350;
                    iaqiMin = 300;
                    iaqiMax = 400;
                }
                if (PM25 >= 350 && PM25 < 500)
                {
                    pm25Min = 350;
                    pm25Max = 500;
                    iaqiMin = 400;
                    iaqiMax = 500;
                }
                if (PM25 >= 500)
                {
                    pm25Min = 350;
                    pm25Max = 500;
                    iaqiMin = 400;
                    iaqiMax = 500;
                }
                double iaqi = (iaqiMax - iaqiMin) / (pm25Max - pm25Min) * (PM25 - pm25Min) + iaqiMin;
                //if (iaqi > 0 && iaqi <= 50)
                //{
                //    grade = 1;
                //}
                //if (iaqi > 50 && iaqi <= 100)
                //{
                //    grade = 2;
                //}
                //if (iaqi > 100 && iaqi <= 150)
                //{
                //    grade = 3;
                //}
                //if (iaqi > 150 && iaqi <= 200)
                //{
                //    grade = 4;
                //}
                //if (iaqi > 200 && iaqi <= 300)
                //{
                //    grade = 5;
                //}
                //if (iaqi > 300 && iaqi <= 500)
                //{
                //    grade = 6;
                //}
                //if (iaqi > 500)
                //{
                //    grade = 7;
                //}
                return iaqi;
            }
            catch(Exception ex)
            {
                return 0;
            }
        }
        #endregion
    }
}
