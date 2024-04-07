﻿using System.Reflection.PortableExecutable;
using System.Threading.Tasks.Dataflow;
using KVDB.Data;
using Microsoft.Extensions.Logging;

namespace KVDB
{
    internal class RESPParser
    {
        const char STRING = '+';
        const char ERROR = '-';
        const char INTEGER = ':';
        const char BULK = '$';
        const char ARRAY = '*';


        //ILogger<RESPParser> _logger;
        StringReader _reader;

        public RESPParser(StringReader reader)
        {
            //_logger = logger;
            _reader = reader;
        }

        private List<byte> ReadLine()
        {
            int n = 0;
            var line = new List<byte>();

            while (true)
            {
                var b = _reader.Read();
                if (b == '\r')
                {
                    _reader.Read();
                    break;
                }
                line.Add((byte)b);
                n += 1;
            }

            return line;
        }

        private int ReadInteger()
        {
            var line = ReadLine();

            return int.Parse(line.ToArray());
        }

        public Value Read()
        {
            var _type = _reader.Read();

            switch (_type)
            {
                case ARRAY:
                    return ReadArray();
                case BULK:
                    return ReadBulk();
                default:
                    Console.WriteLine($"Unkown Type: {_type}");
                    return new Value();
            }
        }

        private Value ReadArray()
        {
            var value = new Value
            {
                Type = "array",
                Array = []
            };

            var arrLen = ReadInteger();

            for (var i = 0; i < arrLen; i++)
            {
                value.Array = value.Array.Append(Read()).ToArray();
                //value.Array[i] = Read();
            }

            return value;
        }

        private Value ReadBulk()
        {
            var value = new Value
            {
                Type = "bulk"
            };

            var len = ReadInteger();

            var bulk = new char[len];

            _reader.Read(bulk, 0, len);

            value.Bulk = new string(bulk);

            ReadLine();

            return value;
        }
    }
}
