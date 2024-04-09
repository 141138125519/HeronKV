using HeronKV.Data;
using Microsoft.Extensions.Logging;

namespace HeronKV
{
    internal class CommandsHandler
    {
        private readonly ILogger<CommandsHandler> _logger;
        private Dictionary<string, string> map;

        public CommandsHandler(ILogger<CommandsHandler> logger)
        {
            _logger = logger;
            map = [];
        }

        public RESPValue Command(RESPValue[] args)
        {
            return args[0].Bulk switch
            {
                "PING" => Ping(args),
                "SET" => Set(args),
                "GET" => Get(args),
                _ => new RESPValue { Type = "string", Str = "" },
            };
        }

        private RESPValue Ping(RESPValue[] args)
        {
            if (args.Length <= 1)
            {
                return new RESPValue { Type = "string", Str = "PONG" };
            }

            return new RESPValue { Type = "string", Str = args[1].Bulk };
        }

        private RESPValue Set(RESPValue[] args)
        {
            if (args.Length < 3)
            {
                return new RESPValue { Type = "error", Str = "ERR too few arguments for SET command" };
            }

            var key = args[1].Bulk!;
            var value = args[2].Bulk!;

            lock (map)
            {
                map[key] = value;
            }

            return new RESPValue { Type = "string", Str = "OK" };
        }

        private RESPValue Get(RESPValue[] args)
        {
            if (args.Length != 2)
            {
                return new RESPValue { Type = "error", Str = "ERR wrong number of arguments for GET command" };
            }

            var key = args[1].Bulk!;

            var ok = false;
            string? value;


            lock (map)
            {
                ok = map.TryGetValue(key, out value);
            }

            if (!ok)
            {
                return new RESPValue { Type = "null" };
            }

            return new RESPValue { Type = "bulk", Bulk = value };
        }
    }
}
