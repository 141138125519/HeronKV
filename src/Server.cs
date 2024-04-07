using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace src
{
    internal class Server : BackgroundService
    {
        private readonly ILogger<Server> _logger;

        IPHostEntry ipHostInfo;
        IPAddress ipAddress;
        IPEndPoint ipEndPoint;
        
        public Server(ILogger<Server> logger)
        {
            _logger = logger;

            _logger.LogInformation("Starting Server");

            ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            ipAddress = ipHostInfo.AddressList[0];
            ipEndPoint = new(ipAddress, 6379);

            _logger.LogInformation("Setup ip end point at: {ipEndPoint}", ipEndPoint);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await Listen();

                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task Listen()
        {
            _logger.LogInformation("Listening");
            try
            {
                using Socket listener = new(
                    ipEndPoint.AddressFamily,
                    SocketType.Stream,
                    ProtocolType.Tcp);

                listener.Bind(ipEndPoint);
                listener.Listen(100);

                var handler = await listener.AcceptAsync();
                _logger.LogInformation("Receiving");
                while (true)
                {
                    var buffer = new byte[1_024];
                    var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
                    var response = Encoding.UTF8.GetString(buffer, 0, received);

                    var eom = "<|EOM|>";
                    if (response.IndexOf(eom) > -1)
                    {
                        var echoBytes = Encoding.UTF8.GetBytes("+OK\\r\\n");
                        await handler.SendAsync(echoBytes, 0);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception: {ex}", ex);
            }
        }
    }
}
