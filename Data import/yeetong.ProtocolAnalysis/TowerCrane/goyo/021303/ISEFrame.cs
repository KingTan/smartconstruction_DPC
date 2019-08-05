using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ProtocolAnalysis.TowerCrane._021303
{
    public interface ISEFrame
    {
        /// <summary>
        /// 版本号
        /// </summary>
        string AVersion { get; set; }
        /// <summary>
        /// 设备编号
        /// </summary>
        string EquipmentID { get; set; }
        /// <summary>
        /// 数据帧类型  heartbeat，current，other
        /// 主要用于MQTT
        /// </summary>
        string FrameDataType { get; set; }
        /// <summary>
        /// 帧数据对象
        /// </summary>
        object FrameObject { get; set; }
        /// <summary>
        /// MQTT数据对象
        /// </summary>
        object FrameObject_MQTT { get; set; }
    }
}
