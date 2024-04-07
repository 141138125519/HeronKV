using System.Reflection.PortableExecutable;
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

        private byte[] ReadLine(int size)
        {
            int n = 0;
            var line = new byte[size];

            while (true)
            {
                var b = _reader.Read();
                if (b == '\r')
                    break;
                line[n] = (byte)b;
                n += 1;
            }

            return line;
        }

        private int ReadInteger()
        {
            var line = ReadLine(1);

            return int.Parse(line);
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
                    //_logger.LogWarning("Unkown Type: {type}", _type.ToString());
                    Console.WriteLine($"Unkown Type: {_type.ToString()}");
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
                value.Array.Append(Read());
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

            ReadLine(2);

            return value;
        }
    }
}
