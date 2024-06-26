﻿using Microsoft.Extensions.Logging;

namespace HeronKV.Data.Parser
{
    public class RESPParser : IRESPParser
    {
        const char STRING = '+';
        const char ERROR = '-';
        const char INTEGER = ':';
        const char BULK = '$';
        const char ARRAY = '*';

        ILogger<IRESPParser> _logger;

        public RESPParser(ILogger<IRESPParser> logger)
        {
            _logger = logger;
        }

        public RESPValue Parse(StringReader reader)
        {
            return Read(reader);
        }

        private List<byte> ReadLine(StringReader reader)
        {
            int n = 0;
            var line = new List<byte>();

            while (true)
            {
                var b = reader.Read();
                if (b == '\r')
                {
                    reader.Read();
                    break;
                }
                line.Add((byte)b);
                n += 1;
            }

            return line;
        }

        private int ReadInteger(StringReader reader)
        {
            var line = ReadLine(reader);

            return int.Parse(line.ToArray());
        }

        private RESPValue Read(StringReader reader)
        {
            var _type = reader.Read();

            switch (_type)
            {
                case ARRAY:
                    return ReadArray(reader);
                case BULK:
                    return ReadBulk(reader);
                default:
                    Console.WriteLine($"Unkown Type: {_type}");
                    return new RESPValue();
            }
        }

        private RESPValue ReadArray(StringReader reader)
        {
            var value = new RESPValue
            {
                Type = "array",
                Array = []
            };

            var arrLen = ReadInteger(reader);

            for (var i = 0; i < arrLen; i++)
            {
                value.Array = [.. value.Array, Read(reader)];
            }

            return value;
        }

        private RESPValue ReadBulk(StringReader reader)
        {
            var value = new RESPValue
            {
                Type = "bulk"
            };

            var len = ReadInteger(reader);

            var bulk = new char[len];

            reader.Read(bulk, 0, len);

            value.Bulk = new string(bulk);

            ReadLine(reader);

            return value;
        }
    }
}
