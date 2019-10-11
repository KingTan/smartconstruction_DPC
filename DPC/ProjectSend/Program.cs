using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectSend
{
    class Program
    {
        static void Main(string[] args)
        {
            //生产者
            var factory = new ConnectionFactory();
            factory.HostName = "39.104.228.149";//RabbitMQ服务在本地运行
            factory.Port = 5672;
            factory.UserName = "guest";//用户名
            factory.Password = "sixteam666";//密码

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {

                    bool durable = true;
                    channel.QueueDeclare("task_queue", durable, false, false, null);

                    string message = GetMessage(args);
                    var properties = channel.CreateBasicProperties();
                    properties.SetPersistent(true);


                    var body = Encoding.UTF8.GetBytes(message);
                    channel.BasicPublish("", "task_queue", properties, body);
                    Console.WriteLine(" set {0}", message);
                }
            }

        }
        private static string GetMessage(string[] args)
        {
            return ((args.Length > 0) ? string.Join(" ", args) : "Hello World!");
        }
    }
}
