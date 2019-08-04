using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProtocolAnalysis.FogGun
{
    /// <summary>
    ///雾泡定时所yon
    /// </summary>
    public class FoggunSettingTimeWorkModel
    {
        public string uuid
        {
            get;
            set;
        }
        /// <summary>
        /// 设备编号
        /// </summary>
        public string equipmentNo
        {
            get;
            set;
        }
        /// <summary>
        /// 喷淋时间
        /// </summary>
        public DateTime workTime
        {
            get;
            set;
        }
        /// <summary>
        /// 工作周期
        /// </summary>
        public int workCycle
        {
            get;
            set;
        }
        /// <summary>
        /// 闹钟的重复配置
        /// </summary>
        public string repeatConfig
        {
            get;
            set;
        }

        /// <summary>
        /// 操作标识的更新时间
        /// </summary>
        public string OperationMarkTime
        {
            get;
            set;
        }
    }
}
