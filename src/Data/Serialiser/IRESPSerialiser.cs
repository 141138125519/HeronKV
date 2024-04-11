namespace HeronKV.Data.Serialiser
{
    internal interface IRESPSerialiser
    {
        public byte[] SerialiseRESP(RESPValue value);
    }
}
