using Microsoft.Extensions.Logging;
using System.Text;

namespace HeronKV.Data.Serialiser
{
    internal class RESPSerialiser : IRESPSerialiser
    {
        const char STRING = '+';
        const char ERROR = '-';
        const char INTEGER = ':';
        const char BULK = '$';
        const char ARRAY = '*';

        ILogger<RESPSerialiser> _logger;

        public RESPSerialiser(ILogger<RESPSerialiser> logger)
        {
            _logger = logger;
        }

        public byte[] SerialiseRESP(RESPValue value)
        {
            var bytes = Serialise(value);

            return [.. bytes];
        }

        private List<byte> Serialise(RESPValue value)
        {
            return value.Type switch
            {
                "array" => SerialiseArray(value),
                "bulk" => SerialiseBulk(value),
                "string" => SerialiseString(value),
                "null" => SerialiseNull(value),
                "error" => SerialiseError(value),
                _ => [],
            };
        }

        private List<byte> SerialiseArray(RESPValue value)
        {
            var length = value.Array!.Length;
            _logger.LogWarning(length.ToString());
            var bytes = new List<byte>
            {
                (byte)ARRAY
            };
            bytes.AddRange(Encoding.UTF8.GetBytes(length.ToString()));
            bytes.AddRange([(byte)'\r', (byte)'\n']);
            _logger.LogWarning($"array {Encoding.UTF8.GetString(bytes.ToArray())}");
            for (int i = 0; i < length; i++)
            {
                bytes.AddRange(Serialise(value.Array[i]));
            }

            return bytes;
        }

        private List<byte> SerialiseBulk(RESPValue value)
        {
            var bytes = new List<byte>
            {
                (byte)BULK
            };
            bytes.AddRange(Encoding.UTF8.GetBytes(value.Bulk!.Length.ToString()));
            bytes.AddRange([(byte)'\r', (byte)'\n']);
            bytes.AddRange(Encoding.UTF8.GetBytes(value.Bulk));
            bytes.AddRange([(byte)'\r', (byte)'\n']);

            return bytes;
        }

        private List<byte> SerialiseString(RESPValue value)
        {
            var bytes = new List<byte>
            {
                (byte)STRING
            };
            bytes.AddRange(Encoding.UTF8.GetBytes(value.Str!));
            bytes.AddRange([(byte)'\r', (byte)'\n']);

            return bytes;
        }

        private List<byte> SerialiseNull(RESPValue value)
        {
            return [.. Encoding.UTF8.GetBytes("$-1\r\n")];
        }

        private List<byte> SerialiseError(RESPValue value)
        {
            var bytes = new List<byte>
            {
                (byte)ERROR
            };
            bytes.AddRange(Encoding.UTF8.GetBytes(value.Str!));
            bytes.AddRange([(byte)'\r', (byte)'\n']);

            return bytes;
        }
    }
}
