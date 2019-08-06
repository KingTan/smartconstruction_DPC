﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToolAPI;
namespace Architecture
{
    public class MainClass
    {
        Process O_GprsSpecialEquipment;
        /// <summary>
        /// 启动程序
        /// </summary>
        public void App_Open(Subject sub)
        {
            try
            {
                O_GprsSpecialEquipment = new Process(sub);
                ToolAPI.XMLOperation.WriteLogXmlNoTail("程序启动", "");
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("启动程序出现异常", ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void App_Close()
        {
            try
            {
                if (O_GprsSpecialEquipment != null)
                {
                    O_GprsSpecialEquipment.App_Close();
                }
                ToolAPI.XMLOperation.WriteLogXmlNoTail("程序关闭", "");
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("启动关闭出现异常", ex.Message + ex.StackTrace);
            }
        }
    }
}
