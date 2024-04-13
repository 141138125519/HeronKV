using HeronKV.Data;

namespace HeronKV.CommandHandler
{
    public interface ICommandsHandler
    {
        public RESPValue Command(RESPValue[] args);
    }
}
