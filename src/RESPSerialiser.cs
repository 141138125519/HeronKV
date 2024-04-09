using HeronKV;
using HeronKV.Data;
using Microsoft.Extensions.Logging;
using System.Text;

namespace src
{
    internal class RESPSerialiser
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

        public byte[] WriteRESP(RESPValue value)
        {
            var bytes = Marshal(value);

            // write bytes
            _logger.LogInformation(Encoding.UTF8.GetString(bytes.ToArray()));
            return [.. bytes];
        }

        private List<byte> Marshal(RESPValue value)
        {
            return value.Type switch
            {
                "array" => MarshalArray(value),
                "bulk" => MarshalBulk(value),
                "string" => MarshalString(value),
                "null" => MarshalNull(value),
                "error" => MarshalError(value),
                _ => [],
            };
        }

        private List<byte> MarshalArray(RESPValue value)
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
                bytes.AddRange(Marshal(value.Array[i]));
            }

            return bytes;
        }

        private List<byte> MarshalBulk(RESPValue value)
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

        private List<byte> MarshalString(RESPValue value)
        {
            var bytes = new List<byte>
            {
                (byte)STRING
            };
            bytes.AddRange(Encoding.UTF8.GetBytes(value.Str!));
            bytes.AddRange([(byte)'\r', (byte)'\n']);

            return bytes;
        }

        private List<byte> MarshalNull(RESPValue value)
        {
            return [.. Encoding.UTF8.GetBytes("$-1\r\n")];
        }

        private List<byte> MarshalError(RESPValue value)
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
