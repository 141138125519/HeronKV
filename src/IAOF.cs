using HeronKV.Data;

namespace HeronKV
{
    internal interface IAOF
    {
        public void Write(RESPValue value);
        public void Rebuild();
    }
}
