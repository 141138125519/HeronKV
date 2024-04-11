using HeronKV.Data;

namespace HeronKV.CommandHandler
{
    internal interface ICommandsHandler
    {
        public RESPValue Command(RESPValue[] args);
    }
}
