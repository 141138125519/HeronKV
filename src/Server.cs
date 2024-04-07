using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace KVDB
{
    internal class Server : BackgroundService
    {
        private readonly ILogger<Server> _logger;

        IPHostEntry ipHostInfo;
        IPAddress ipAddress;
        IPEndPoint ipEndPoint;

        List<Socket> clients;
        
        public Server(ILogger<Server> logger)
        {
            _logger = logger;

            _logger.LogInformation("Starting Server");

            ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            ipAddress = ipHostInfo.AddressList[0];
            //ipEndPoint = new(ipAddress, 6379);
            ipEndPoint = new(IPAddress.Parse("0.0.0.0"), 6379);

            clients = [];

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
                await ListenForClientConnections();
            }
        }

        private async Task ListenForClientConnections()
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

                clients.Add(handler);
                _logger.LogInformation("Connected Clients: {clientsCount}", clients.Count);

                // Creates a new task for the newly connected client
                // not sure if this is the best way to handle this
                // but it allows for multple connections, without
                // having to handle multithreading myself.
                _ = Task.Run(() => Listen(handler));

            }
            catch (Exception ex)
            {
                _logger.LogError("Exception: {ex}", ex);
            }
        }

        private async Task Listen(Socket client)
        {
            _logger.LogInformation("New client thread");
            try
            {
                while (client.Connected)
                {
                    _logger.LogInformation("Receiving");

                    using NetworkStream nStream = new(client);
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
                    await client.SendAsync(echoBytes, 0);
                }
            }
            catch (NullReferenceException ex)
            {
                _logger.LogError("Client Disconnected - remove from list and close socket");
                _logger.LogError("{ex}", ex);
                clients.Remove(client);
                client.Close();
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception: {ex}", ex);
            }
        }
    }
}
