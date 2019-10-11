using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DPC
{
    /// <summary>
    /// RabbitMQ队列类
    /// 生产者/消费者
    /// </summary>
    public class RabbitMQ
    {
        public static string HostName = "39.104.228.149";
        public static Int32 Port = 5672;
        public static string UserName = "guest";
        public static string Password = "sixteam666";

        /// <summary>
        /// 生产者生产产品进仓库
        /// </summary>
        /// <param name="queue_name">仓库名称</param>
        /// <param name="message">生成的产品</param>
        /// <returns></returns>
        static public bool producer(string queue_name, string message)
        {
            try
            {
                var factory = new ConnectionFactory();
                factory.HostName = HostName;//RabbitMQ服务在本地运行
                factory.Port = Port;
                factory.UserName = UserName;//用户名
                factory.Password = Password;//密码

                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {

                        bool durable = true;
                        channel.QueueDeclare(queue_name, durable, false, false, null);

                        var properties = channel.CreateBasicProperties();
                        properties.SetPersistent(true);

                        var body = Encoding.UTF8.GetBytes(message);
                        channel.BasicPublish("", queue_name, properties, body);
                        Console.WriteLine(" set {0}", message);
                        ToolAPI.XMLOperation.WriteLogXmlNoTail("queue_name:" + queue_name, message);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                ToolAPI.XMLOperation.WriteLogXmlNoTail("producer异常", ex.Message);
                return false;
            }
        }
        /// <summary>
        /// 消费者使用样例
        /// </summary>
        /// <param name="queue_name"></param>
        static public void consumer(string queue_name)
        {
            var factory = new ConnectionFactory();
            factory.HostName = HostName;//RabbitMQ服务在本地运行
            factory.Port = Port;
            factory.UserName = UserName;//用户名
            factory.Password = Password;//密码

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    bool durable = true;
                    channel.QueueDeclare(queue_name, durable, false, false, null);
                    channel.BasicQos(0, 1, false);

                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume(queue_name, false, consumer);
                    while (true)
                    {
                        var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();

                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        ///todo
                        channel.BasicAck(ea.DeliveryTag, false);
                    }
                }
            }
        }
    }
}
