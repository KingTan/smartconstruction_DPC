using API_request_data.科宇.卸料.model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_request_data
{
    /// <summary>
    /// 针对科宇的卸料平台
    /// </summary>
    public class Unload_KY
    {
        static string Get_token_url = "https://101.37.149.55/SmartSite/v2/getToken.action";
        static public void Get_token(string useuname,string password)
        {
            string datastring = string.Format("userName={0}&password={1}", useuname, password);
            string result = Restful.Post(Get_token_url, datastring);
        }
    }
}
