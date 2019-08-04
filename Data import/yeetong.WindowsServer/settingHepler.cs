using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Configuration;
using ToolAPI;
namespace GOYO_SpecialEquipmentServer
{
   public class SettingHelper : IDisposable
    {
        #region 构造函数
        /// <summary> 
        /// 初始化服务配置帮助类 
        /// </summary> 
        public SettingHelper()
        {
            InitSettings();
        }
        #endregion

        #region 属性
        /// <summary> 
        /// 系统用于标志此服务的名称 
        /// </summary> 
        public string ServiceName
        {
            get;
            set;
        }
        /// <summary> 
        /// 向用户标志服务的友好名称 
        /// </summary> 
        public string DisplayName
        {
            get;
            set;
        }
        /// <summary> 
        /// 服务的说明 
        /// </summary> 
        public string Description
        {
            get;
            set;
        }
        #endregion

        #region 私有方法
        #region 初始化服务配置信息
        /// <summary> 
        /// 初始化服务配置信息 
        /// </summary> 
        private void InitSettings()
        {
            string root = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string path = root.Remove(root.LastIndexOf('\\') + 1) + "Config.ini";
            ServiceName = ToolAPI.INIOperate.IniReadValue("goyo", "ServiceName", path);
            DisplayName = ToolAPI.INIOperate.IniReadValue("goyo", "DisplayName", path);
            Description = ToolAPI.INIOperate.IniReadValue("goyo", "Description", path);
        }
        #endregion
        #endregion

        #region IDisposable 成员
        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            disposed = true;
        }
        ~SettingHelper()
        {
            Dispose(false);
        }
        #endregion 
    }
}
