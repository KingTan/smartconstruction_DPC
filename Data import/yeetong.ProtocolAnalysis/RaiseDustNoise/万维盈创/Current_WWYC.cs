using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace ProtocolAnalysis
{
    /*
     *##0265ST=22;CN=2011;PW=123456;MN=WWYC0010000001;CP=&&DataTime=20160719000100;925-Rtd=32.9,925-Flag=N;107-Rtd=56.2,107-Flag=N;126-Rtd=28.1,126-Flag=N;128-Rtd=81.0,128-Flag=N;129-Rtd=0.1,129-Flag=N;130-Rtd=315.0,130-Flag=N;127-Rtd=99.803,127-Flag=N;DI1-Rtd=1.0,DI1-Flag=N;103-Rtd=2.80,103-Flag=N;B03-Rtd=65.0,B03-Flag=N;926-Rtd=299.803,926-Flag=N;927-Rtd=199.803,927-Flag=N&&BC81
     * */
    [Serializable]
    public class Current_WWYC
    {
        /// <summary>
        /// 数据上传时间 时间戳
        /// </summary>
        public string Time { set; get; }
        /// <summary>
        /// ST 212协议的系统编号，“22”代表 空气质量监测；
        /// </summary>
        public string ST { set; get; }
        /// <summary>
        /// 212协议的命令编号，“2011”代表 取污染物实时数据（分钟数据）
        /// </summary>
        public string CN { set; get; }
        /// <summary>
        /// 现无实际用途，可固定为“123456”；
        /// </summary>
        public string PW { set; get; }
        /// <summary>
        /// MN 用作设备识别，共14位，格式规范详见附表1“堆场监测设备MN编号命名规则”。
        /// </summary>
        public string MN { set; get; }
        /// <summary>
        /// PM2.5 925
        /// </summary>
        public string PM2_5 { set; get; }
        /// <summary>
        /// PM10 107
        /// </summary>
        public string PM10 { set; get; }
        /// <summary>
        /// 温度 126
        /// </summary>
        public string Temperature { set; get; }
        /// <summary>
        /// 湿度 128
        /// </summary>
        public string Humidity { set; get; }
        /// <summary>
        /// 风速 129
        /// </summary>
        public string WindSpeed { set; get; }
        /// <summary>
        /// 风向 130
        /// </summary>
        public string WindDirection { set; get; }
        /// <summary>
        /// 气压 127
        /// </summary>
        public string Pressure { set; get; }
        /// <summary>
        /// TSP 103
        /// </summary>
        public string TSP { set; get; }
        /// <summary>
        /// 噪声 B03
        /// </summary>
        public string Noise { set; get; }
        /// <summary>
        /// 经度 926
        /// </summary>
        public string Longitude { set; get; }
        /// <summary>
        /// 纬度 927
        /// </summary>
        public string Latitude { set; get; }

        public Current_WWYC()
        {
            Time = "";
            ST = "";
            CN = "";
            PW = "";
            MN = "";
            PM2_5 = "";
            PM10 = "";
            Temperature = "";
            Humidity = "";
            WindSpeed = "";
            WindDirection = "";
            Pressure = "";
            TSP = "";
            Noise = "";
            Longitude = "";
            Latitude = "";
        }
        public Current_WWYC Clone()
        {
            return (Current_WWYC)this.MemberwiseClone();
        }
    }
}
