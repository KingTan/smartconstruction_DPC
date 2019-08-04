using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace ProtocolAnalysis
{
    [Serializable]
    public class Current_CT
    {
        #region 属性
        /// <summary>
        /// 数据上传时间
        /// </summary>
        public string Time { set; get; }
        #region
        /// <summary>
        /// O3
        /// </summary>
        public string O3 { set; get; }
        /// <summary>
        /// 臭氧8小时浓度
        /// </summary>
        public string O3_eight { set; get; }
        /// <summary>
        /// NOX
        /// </summary>
        public string NOX { set; get; }
        /// <summary>
        /// CO
        /// </summary>
        public string CO { set; get; }
        /// <summary>
        /// SO2
        /// </summary>
        public string SO2 { set; get; }
        #endregion
        /// <summary>
        /// 消息流水号
        /// </summary>
        public int Sequence_Id { get; set; }
        /// <summary>
        /// 设备编号
        /// </summary>
        public uint Device_Id { get; set; }
        /// <summary>
        /// 时间戳
        /// </summary>
        public int Time_stamp { get; set; }
        /// <summary>
        /// 数据类型
        /// </summary>
        public short Data_type { get; set; }
        /// <summary>
        /// 粉尘数据 （需要除10000）
        /// </summary>
        public Decimal SPM { set; get; }
        /// <summary>
        /// PM2.5
        /// </summary>
        public Decimal PM25 { set; get; }
        /// <summary>
        /// PM10
        /// </summary>
        public Decimal PM10 { set; get; }
        /// <summary>
        /// 0：SPM有效1：PM2.5有效2：PM10 有效3：3个都有效
        /// </summary>
        public short TYPE { get; set; }
        /// <summary>
        /// 风向为0后台为正北90°
        /// </summary>
        public Decimal windDirection { get; set; }
        /// <summary>
        /// 风速为0后台显示0 m/s
        /// </summary>
        public Decimal windSpeed { get; set; }
        /// <summary>
        /// 温度2662 表示26.61
        /// </summary>
        public Decimal Temperature { get; set; }
        /// <summary>
        /// 湿度7661表示76.61
        /// </summary>
        public Decimal Humidity { get; set; }
        /// <summary>
        /// 噪音等效值763值为76.3
        /// </summary>
        public Decimal Noise { get; set; }
        /// <summary>
        /// 噪音峰值927 值为92.7
        /// </summary>
        public Decimal maxNoise { get; set; }
        /// <summary>
        /// 经度
        /// </summary>
        public double GPS_Y { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        public double GPS_X { get; set; }
        /// <summary>
        /// 大气压
        /// </summary>
        public float Pressure { get; set; }
        /// <summary>
        /// 保留
        /// </summary>
        public string Remark { get; set; }
        #endregion
        #region 方法
        public Current_CT Clone()
        {
            return (Current_CT)this.MemberwiseClone();
        }
        #endregion
    }
}
