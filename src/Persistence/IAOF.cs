using HeronKV.Data;

namespace HeronKV.Persistence
{
    internal interface IAOF
    {
        public void Write(RESPValue value);
        public void Rebuild();
    }
}
