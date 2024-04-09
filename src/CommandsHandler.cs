using HeronKV.Data;

namespace HeronKV
{
    internal class CommandsHandler
    {
        public CommandsHandler()
        {

        }

        public RESPValue Command(RESPValue[] args)
        {
            return args[0].Bulk switch
            {
                "PING" => Ping(args),
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
    }
}
