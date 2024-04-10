using HeronKV.Data;
using Microsoft.Extensions.Logging;

namespace HeronKV
{
    internal class CommandsHandler
    {
        private readonly ILogger<CommandsHandler> _logger;
        private Dictionary<string, string> data; // for SET GET
        private Dictionary<string, Dictionary<string, string>> hData; // for HSET HGET

        public CommandsHandler(ILogger<CommandsHandler> logger)
        {
            _logger = logger;
            data = [];
            hData = [];
        }

        public RESPValue Command(RESPValue[] args)
        {
            return args[0].Bulk switch
            {
                "PING" => Ping(args),
                "SET" => Set(args),
                "GET" => Get(args),
                "HSET" => HSet(args),
                "HGET" => HGet(args),
                "HGETALL" => HGetAll(args),
                _ => new RESPValue { Type = "error", Str = "Unkown Command" },
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

            lock (data)
            {
                data[key] = value;
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


            lock (data)
            {
                ok = data.TryGetValue(key, out value);
            }

            if (!ok)
            {
                return new RESPValue { Type = "null" };
            }

            return new RESPValue { Type = "bulk", Bulk = value };
        }

        private RESPValue HSet(RESPValue[] args)
        {
            if (args.Length != 4)
            {
                return new RESPValue { Type = "error", Str = "ERR wrong number of arguments for HSET command" };
            }

            var hash = args[1].Bulk!;
            var key = args[2].Bulk!;
            var value = args[3].Bulk!;

            lock (hData)
            {
                if (!hData.ContainsKey(hash))
                {
                    hData[hash] = [];
                }

                hData[hash][key] = value;
            }

            return new RESPValue { Type = "string", Str = "OK" };
        }

        private RESPValue HGet(RESPValue[] args)
        {
            if (args.Length != 3)
            {
                return new RESPValue { Type = "error", Str = "ERR wrong number of arguments for HGET command" };
            }

            var hash = args[1].Bulk!;
            var key = args[2].Bulk!;

            var ok = false;
            string? value;

            lock (hData)
            {
                if (!hData.TryGetValue(hash, out var hashDict))
                    {
                    return new RESPValue { Type = "null" };
                }

                ok = hashDict.TryGetValue(key, out value);
            }

            if (!ok)
            {
                return new RESPValue { Type = "null" };
            }

            return new RESPValue { Type = "bulk", Bulk = value };
        }

        private RESPValue HGetAll(RESPValue[] args)
        {
            if (args.Length != 2)
            {
                return new RESPValue { Type = "error", Str = "ERR wrong number of arguments for HGETALL command" };
            }

            var hash = args[1].Bulk!;

            var value = new List<string>();

            lock (hData)
            {
                if (!hData.TryGetValue(hash, out Dictionary<string, string>? hashDict))
                {
                    return new RESPValue { Type = "null" };
                }

                value = [.. hashDict.Values];
            }
            
            var array = new List<RESPValue>();
            foreach (var item in value)
            {
                array.Add(new RESPValue { Type = "bulk", Bulk = item });
            }

            return new RESPValue { Type = "array", Array = [.. array] };
        }
    }
}
