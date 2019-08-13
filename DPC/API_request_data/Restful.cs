using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace API_request_data
{
    public class Restful
    {
        static public string Post(string url ,string senddata,string ContentType= "application/x-www-form-urlencoded")
        {
            try
            {
                #region http有返回
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = ContentType;
                Util.SetCertificatePolicy();
                byte[] data = Encoding.UTF8.GetBytes(senddata);
                request.ContentLength = data.Length;
                using (Stream reqStream = request.GetRequestStream())
                {
                    reqStream.Write(data, 0, data.Length);
                    reqStream.Close();
                }
                HttpWebResponse resp = (HttpWebResponse)request.GetResponse();
                Stream stream = resp.GetResponseStream();
                string encoding = resp.ContentEncoding;
                if (encoding == null || encoding.Length < 1)
                {
                    encoding = "UTF-8"; //默认编码  
                }
                StreamReader reader = new StreamReader(resp.GetResponseStream(), Encoding.GetEncoding(encoding));
                string retString = reader.ReadToEnd();
                return retString;
                #endregion
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("Post异常", ex.Message);
                return ex.Message;
            }
        }

       

        #region 默认ssl 成功
        public static class Util
        {
            /// <summary>
            /// Sets the cert policy.
            /// </summary>
            public static void SetCertificatePolicy()
            {
                ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidate;
            }

            /// <summary>
            /// Remotes the certificate validate.
            /// </summary>
            private static bool RemoteCertificateValidate(
               object sender, X509Certificate cert,
                X509Chain chain, SslPolicyErrors error)
            {
                // trust any certificate!!!
                System.Console.WriteLine("Warning, trust any certificate");
                return true;
            }
        }
        #endregion
    }
}
