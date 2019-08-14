using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace API_request_data.科宇.扬尘噪音
{
    public class Get_Real_list_result
    {
        /// <summary>
        /// 状态信息
        /// </summary>
        public string msg { get; set; }
        /// <summary>
        /// 状态码
        /// </summary>
        public string code { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        public object[] data { get; set; }
    }
    public class Dust_noise_real
    {
        /// <summary>
        /// 噪音
        /// </summary>
        public double B03_Avg { get; set; }
        /// <summary>
        /// 风速
        /// </summary>
        public double W02_Avg { get; set; }
        /// <summary>
        /// 温度
        /// </summary>
        public double T01_Avg { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        public string latitude { get; set; }
        /// <summary>
        /// 站点名称
        /// </summary>
        public string siteName { get; set; }
        /// <summary>
        /// 风向
        /// </summary>
        public double W01_Avg { get; set; }
        /// <summary>
        /// PM10
        /// </summary>
        public double PM10_Avg { get; set; }
        /// <summary>
        /// PM2.5
        /// </summary>
        public double PM25_Avg { get; set; }
        /// <summary>
        /// 站点id
        /// </summary>
        public int siteId { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public int state { get; set; }
        /// <summary>
        /// 时间
        /// </summary>
        public long time { get; set; }
        /// <summary>
        /// 经度
        /// </summary>
        public string longitude { get; set; }
        /// <summary>
        /// 湿度
        /// </summary>
        public double H01_Avg { get; set; }

        public static  Dust_noise_real Init_Dust_noise_real(string jsonstring)
        {
            try
            {
                Dust_noise_real dust_Noise_Real = JsonConvert.DeserializeObject<Dust_noise_real>(jsonstring);
                dust_Noise_Real.B03_Avg = double.Parse(GetAttributeValue(jsonstring, "\"B03-Avg\"").Replace("\"",""));
                dust_Noise_Real.W02_Avg = double.Parse(GetAttributeValue(jsonstring, "\"W02-Avg\"").Replace("\"", ""));
                dust_Noise_Real.T01_Avg = double.Parse(GetAttributeValue(jsonstring, "\"T01-Avg\"").Replace("\"", ""));
                dust_Noise_Real.W01_Avg = double.Parse(GetAttributeValue(jsonstring, "\"W01-Avg\"").Replace("\"", ""));
                dust_Noise_Real.PM10_Avg = double.Parse(GetAttributeValue(jsonstring, "\"PM10-Avg\"").Replace("\"", ""));
                dust_Noise_Real.PM25_Avg = double.Parse(GetAttributeValue(jsonstring, "\"PM25-Avg\"").Replace("\"", ""));
                dust_Noise_Real.H01_Avg = double.Parse(GetAttributeValue_H(jsonstring, "\"H01-Avg\"").Replace("\"", ""));
                dust_Noise_Real.time = DPC.DPC_Tool.GetTimeStamp(DateTime.ParseExact(dust_Noise_Real.time.ToString(), "yyyyMMddHHmmss", System.Globalization.CultureInfo.CurrentCulture));
                return dust_Noise_Real;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        #region 获取对应的字段的值
        /// <summary>
        /// 获取对应属性的值 =和;
        /// </summary>
        /// <param name="sourceString"></param>
        /// <param name="Attribute"></param>
        /// <returns></returns>
        static public string GetAttributeValue(string sourceString, string Attribute)
        {
            Attribute += ":";
            return GetValue(sourceString, Attribute, ",");
        }
        static public string GetAttributeValue_H(string sourceString, string Attribute)
        {
            Attribute += ":";
            return GetValue(sourceString, Attribute, "}");
        }
        /// <summary>
        /// 街区一段字符中对应的开始和结束之间的字符
        /// </summary>
        /// <param name="sourceString">源字符串</param>
        /// <param name="start">开始字符串</param>
        /// <param name="end">结束字符串</param>
        /// <returns></returns>
        static public string GetValue(string sourceString, string start, string end)
        {
            Regex rg = new Regex("(?<=(" + start + "))[.\\s\\S]*?(?=(" + end + "))", RegexOptions.Multiline | RegexOptions.Singleline);
            return rg.Match(sourceString).Value;
        }
        #endregion
    }

}
