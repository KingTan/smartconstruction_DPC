using DPC_core;
using Nest;
using System;

namespace testDPC
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            ES_process eS_Process = new ES_process();
            ElasticClient client = eS_Process.ES_single_point_connection();

            //创建索引
            var descriptor = new CreateIndexDescriptor("db_student").Settings(s => s.NumberOfShards(5).NumberOfReplicas(1));
            client.Index(descriptor);
        }
    }
}
