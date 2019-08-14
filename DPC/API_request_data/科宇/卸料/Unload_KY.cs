
using API_request_data.科宇.卸料;
using DPC;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace API_request_data
{
    /// <summary>
    /// 针对科宇的卸料平台
    /// </summary>
    public class Unload_KY
    {
        static string Unload_userName;
        static string Unload_password;
        static string Get_token_url = "http://101.37.149.55/SmartSite/v2/getToken.action";
        static string Get_Device_list_url = "http://101.37.149.55/UnloadingPlatform/v2/listAllUnloadByUnloadId.action";
        static string Get_Real_list_url = "http://101.37.149.55/UnloadingPlatform/v2/listRealUnloadData.action";
        static Thread Get_real_currentT;
        static Unload_KY()
        {
            try
            {
                Unload_userName = ToolAPI.INIOperate.IniReadValue("Base", "Unload_userName", Application.StartupPath + "\\Config.ini");
                Unload_password = ToolAPI.INIOperate.IniReadValue("Base", "Unload_password", Application.StartupPath + "\\Config.ini");
                Get_real_current_func();
            }
            catch (Exception ex)
            {
                Unload_userName = "hfykcs";
                Unload_password = "hfykcs";
            }
        }

        public static void App_Open()
        {
            try
            {
                Get_real_currentT = new Thread(Get_real_current) { IsBackground = true};
                Get_real_currentT.Start();

                ToolAPI.XMLOperation.WriteLogXmlNoTail("Unload_KY程序启动", "");
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("Unload_KY启动程序出现异常", ex.Message + ex.StackTrace);
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
                ToolAPI.XMLOperation.WriteLogXmlNoTail("Unload_KY程序关闭", "");
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("Unload_KY启动关闭出现异常", ex.Message + ex.StackTrace);
            }
        }

        #region 接口调用
        /// <summary>
        /// 得到token
        /// </summary>
        /// <param name="useuname">用户名</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        static public Get_token_result Get_token(string useuname, string password)
        {
            try
            {
                string datastring = string.Format("userName={0}&password={1}", useuname, password);
                string result = Restful.Post(Get_token_url, datastring);
                Get_token_result get_Token_Result = JsonConvert.DeserializeObject<Get_token_result>(result);
                return get_Token_Result;
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        /// <summary>
        /// 得到设备列表
        /// </summary>
        /// <param name="useuname">用户名</param>
        /// <param name="token">token</param>
        /// <returns></returns>
        static public Get_Device_list_result Get_Device_list(string useuname, string token)
        {
            try
            {
                string datastring = string.Format("userName={0}&token={1}", useuname, token);
                string result = Restful.Post(Get_Device_list_url, datastring);
                Get_Device_list_result get_Device_List_Result = JsonConvert.DeserializeObject<Get_Device_list_result>(result);
                return get_Device_List_Result;
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        /// <summary>
        /// 得到实时数据列表
        /// </summary>
        /// <param name="useuname">用户名</param>
        /// <param name="token">token</param>
        /// <param name="listUnloadId">设备编号集合，多个设备编号用‘,’隔开</param>
        /// <returns></returns>
        static public Get_Real_list_result Get_Real_list(string useuname, string token, string listUnloadId)
        {
            try
            {
                string datastring = string.Format("userName={0}&token={1}&listUnloadId={2}", useuname, token, listUnloadId);
                string result = Restful.Post(Get_Real_list_url, datastring);
                Get_Real_list_result get_Real_List_Result = JsonConvert.DeserializeObject<Get_Real_list_result>(result);
                return get_Real_List_Result;
            }
            catch (Exception ex)
            {
                return null;
            }

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
                Get_token_result get_Token_Result = Get_token(Unload_userName, Unload_password);
                if (get_Token_Result != null && get_Token_Result.status == "1" && get_Token_Result.data != null && !string.IsNullOrEmpty(get_Token_Result.data.token))
                {
                    //得到设备列表
                    Get_Device_list_result get_Device_List_Result = Get_Device_list(Unload_userName, get_Token_Result.data.token);
                    if (get_Device_List_Result != null && get_Token_Result.status == "1" && get_Device_List_Result.data != null && get_Device_List_Result.data.Length > 0)
                    {
                        string equmient_list = "";
                        foreach (Unload_device unload_Device in get_Device_List_Result.data)
                        {
                            if (unload_Device.activated == "1")
                                equmient_list += unload_Device.unload_id + ",";
                        }
                        if (equmient_list != "")
                            equmient_list = equmient_list.Substring(0, equmient_list.Length - 1);
                        //得到实时数据
                        Get_Real_list_result get_Real_List_Result = Get_Real_list(Unload_userName, get_Token_Result.data.token, equmient_list);
                        if (get_Real_List_Result != null && get_Real_List_Result.status == "1" && get_Real_List_Result.data != null && get_Real_List_Result.data.Length > 0)
                        {
                            foreach (Unload_real unload_Real in get_Real_List_Result.data)
                            {
                                Zhgd_iot_discharge_current data = new Zhgd_iot_discharge_current();
                                data.sn = unload_Real.unload_id;
                                if (unload_Real.upstate == 2)
                                {
                                    data.is_warning = "Y";
                                    data.warning_type = new string[] { Warning_type.重量告警 };
                                }
                                else
                                {
                                    data.is_warning = "N";
                                    data.warning_type = new string[] { };
                                }
                                data.@timestamp = unload_Real.time;
                                data.weight = unload_Real.weight;
                                if (unload_Real.bias < 0)
                                    data.dip_x = System.Math.Abs(unload_Real.bias);
                                else
                                    data.dip_y = unload_Real.bias;

                                //进行数据put 
                                DPC.Discharge_operation.Send_discharge_Current(data);
                            }
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
