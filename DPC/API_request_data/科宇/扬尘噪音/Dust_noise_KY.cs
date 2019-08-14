using API_request_data.科宇.扬尘噪音;
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
    /// <summary>
    /// 针对科宇的扬尘噪音
    /// </summary>
    public class Dust_noise_KY
    {
        static string Dust_userName;
        static string Dust_password;
        static string Get_Real_list_url = "http://ykzt-hjjc.com:8086/apiservice/v2/getRealData";
        static Thread Get_real_currentT;
        static Dust_noise_KY()
        {
            try
            {
                Dust_userName = ToolAPI.INIOperate.IniReadValue("Base", "Dust_userName", Application.StartupPath + "\\Config.ini");
                Dust_password = ToolAPI.INIOperate.IniReadValue("Base", "Dust_password", Application.StartupPath + "\\Config.ini");
                Get_real_current_func();
            }
            catch (Exception ex)
            {
                Dust_userName = "zmkjycjc";
                Dust_password = "123456";
            }
        }

        public static void App_Open()
        {
            try
            {
                Get_real_currentT = new Thread(Get_real_current) { IsBackground = true};
                Get_real_currentT.Start();

                ToolAPI.XMLOperation.WriteLogXmlNoTail("Dust_noise_KY程序启动", "");
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("Dust_noise_KY启动程序出现异常", ex.Message + ex.StackTrace);
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
                ToolAPI.XMLOperation.WriteLogXmlNoTail("Dust_noise_KY程序关闭", "");
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("Dust_noise_KY启动关闭出现异常", ex.Message + ex.StackTrace);
            }
        }

        #region 接口调用
        /// <summary>
        /// 得到实时数据列表
        /// </summary>
        /// <param name="useuname">用户名</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        static public Get_Real_list_result Get_real_list(string useuname, string password)
        {
            try
            {
                string token = Get_token(useuname, password);
                string datastring = string.Format("userName={0}&token={1}", useuname, token);
                string result = Restful.Post(Get_Real_list_url, datastring);
                Get_Real_list_result real_List_Result = JsonConvert.DeserializeObject<Get_Real_list_result>(result);
                return real_List_Result;
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        public static string Get_token(string useuname,string password)
        {
            string string_split = useuname + password + DateTime.Now.ToString("yyyy-MM-dd");
            return MD5Encrypt(string_split, 32);
        }
        public static  string MD5Encrypt(string password, int bit)
        {
            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
            byte[] hashedDataBytes;
            hashedDataBytes = md5Hasher.ComputeHash(Encoding.GetEncoding("gb2312").GetBytes(password));
            StringBuilder tmp = new StringBuilder();
            foreach (byte i in hashedDataBytes)
            {
                tmp.Append(i.ToString("x2"));
            }
            if (bit == 16)
                return tmp.ToString().Substring(8, 16);
            else
            if (bit == 32) return tmp.ToString();//默认情况
            else return string.Empty;
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
                Get_Real_list_result real_List_Result = Get_real_list(Dust_userName, Dust_password);
                if (real_List_Result != null && real_List_Result.code == "200" && real_List_Result.data != null && real_List_Result.data.Length>0)
                {
                    foreach(object obj in real_List_Result.data)
                    {
                        Dust_noise_real dust_Noise_Real = Dust_noise_real.Init_Dust_noise_real(obj.ToString());
                        if(dust_Noise_Real!=null)
                        {
                            Zhgd_iot_dust_noise_current data = new Zhgd_iot_dust_noise_current();
                            data.sn = dust_Noise_Real.siteId.ToString();
                            data.@timestamp = dust_Noise_Real.time;
                            data.pm2_5 = dust_Noise_Real.PM25_Avg;
                            data.pm10 = dust_Noise_Real.PM10_Avg;
                            data.tsp = 0;
                            data.noise = dust_Noise_Real.B03_Avg;
                            data.temperature = dust_Noise_Real.T01_Avg;
                            data.humidity = dust_Noise_Real.H01_Avg;
                            data.wind_speed = dust_Noise_Real.W02_Avg;
                            data.wind_grade = ConvertWind.WindToLeve((float)data.wind_speed);
                            data.wind_direction = dust_Noise_Real.W01_Avg;
                            data.air_pressure = 0;
                            data.rainfall = 0;

                            //进行数据put 
                            DPC.Dust_noise_operation.Send_dust_noise_Current(data);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("Get_real_current_func异常", ex.Message);
            }
        }
        #endregion
    }
}
