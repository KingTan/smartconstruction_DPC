using System;
using System.Collections.Generic;
using System.Text;
using Nest;

namespace DPC_core
{
    public static class Setting
    {
        public static string strConnectionString = @"https://111.56.13.177:52001";
        public static Uri Node
        {
            get
            {
                return new Uri(strConnectionString);
            }
        }
        public static ConnectionSettings ConnectionSettings
        {
            get
            {
                return new ConnectionSettings(Node).DefaultIndex("default");
            }
        }
    }
}
