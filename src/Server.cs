using HeronKV.Data;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HeronKV
{
    internal class Server : BackgroundService
    {
        private readonly ILogger<Server> _logger;
        private readonly RESPParser _parser;
        private readonly RESPSerialiser _serialiser;
        private readonly CommandsHandler _commandsHandler;
        private readonly IAOF _aof;

        IPHostEntry ipHostInfo;
        IPAddress ipAddress;
        IPEndPoint ipEndPoint;

        List<Socket> clients;
        List<Task> tasks;
        
        public Server(ILogger<Server> logger,
            RESPParser parser,
            RESPSerialiser serialiser,
            CommandsHandler handler,
            IAOF aof)
        {
            _logger = logger;
            _parser = parser;
            _serialiser = serialiser;
            _commandsHandler = handler;
            _aof = aof;

            _logger.LogInformation("Starting Server");

            ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            ipAddress = ipHostInfo.AddressList[0];
            //ipEndPoint = new(ipAddress, 6379);
            ipEndPoint = new(IPAddress.Parse("0.0.0.0"), 6379);

            _logger.LogInformation("Setup ip end point at: {ipEndPoint}", ipEndPoint);

            clients = [];
            tasks = [];

            _logger.LogInformation("Read from AOF file");

            _aof.Rebuild();

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }

                await ListenForClientConnections(stoppingToken);
            }
            Task.WaitAll([.. tasks], stoppingToken);
        }

        private async Task ListenForClientConnections(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
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

                    var handler = await listener.AcceptAsync(cancellationToken);
                    if (handler.Connected)
                    {
                        clients.Add(handler);
                        _logger.LogInformation("Connected Clients: {clientsCount}", clients.Count);

                        // Creates a new task for the newly connected client
                        // not sure if this is the best way to handle this
                        // but it allows for multple connections, without
                        // having to handle multithreading myself.
                        //_ = Task.Run(() => Listen(handler));
                        tasks.Add(Task.Run(() => Listen(handler, cancellationToken), cancellationToken));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Exception: {ex}", ex);
                }
            }
        }

        private async Task Listen(Socket client, CancellationToken cancellationToken)
        {
            _logger.LogInformation("New client thread");
            try
            {
                while (client.Connected && !cancellationToken.IsCancellationRequested)
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
                    var cont = _parser.NewRead(sReader);

                    foreach (var a in cont.Array!)
                    {
                        _logger.LogInformation($"arrBulk: {a.Bulk}");
                    }
                    //

                    if (cont.Type != "array")
                    {
                        _logger.LogError("Invalid Request - expected array");
                        continue;
                    }

                    if (cont.Array.Length == 0)
                    {
                        _logger.LogError("Invalid Request - expected array length > 0");
                    }

                    cont.Array[0].Bulk = cont.Array[0].Bulk!.ToUpper();
                    
                    if (cont.Array[0].Bulk == "SET" || cont.Array[0].Bulk == "HSET")
                    {
                        _aof.Write(cont);
                    }
                    
                    var cmdResult = _commandsHandler.Command(cont.Array);

                    await client.SendAsync(_serialiser.SerialiseRESP(cmdResult), 0);

                }
            }
            catch (NullReferenceException ex)
            {
                _logger.LogWarning("Client Disconnected - remove from list and close socket");
                _logger.LogWarning("{ex}", ex);
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
