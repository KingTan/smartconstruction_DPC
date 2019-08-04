using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Threading;
using TCPAPI;
using Architecture;
using ToolAPI;
using Newtonsoft.Json;
using ProtocolAnalysis.MassConcrete;
using System.Security.Cryptography;
using System.Linq;

namespace ProtocolAnalysis
{
    /// <summary>
    /// 联瑞科
    /// </summary>
    public static class GprsResolveData_LRK
    {
        #region 解析入口
        /// <summary>
        /// 解析、存储、显示数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static string OnResolveRecvMessage(byte[] b, int c, TcpSocketClient client)
        {
            DBFrame df = new DBFrame();
            // df.contenthex = ConvertData.ToHexString(b, 0, c);
            df.version = "01";
            df.datatype = "current";

            string contentString = Encoding.ASCII.GetString(b, 0, c);
            int stringLength = contentString.Length;
            bool startIsValid = contentString.StartsWith("$LRKKJ$");
            bool endIsValid = contentString.EndsWith("END");
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
            if (startIsValid && endIsValid)
            {
                string[] dictionary = contentString.Split(';');
                foreach (string dictionaryItem in dictionary)
                {
                    string[] ietm = dictionaryItem.Split(':');
                    if (ietm.Length > 1)
                    {
                        if (!keyValuePairs.ContainsKey(ietm[0]))
                            keyValuePairs.Add(ietm[0], ietm[1]);
                        if (ietm[0] == "DEV_sn")
                        {
                            df.deviceid = ietm[1];
                            TcpClientBindingExternalClass TcpExtendTemp = client.External.External as TcpClientBindingExternalClass;
                            if (TcpExtendTemp.EquipmentID == null || TcpExtendTemp.EquipmentID.Equals(""))
                            {
                                TcpExtendTemp.EquipmentID = df.deviceid;
                            }
                        }

                    }
                }
                if (keyValuePairs != null && keyValuePairs.Keys.Count > 0)
                {
                    df.contentjson = JsonConvert.SerializeObject(keyValuePairs);
                    DB_MysqlMassConcrete.SaveMassConcrete(df);
                }
            }
            return "";
        }
        #endregion



    }




}
