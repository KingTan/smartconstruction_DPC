using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace API_request_data
{
    public class MainClass
    {
        /// <summary>
        /// 启动程序
        /// </summary>
        public static void App_Open()
        {
            try
            {
                Unload_KY.App_Open();
                Dust_noise_KY.App_Open();
                Dust_noise_RK.App_Open();
                ToolAPI.XMLOperation.WriteLogXmlNoTail("程序启动", "");
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("启动程序出现异常", ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 关闭程序
        /// </summary>
        public static void App_Close()
        {
            try
            {
                Unload_KY.App_Close();
                Dust_noise_KY.App_Close();
                Dust_noise_RK.App_Close();
                ToolAPI.XMLOperation.WriteLogXmlNoTail("程序关闭", "");
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("启动关闭出现异常", ex.Message + ex.StackTrace);
            }
        }
    }
}
