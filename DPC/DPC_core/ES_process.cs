using System;
using Elasticsearch.Net;
using Nest;
using Newtonsoft.Json;

namespace DPC_core
{
    public class ES_process
    {
        /// <summary>
        /// 点单链接
        /// </summary>
        public ElasticClient ES_single_point_connection()
        {
            //https://111.56.13.177:52001/
            //elastic   yitongwuxian
            var node = new Uri("https://111.56.13.177:52001");
            var settings = new ConnectionSettings(node);
            var client = new ElasticClient(settings);
            return client;
        }

        /// <summary>
        /// 链接池链接
        /// </summary>
        public ElasticClient ES_pool_connection()
        {
            var nodes = new Uri[]
             {
                new Uri("https://111.56.13.177:52001"),
                new Uri("https://111.56.13.177:52001"),
                new Uri("https://111.56.13.177:52001")
             };
            var pool = new StaticConnectionPool(nodes);
            var settings = new ConnectionSettings(pool);
            var client = new ElasticClient(settings);
            return client;
        }
    }
}
