using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;

namespace KVDB
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
            //ipEndPoint = new(ipAddress, 6379);
            ipEndPoint = new(IPAddress.Parse("0.0.0.0"), 6379);

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

                //await Task.Delay(1000, stoppingToken);
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
                while (handler.Connected)
                {
                    _logger.LogInformation("Receiving");

                    using NetworkStream nStream = new(handler);
                    var buffer = new List<byte>();

                    do
                    {
                        buffer.Add((byte)nStream.ReadByte());
                    } while (nStream.DataAvailable);

                    var resp = Encoding.UTF8.GetString(buffer.ToArray());
                    _logger.LogWarning(resp);

                    // pass to parser
                    var sReader = new StringReader(resp);
                    RESPParser parse = new(sReader);
                    var cont = parse.Read();
                    foreach (var a in cont.Array)
                    {
                        _logger.LogInformation($"arrBulk: {a.Bulk}");
                    }
                    //

                    var echoBytes = Encoding.UTF8.GetBytes("+OK\r\n");
                    await handler.SendAsync(echoBytes, 0);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception: {ex}", ex);
            }
        }
    }
}
