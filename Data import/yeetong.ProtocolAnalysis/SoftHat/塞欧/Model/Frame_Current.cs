using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace ProtocolAnalysis.SoftHat
{
    [Serializable]
    public class Frame_Current
    {
        /// <summary>
        /// 设备地址
        /// </summary>
        public string DeviceNo
        {
            get;
            set;
        }
        /// <summary>
        /// 标签ID
        /// </summary>
        public string IDCard_Id
        {
            get;
            set;
        }
        /// <summary>
        /// 标签状态 0 高电量 1低电量
        /// </summary>
        public string IDCard_Status
        {
            get;
            set;
        }
        /// <summary>
        /// 时间
        /// </summary>
        public string Receive_Time
        {
            get;
            set;
        }

        public Frame_Current()
        {
            DeviceNo = "";
            IDCard_Id = "";
            IDCard_Status = "1";
            Receive_Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public object Clone()
        {
            BinaryFormatter formatter = new BinaryFormatter(null, new System.Runtime.Serialization.StreamingContext(System.Runtime.Serialization.StreamingContextStates.Clone));
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, this);
            stream.Position = 0;
            object clonedObj = formatter.Deserialize(stream);
            stream.Close();
            return clonedObj;
        }
    }

    [Serializable]
    public class Frame_CurrentList
    {
        /// <summary>
        /// 设备地址
        /// </summary>
        public string DeviceNo
        {
            get;
            set;
        }
        /// <summary>
        /// 人数
        /// </summary>
        public int PeopleNumber
        {
            get;
            set;
        }
        /// <summary>
        /// 标签ID
        /// </summary>
        public Dictionary<string,string> IDCard_Id
        {
            get;
            set;
        }
        public Frame_CurrentList()
        {
            DeviceNo = "";
            IDCard_Id = new Dictionary<string, string>();
        }

        public object Clone()
        {
            BinaryFormatter formatter = new BinaryFormatter(null, new System.Runtime.Serialization.StreamingContext(System.Runtime.Serialization.StreamingContextStates.Clone));
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, this);
            stream.Position = 0;
            object clonedObj = formatter.Deserialize(stream);
            stream.Close();
            return clonedObj;
        }
    }
}
