﻿using HeronKV;
using HeronKV.Data;
using Microsoft.Extensions.Logging;
using System.Text;

namespace src
{
    internal class RESPWriter
    {
        const char STRING = '+';
        const char ERROR = '-';
        const char INTEGER = ':';
        const char BULK = '$';
        const char ARRAY = '*';

        ILogger<RESPWriter> _logger;

        public RESPWriter(ILogger<RESPWriter> logger)
        {
            _logger = logger;
        }

        public void WriteRESP(Value value)
        {
            var bytes = Marshal(value);

            // write bytes
            _logger.LogInformation(Encoding.UTF8.GetString(bytes.ToArray()));

        }

        private List<byte> Marshal(Value value)
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

        private List<byte> MarshalArray(Value value)
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

        private List<byte> MarshalBulk(Value value)
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

        private List<byte> MarshalString(Value value)
        {
            var bytes = new List<byte>
            {
                (byte)STRING
            };
            bytes.AddRange(Encoding.UTF8.GetBytes(value.Str!));
            bytes.AddRange([(byte)'\r', (byte)'\n']);

            return bytes;
        }

        private List<byte> MarshalNull(Value value)
        {
            return [.. Encoding.UTF8.GetBytes("$-1\r\n")];
        }

        private List<byte> MarshalError(Value value)
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
