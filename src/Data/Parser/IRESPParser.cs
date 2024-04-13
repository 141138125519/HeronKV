namespace HeronKV.Data.Parser
{
    public interface IRESPParser
    {
        public RESPValue Parse(StringReader reader);
    }
}
