using DPC;
using Newtonsoft.Json;
using ProtocolAnalysis.Iot_v1.model;
using SIXH.DBUtility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProtocolAnalysis.Iot_v1.operation
{
    /// <summary>
    /// 注册帧的处理
    /// </summary>
    public class Register_operation
    {
        static Register_operation()
        {
            Sync_vendor_code_func();
            Sync_vendor_code_T = new Thread(Sync_vendor_code) { IsBackground = true };
            Sync_vendor_code_T.Start();
        }
        #region 厂商编码项目同步
        //获取设备项目同步字典线程
        static Thread Sync_vendor_code_T;
        /// <summary>
        /// 同步设备项目
        /// </summary>
        static void Sync_vendor_code()
        {
            while (true)
            {
                Thread.Sleep(280000);//延时大约分钟
                Sync_vendor_code_func();
            }
        }

        static void Sync_vendor_code_func()
        {
            try
            {
                DbHelperSQL dbNet = new DbHelperSQL(string.Format("Data Source={0};Port={1};Database={2};User={3};Password={4}", "39.104.20.2", "3306", "gd_db_v2", "wisdom_root", "JIwLi5j40SY#o1Et"), DbProviderType.MySql);
                Dictionary<string, string> Equipment_project_temp = new Dictionary<string, string>();
                string sql = "select distinct  supplier_code,supplier_abbreviation from biz_supplier ";
                DataTable dt = dbNet.ExecuteDataTable(sql, null);
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        string supplier_code = dr["supplier_code"].ToString();
                        string supplier_abbreviation = dr["supplier_abbreviation"].ToString();
                        //存入redis中
                        string key = "supplier_code:" + supplier_code;
                        TimeSpan timeSpan = new TimeSpan(0, 0, 300);
                        RedisCacheHelper.Add(key, supplier_abbreviation, timeSpan);
                    }
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("Sync_vendor_code_func异常", ex.Message);
            }
        }
        #endregion

        #region 认证厂商识别码是否正确
        /// <summary>
        /// 认证厂商识别码是否正确且返回帧验证
        /// </summary>
        /// <param name="register_send_frame">厂商识别码</param>
        /// <returns>帧验证码</returns>
        public static string Judge_vendor_code(object register_send_frame)
        {
            try
            {
                Register_send_frame rsf = JsonConvert.DeserializeObject<Register_send_frame>(register_send_frame.ToString());
                if (rsf != null && !string.IsNullOrEmpty(rsf.vendor_code))
                {
                    string key = "supplier_code:" + rsf.vendor_code;
                    string value = RedisCacheHelper.Get<string>(key);
                    if (value != null)
                    {
                        return System.Guid.NewGuid().ToString("N");
                    }
                }
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("Judge_vendor_code异常", ex.Message);
            }
            return null;
        }
        #endregion
    }
}
