using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Architecture;
using ProtocolAnalysis.MassConcrete;
using TCPAPI;
using ToolAPI;
namespace ProtocolAnalysis.TowerMcLrk.OE
{
    /// <summary>
    /// 命令下发
    /// </summary>
    public class CommandIssued_MCLRK
    {
        //更改限位控制信息
        public static void MCLRK_SetControl(IList<TcpSocketClient> SocketList)
        {
            try
            {
                DataTable dt = DB_MysqlMassConcrete.GetMcControlCongfig();
                if (dt != null)
                {
                    int iRows = dt.Rows.Count;
                    if (iRows > 0)
                    {
                        for (int i = 0; i < iRows; i++)
                        {
                            for (int j = 0; j < SocketList.Count; j++)
                            {
                                string McLrkNo = (SocketList[j].External.External as TcpClientBindingExternalClass).EquipmentID;
                                string McLrkNoServer = dt.Rows[i]["equipmentNo"].ToString();
                                if (McLrkNo != null && McLrkNo.Equals(McLrkNoServer))
                                {
                                    string sendString = "$LRKKJ$;";
                                    sendString += "CWY_HC:" + dt.Rows[i]["returntime"].ToString() + ";";
                                    sendString += "CWY_MAX:" + dt.Rows[i]["alarmmax"].ToString() + ";";
                                    sendString += "CWY_MIN:" + dt.Rows[i]["alarmmin"].ToString() + ";";
                                    sendString += "CWY_TIM:" + dt.Rows[i]["spraytim"].ToString() + ";";
                                    sendString += "CWY_PL:" + dt.Rows[i]["spraypl"].ToString() + ";";
                                    sendString += "END";
                                    byte[] message = Encoding.Default.GetBytes(sendString);
                                    if (message != null)
                                    {
                                        SocketList[j].SendBuffer(message);
                                        ToolAPI.XMLOperation.WriteLogXmlNoTail("GprsMcLrk.McLrk_SetControl:info", string.Format("【{0}】控制设备{1}的ip,{2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), McLrkNo, ConvertData.ToHexString(message, 0, message.Length)));
                                    }
                                    DB_MysqlMassConcrete.UpdateMcControlCongfig(McLrkNo);
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception) { }
        }
    }
}
