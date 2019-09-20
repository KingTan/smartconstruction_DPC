using API_request_data.仁科.扬尘噪音;
using DPC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace API_request_data
{
    public class Dust_noise_RK
    {
        static string Dust_userName;
        static string Dust_password;
        static string Get_Real_list_url = "http://www.0531yun.cn/wsjc/Device/getDeviceData.do?";//http://www.0531yun.cn/wsjc/Device/getDeviceData.do?userID= jnrstest&userPassword= jnrstest321
        static Thread Get_real_currentT;
        static Dust_noise_RK()
        {
            try
            {
                Dust_userName = ToolAPI.INIOperate.IniReadValue("Base", "Dust_userName_rk", Application.StartupPath + "\\Config.ini");
                Dust_password = ToolAPI.INIOperate.IniReadValue("Base", "Dust_password_rk", Application.StartupPath + "\\Config.ini");
                Get_real_current_func();
            }
            catch (Exception ex)
            {
                Dust_userName = "190918mlzj";
                Dust_password = "190918mlzj";
            }
        }

        public static void App_Open()
        {
            try
            {
                Get_real_currentT = new Thread(Get_real_current) { IsBackground = true};
                Get_real_currentT.Start();

                ToolAPI.XMLOperation.WriteLogXmlNoTail("Dust_noise_RK程序启动", "");
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("Dust_noise_RK启动程序出现异常", ex.Message + ex.StackTrace);
            }
        }
        public static void App_Close()
        {
            try
            {
                if (Get_real_currentT != null && Get_real_currentT.IsAlive)
                {
                    Get_real_currentT.Abort();
                    Get_real_currentT = null;
                }
                ToolAPI.XMLOperation.WriteLogXmlNoTail("Dust_noise_RK程序关闭", "");
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("Dust_noise_RK启动关闭出现异常", ex.Message + ex.StackTrace);
            }
        }

        #region 接口调用
        /// <summary>
        /// 得到实时数据列表
        /// </summary>
        /// <param name="useuname">用户名</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        static public List<Get_Real_list_result_RK> Get_real_list(string useuname, string password)
        {
            try
            {
                string datastring = string.Format("{0}userID={1}&userPassword={2}", Get_Real_list_url, useuname, password);
                string result = Restful.HttpGet( datastring);
                List< Get_Real_list_result_RK> real_List_Result = JsonConvert.DeserializeObject<List<Get_Real_list_result_RK>>(result);
                return real_List_Result;
            }
            catch (Exception ex)
            {
                return null;
            }

        }
       
        #endregion

        #region 数据获取业务
        static void Get_real_current()
        {
            while (true)
            {
                Thread.Sleep(280000);//延时大约5分钟
                Get_real_current_func();
            }
        }

        static void Get_real_current_func()
        {

            try
            {
                //先得到token
                List<Get_Real_list_result_RK> real_List_Result = Get_real_list(Dust_userName, Dust_password);
                if (real_List_Result != null && real_List_Result.Count>0)
                {
                    Dictionary<string, Zhgd_iot_dust_noise_current> noise_current = new Dictionary<string, Zhgd_iot_dust_noise_current>();
                    foreach(Get_Real_list_result_RK obj in real_List_Result)
                    {
                        if(noise_current.ContainsKey(obj.DevAddr))
                        {
                            if (obj.DevStatus == "true")
                                item_foreach(obj, noise_current[obj.DevAddr]);
                        }
                        else
                        {
                            if (obj.DevStatus == "true")
                            {
                                Zhgd_iot_dust_noise_current zhgd_Iot_Dust_Noise_Current = new Zhgd_iot_dust_noise_current();
                                zhgd_Iot_Dust_Noise_Current.sn = obj.DevAddr;
                                item_foreach(obj, zhgd_Iot_Dust_Noise_Current);
                                noise_current.Add(obj.DevAddr, zhgd_Iot_Dust_Noise_Current);
                            }
                        }
                    }

                    foreach (Zhgd_iot_dust_noise_current data in noise_current.Values)
                    {
                        data.timestamp = GetTimeStamp();
                        DPC.Dust_noise_operation.Send_dust_noise_Current(data);
                    }
                }

            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("Get_real_current_func异常", ex.Message);
            }
        }

        static void item_foreach( Get_Real_list_result_RK rl, Zhgd_iot_dust_noise_current zc)
        {
            switch (rl.DevHumiName)
            {
                case "噪声(dB)":
                    zc.noise = double.Parse(rl.DevHumiValue); break;
                case "PM10(ug/m3)":
                    zc.pm10 = double.Parse(rl.DevHumiValue); break;
                case "PM2.5(ug/m3)":
                    zc.pm2_5 = double.Parse(rl.DevHumiValue); break;
                case "气压(Kpa)":
                    zc.air_pressure = double.Parse(rl.DevHumiValue); break;
                case "累计雨量(mm)":
                    zc.rainfall = double.Parse(rl.DevHumiValue); break;
                case "TSP(ug/m3)":
                    zc.tsp = double.Parse(rl.DevHumiValue); break;
                case "湿度(%RH)":
                    zc.humidity = double.Parse(rl.DevHumiValue); break;
                case "温度(℃)":
                    zc.temperature = double.Parse(rl.DevHumiValue); break;
                case "风力(风力)":
                    zc.wind_grade = int.Parse(rl.DevHumiValue); break;
                case "风速(m/s)":
                    zc.wind_speed = double.Parse(rl.DevHumiValue); break;
                case "风向":
                    zc.wind_direction = double.Parse(rl.DevHumiValue); break;
                default: break;
            }
            switch (rl.DevTempName)
            {
                case "噪声(dB)":
                    zc.noise = double.Parse(rl.DevTempValue); break;
                case "PM10(ug/m3)":
                    zc.pm10 = double.Parse(rl.DevTempValue); break;
                case "PM2.5(ug/m3)":
                    zc.pm2_5 = double.Parse(rl.DevTempValue); break;
                case "气压(Kpa)":
                    zc.air_pressure = double.Parse(rl.DevTempValue); break;
                case "累计雨量(mm)":
                    zc.rainfall = double.Parse(rl.DevTempValue); break;
                case "TSP(ug/m3)":
                    zc.tsp = double.Parse(rl.DevTempValue); break;
                case "湿度(%RH)":
                    zc.humidity = double.Parse(rl.DevTempValue); break;
                case "温度(℃)":
                    zc.temperature = double.Parse(rl.DevTempValue); break;
                case "风力(风力)":
                    zc.wind_grade = int.Parse(rl.DevTempValue); break;
                case "风速(m/s)":
                    zc.wind_speed = double.Parse(rl.DevTempValue); break;
                case "风向":
                    zc.wind_direction = double.Parse(rl.DevTempValue); break;
                default: break;
            }
        }
        public static long GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds);
        }
        #endregion
    }
}
