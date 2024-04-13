using Microsoft.Extensions.Logging;
using HeronKV.CommandHandler;
using Moq;
using HeronKV.Data;

namespace Test
{
    public class CommandsHandlerTests
    {
        private ICommandsHandler _cmdHandler;

        public CommandsHandlerTests()
        {
            var logger = Mock.Of<ILogger<ICommandsHandler>>();

            _cmdHandler = new CommandsHandler(logger);
        }

        [Fact]
        public void NotNullTest()
        {
            Assert.NotNull( _cmdHandler );
        }

        [Fact]
        public void PingTest()
        {
            var input = new RESPValue[] { new RESPValue { Type = "bulk", Bulk = "PING" } };
            var result = _cmdHandler.Command( input );

            Assert.NotNull( result );

            var expected = new RESPValue { Type = "string", Str = "PONG" };

            Assert.Equivalent( expected, result );
        }

        [Fact]
        public void PingTest2()
        {
            var input = new RESPValue[] { new RESPValue { Type = "bulk", Bulk = "PING" }, new RESPValue { Type = "bulk", Bulk = "hello" } };
            var result = _cmdHandler.Command(input);

            Assert.NotNull(result);

            var expected = new RESPValue { Type = "string", Str = "hello" };

            Assert.Equivalent(expected, result);
        }

        [Fact]
        public void CommandTest()
        {
            var input = new RESPValue[] { new RESPValue { Type = "bulk", Bulk = "COMMAND" } };
            var result = _cmdHandler.Command(input);

            Assert.NotNull(result);

            var expected = new RESPValue
            {
                Type = "array",
                Array = [
                    new() { Type = "bulk", Bulk = "ping" },
                    new() { Type = "bulk", Bulk = "command" },
                    new() { Type = "bulk", Bulk = "set" },
                    new() { Type = "bulk", Bulk = "get" },
                    new() { Type = "bulk", Bulk = "hset" },
                    new() { Type = "bulk", Bulk = "hget" },
                    new() { Type = "bulk", Bulk = "hgetall" }
                ]
            };

            Assert.Equivalent(expected, result);
        }

        [Fact]
        public void SetAndGetTest()
        {
            var input = new RESPValue[]
            { 
                new() { Type = "bulk", Bulk = "SET" },
                new() { Type = "bulk", Bulk = "key" },
                new() { Type = "bulk", Bulk = "value" }
            };
            var result = _cmdHandler.Command(input);

            Assert.NotNull(result);

            var expected = new RESPValue { Type = "string", Str = "OK" };

            Assert.Equivalent(expected, result);

            // now get
            var inputGet = new RESPValue[]
            {
                new() { Type = "bulk", Bulk = "GET" },
                new() { Type = "bulk", Bulk = "key" }
            };
            var resultGet = _cmdHandler.Command(inputGet);

            Assert.NotNull(resultGet);

            var expectedGet = new RESPValue { Type = "bulk", Bulk = "value" };

            Assert.Equivalent(expectedGet, resultGet);
        }

        [Fact]
        public void HSetAndHGetAndHGetAllTest()
        {
            var input = new RESPValue[]
            {
                new() { Type = "bulk", Bulk = "HSET" },
                new() { Type = "bulk", Bulk = "group" },
                new() { Type = "bulk", Bulk = "key" },
                new() { Type = "bulk", Bulk = "value" }
            };
            var result = _cmdHandler.Command(input);

            Assert.NotNull(result);

            var expected = new RESPValue { Type = "string", Str = "OK" };

            Assert.Equivalent(expected, result);

            // now hget
            var inputHGet = new RESPValue[]
            {
                new() { Type = "bulk", Bulk = "HGET" },
                new() { Type = "bulk", Bulk = "group" },
                new() { Type = "bulk", Bulk = "key" }
            };
            var resultHGet = _cmdHandler.Command(inputHGet);

            Assert.NotNull(resultHGet);

            var expectedHGet = new RESPValue { Type = "bulk", Bulk = "value" };

            Assert.Equivalent(expectedHGet, resultHGet);

            // now hgetall
            var inputHGetall = new RESPValue[]
            {
                new() { Type = "bulk", Bulk = "HGETALL" },
                new() { Type = "bulk", Bulk = "group" }
            };
            var resultHGetall = _cmdHandler.Command(inputHGetall);

            Assert.NotNull(resultHGetall);

            var expectedHGetall = new RESPValue { Type = "array", Array = [new() { Type = "bulk", Bulk = "value" }]};

            Assert.Equivalent(expectedHGetall, resultHGetall);
        }
    }
}
