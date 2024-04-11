using HeronKV.Data;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;
using HeronKV.Data.Serialiser;
using HeronKV.Data.Parser;
using HeronKV.CommandHandler;

namespace HeronKV.Persistence
{
    internal class AOF : BackgroundService, IAOF
    {
        private readonly ILogger<AOF> _logger;
        private readonly IRESPSerialiser _serialiser;
        private readonly IRESPParser _parser;
        private readonly ICommandsHandler _commandsHandler;

        private FileStream _stream;

        public AOF(ILogger<AOF> logger,
            IRESPSerialiser serialiser,
            IRESPParser parser,
            ICommandsHandler commandsHandler)
        {
            _logger = logger;
            _serialiser = serialiser;
            _parser = parser;
            _commandsHandler = commandsHandler;

            var path = "heron.aof";
            _stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogInformation("Sync file at: {time}", DateTimeOffset.Now);
                }

                Sync();
                await Task.Delay(1_000, stoppingToken);
            }

            _stream.Dispose();
        }

        private void Sync()
        {
            lock (_stream)
            {
                _stream.Flush();
            }
        }

        public void Write(RESPValue value)
        {
            lock (_stream)
            {
                _stream.Write(_serialiser.SerialiseRESP(value));
            }
        }

        public void Rebuild()
        {
            lock (_stream)
            {
                _stream.Seek(0, SeekOrigin.Begin);

                _logger.LogInformation("Rebuild Memory Store");

                var cmd = new List<byte>();

                int read;
                while ((read = _stream.ReadByte()) != -1)
                {
                    if ((byte)read == '*' && cmd.Count != 0)
                    {
                        var sr = new StringReader(Encoding.UTF8.GetString(cmd.ToArray()));

                        _ = _commandsHandler.Command(_parser.Parse(sr).Array!);
                        cmd.Clear();
                    }
                    cmd.Add((byte)read);
                }
            }
        }
    }
}
