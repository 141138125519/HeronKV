namespace HeronKV.Data.Serialiser
{
    public interface IRESPSerialiser
    {
        public byte[] SerialiseRESP(RESPValue value);
    }
}
