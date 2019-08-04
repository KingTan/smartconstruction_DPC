using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
/*---------------------------------------------
    Copyright (c) 2017 共友科技
    版权所有：共友科技
    创建人名：赵通
    创建描述：组装SQL辅助类
    创建时间：2017.6.28
    文件功能描述：组装SQL辅助类
    修改人名：
    修改描述：
    修改标识：
    修改时间：
    ---------------------------------------------*/
namespace ProtocolAnalysis
{
    public class SQLJoint
    {
        /// <summary>
        /// 拼接SQL
        /// </summary>
        /// <param name="name">字段名</param>
        /// <param name="value">对应值</param>
        /// <param name="isString">是否是字符串</param>
        /// <param name="nameStr">字段名集</param>
        /// <param name="valueStr">值集</param>
        public static void AddField(string name, object value, bool isString, ref string nameStr, ref string valueStr)
        {
            nameStr += name.ToString() + ",";
            if (isString) valueStr += "'" + value.ToString() + "',";
            else valueStr += value.ToString() + ",";
        }

        public static string RemoveLastChar(string name)
        {
            try
            {
                if (name.Length <= 0)
                    return name;
                else return name.Substring(0, name.Length - 1);
            }
            catch (Exception) { return ""; }

        }
    }
}
