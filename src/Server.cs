﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Text;
using HeronKV.Data.Serialiser;
using HeronKV.Persistence;
using HeronKV.Data.Parser;
using HeronKV.CommandHandler;

namespace HeronKV
{
    internal class Server : BackgroundService
    {
        private readonly ILogger<Server> _logger;
        private readonly IRESPParser _parser;
        private readonly IRESPSerialiser _serialiser;
        private readonly ICommandsHandler _commandsHandler;
        private readonly IAOF _aof;

        IPHostEntry ipHostInfo;
        IPAddress ipAddress;
        IPEndPoint ipEndPoint;

        List<Socket> clients;
        List<Task> tasks;
        
        public Server(ILogger<Server> logger,
            IRESPParser parser,
            IRESPSerialiser serialiser,
            ICommandsHandler handler,
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
                        tasks.Add(Task.Run(() => ClientConnect(handler, cancellationToken), cancellationToken));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Exception: {ex}", ex);
                }
            }
        }

        /// <summary>
        /// Once a client has connected start this task to handle requests from that client
        /// </summary>
        /// <param name="client"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task ClientConnect(Socket client, CancellationToken cancellationToken)
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

                    // Don't like this.
                    // take data sent from client and turn into string
                    // this string is needed as currently RESPParse uses
                    // a StringReader <= this is the part I am not keen on.
                    var respString = Encoding.UTF8.GetString(buffer.ToArray());
                    // pass to parser
                    var sReader = new StringReader(respString);
                    var respValue = _parser.Parse(sReader);

                    if (respValue.Type != "array")
                    {
                        _logger.LogError("Invalid Request - expected array");
                    }
                    if (respValue.Array!.Length == 0)
                    {
                        _logger.LogError("Invalid Request - expected array length > 0");
                    }

                    respValue.Array[0].Bulk = respValue.Array[0].Bulk!.ToUpper();
                    
                    // Write any SET or HSET commands to aof data can be repopulated into memory on start up
                    if (respValue.Array[0].Bulk == "SET" || respValue.Array[0].Bulk == "HSET")
                    {
                        _aof.Write(respValue);
                    }
                    
                    var cmdResult = _commandsHandler.Command(respValue.Array);

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
