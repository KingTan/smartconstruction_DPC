using System;
using System.Collections.Generic;
using System.Text;
using DPC_core.model;
using Nest;

namespace DPC_core
{
    public class ESProvider
    {
        public static ElasticClient client = new ElasticClient(Setting.ConnectionSettings);
        public static string strIndexName = @"meetup".ToLower();
        public static string strDocType = "events".ToLower();

      
    }
}
