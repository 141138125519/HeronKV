namespace HeronKV.Data.Parser
{
    internal interface IRESPParser
    {
        public RESPValue Parse(StringReader reader);
    }
}
