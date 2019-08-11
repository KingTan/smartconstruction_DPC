using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ToolAPI;
/*---------------------------------------------
    Copyright (c) 2017 共友科技
    版权所有：共友科技
    创建人名：赵通
    创建描述：全局静态类
    创建时间：2017.10.11
    文件功能描述：全局属性供整个项目使用
    修改人名：
    修改描述：
    修改标识：
    修改时间：
    ---------------------------------------------*/
namespace Architecture
{
    public static class MainStatic
    {
        /// <summary>
        /// 配置文件的路径
        /// </summary>
        public static String Path
        {
            get { return Application.StartupPath + "\\Config.ini"; }
        }

        /// <summary>
        /// 服务器端口
        /// </summary>
        public static String Port{ get; set;}
        /// <summary>
        /// 设备类型
        /// </summary>
        public static int DeviceType{get;set; }

        /// <summary>
        /// 塔吊和升降机是否需要身份验证
        /// </summary>
        public static String IsAuthentication { get; set;}
        static  MainStatic()
        {
            try
            {
                Port = ToolAPI.INIOperate.IniReadValue("goyo", "Port", MainStatic.Path);
                DeviceType = int.Parse(ToolAPI.INIOperate.IniReadValue("goyo", "DeviceType", MainStatic.Path));
                IsAuthentication = ToolAPI.INIOperate.IniReadValue("goyo", "IsAuthentication", MainStatic.Path);
            }
            catch(Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("MainStatic构造异常", ex.Message + ex.StackTrace);
                Port = "5000";
                DeviceType = 0;
            }
        }
    }
}
