using Microsoft.Extensions.Logging;
using HeronKV.Data.Parser;
using Moq;
using HeronKV.Data;

namespace Test
{
    public class RESPParserTests
    {
        private IRESPParser _parser;

        public RESPParserTests()
        {
            var logger = Mock.Of<ILogger<IRESPParser>>();

            _parser = new RESPParser(logger);
        }

        [Fact]
        public void NotNullTest()
        {
            Assert.NotNull( _parser );
        }

        [Fact]
        public void ArrayTest()
        {
            var input = "*1\r\n$4\r\nping\r\n";
            var sr = new StringReader( input );

            var result = _parser.Parse(sr);

            Assert.NotNull( result );

            var expected = new RESPValue { Type = "array", Array = [new RESPValue { Type = "bulk", Bulk = "ping" }] };

            Assert.Equivalent(expected, result);
        }

        [Fact]
        public void ArrayTest2()
        {
            var input = "*2\r\n$4\r\nping\r\n$5\r\nhello\r\n";
            var sr = new StringReader(input);

            var result = _parser.Parse(sr);

            Assert.NotNull( result );

            var expected = new RESPValue { Type = "array", Array = [new RESPValue { Type = "bulk", Bulk = "ping" }, new RESPValue { Type = "bulk", Bulk = "hello" }] };

            Assert.Equivalent(expected, result);
        }

        [Fact]
        public void BulkTest()
        {
            var input = "$4\r\nping\r\n";
            var sr = new StringReader(input);

            var result = _parser.Parse(sr);

            Assert.NotNull( result );

            var expected = new RESPValue { Type = "bulk", Bulk = "ping" };

            Assert.Equivalent(expected, result);
        }
    }
}
