using Microsoft.Extensions.Logging;
using HeronKV.Data.Serialiser;
using Moq;
using HeronKV.Data;
using System.Text;

namespace Test
{
    public class RESPSerialiserTests
    {
        private IRESPSerialiser _serialiser;

        public RESPSerialiserTests()
        {
            var logger = Mock.Of<ILogger<RESPSerialiser>>();

            _serialiser = new RESPSerialiser(logger);
        }

        [Fact]
        public void NotNullTest()
        {
            Assert.NotNull( _serialiser );
        }

        [Fact]
        public void SerialiseArrayTest()
        {
            var input = new RESPValue { Type = "array", Array = new RESPValue[] { new RESPValue { Type = "bulk", Bulk = "ping" } } };

            var result = _serialiser.SerialiseRESP( input );

            Assert.NotNull( result );

            var expected = Encoding.UTF8.GetBytes("*1\r\n$4\r\nping\r\n");

            Assert.Equal( expected, result );
        }

        [Fact]
        public void SerialiseBulkTest()
        {
            var input = new RESPValue { Type = "bulk", Bulk = "ping" };

            var result = _serialiser.SerialiseRESP(input);

            Assert.NotNull( result );

            var expected = Encoding.UTF8.GetBytes("$4\r\nping\r\n");

            Assert.Equal( expected, result );
        }

        [Fact]
        public void SerialiseStringTest()
        {
            var input = new RESPValue { Type = "string", Str = "OK" };

            var result = _serialiser.SerialiseRESP(input);

            Assert.NotNull(result);

            var expected = Encoding.UTF8.GetBytes("+OK\r\n");

            Assert.Equal(expected, result);
        }

        [Fact]
        public void SerialiseNullTest()
        {
            var input = new RESPValue { Type = "null" };

            var result = _serialiser.SerialiseRESP(input);

            Assert.NotNull( result );

            var expected = Encoding.UTF8.GetBytes("$-1\r\n");

            Assert.Equal( expected, result );
        }

        [Fact]
        public void SerialiseErrorTest()
        {
            var input = new RESPValue { Type = "error", Str = "error" };

            var result = _serialiser.SerialiseRESP(input);

            Assert.NotNull( result );

            var expected = Encoding.UTF8.GetBytes("-error\r\n");

            Assert.Equal( expected, result );
        }
    }
}
