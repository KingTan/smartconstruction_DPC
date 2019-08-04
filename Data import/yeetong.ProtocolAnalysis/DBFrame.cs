using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace ProtocolAnalysis
{
    [Serializable]
    public class DBFrame
    {
        public string deviceid { get; set; }
        public string datatype { get; set; }
        public string contentjson { get; set; }
        public string contenthex { get; set; }
        public string version { get; set; }

        public static  DBFrame DeepCopy<DBFrame>(DBFrame obj)
        {
            object retval;
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                //序列化成流
                bf.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                //反序列化成对象
                retval = bf.Deserialize(ms);
                ms.Close();
            }
            return (DBFrame)retval;
        }
    }
}
