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

            Student st = new Student
            {
                Id = "001", Name = "zhao", Description = "ceshi", DateTime = DateTime.Now
            };
            client.Index<Student>(st);
        }
    }
}
