using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Xml;
using System.IO;
using System.Data.Common;
using SIXH.DBUtility;
/*---------------------------------------------
    Copyright (c) 2017 共友科技
    版权所有：共友科技
    创建人名：赵通
    创建描述：惜蓝数据库操作
    创建时间：2017.6.28
    文件功能描述：惜蓝数据库操作
    修改人名：
    修改描述：
    修改标识：
    修改时间：
    ---------------------------------------------*/
namespace ProtocolAnalysis
{
    public class DB_XL
    {
        public static Func<Current_XL, int> SaveCurrentAsyn = SaveCurrent;
        #region 获取该工地所有的扬尘噪声设备编号
        /// <summary>
        /// 获取该工地所有的扬尘噪声设备编号
        /// </summary>
        /// <returns></returns>
        public static string GetDustSn(string proid)
        {
            string sn = "";
            string sql = "select equipmentNo from y_equipment where proId='" + proid + "'";
            //DataTable dt = DbHelperSQL.GetDataInfo(sql);
            DataTable dt = DBoperateClass.DBoperateObj.ExecuteDataTable(sql, null, CommandType.Text);
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    sn += dt.Rows[i]["equipmentNo"].ToString() + ",";
                }
            }
            return sn.Trim(',');
        }
        #endregion
        #region 保存扬尘噪声数据
        /// <summary>
        /// 保存扬尘噪声数据
        /// </summary>
        /// <param name="dust"></param>
        /// <returns></returns>
        public static int SaveCurrent(Current_XL dust)
        {
            try
            {
                //IN `Sequence_Id` int,IN `Device_Id` varchar(50) ,IN `Time_stamp`int,IN `Data_type`  int,IN `SPM` decimal(18,2),IN `PM25` decimal(18,2),IN `PM10` decimal(18,2),IN `TYPE` int,IN `windDirection` decimal(18,2),IN `windSpeed` decimal(18,2),IN `Temperature` decimal(18,2),IN `Humidity` decimal(18,2),IN `Noise` decimal(18,2),IN `maxNoise` decimal(18,2),IN `GPS_Y` decimal(18,2),IN `GPS_X` decimal(18,2),IN `Pressure` decimal(18,2)
                IList<DbParameter> paraList = new List<DbParameter>();
                paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@Sequence_Id", 0));
                paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@Device_Id", dust.deCode));
                paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@Time_stamp", 0));
                paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@Data_type", 0));
                paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@SPM", Decimal.Parse("0")));
                paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@PM25", Decimal.Parse(dust.dePm25)));
                paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@PM10", Decimal.Parse(dust.dePm10)));
                paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@TYPE", 0));
                paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@windDirection", Decimal.Parse("0")));
                paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@windSpeed", Decimal.Parse(dust.deSpeed)));
                paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@Temperature", Decimal.Parse(dust.deTem)));
                paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@Humidity", Decimal.Parse(dust.deHum)));
                paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@Noise", Decimal.Parse(dust.deNoise)));
                paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@maxNoise", Decimal.Parse("0")));//没有
                paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@GPS_Y", Decimal.Parse("0")));
                paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@GPS_X", Decimal.Parse("0")));
                paraList.Add(DBoperateClass.DBoperateObj.CreateDbParameter("@Pressure", Decimal.Parse(dust.dePre)));
                int y = DBoperateClass.DBoperateObj.ExecuteNonQuery("pro_PMRealData", paraList, CommandType.StoredProcedure);
                ToolAPI.XMLOperation.WriteLogXmlNoTail(System.Windows.Forms.Application.StartupPath + @"\XL", "惜蓝设备数据库存储", y.ToString());
                return y;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail(System.Windows.Forms.Application.StartupPath + @"\XL", "惜蓝设备数据库存储异常", ex.Message);
                return 0;
            }

        }
        #endregion

    }
}
