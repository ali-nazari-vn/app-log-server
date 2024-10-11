
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using System.Text;

namespace LogServer.BackroundServices
{
    public class LogServerHostedService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var logQueueName = "log-queu";
            var factory = new ConnectionFactory() { HostName = "localhost", UserName = "guest", Password = "guest" };
            var connection = factory.CreateConnection();
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: logQueueName,
                                   durable: false,
                                   exclusive: false,
                                   autoDelete: false,
                                   arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    IDictionary<string, object> headers = ea.BasicProperties.Headers; // get headers from Received msg

                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body.ToArray());
                    Console.WriteLine(message);

                    Log.Error($"Error {DateTime.Now}:  -  " + message);
                };

                channel.BasicConsume(queue: logQueueName,
                                     autoAck: true,
                                     consumer: consumer);

                Console.ReadLine();

            }
        }
    }
}
