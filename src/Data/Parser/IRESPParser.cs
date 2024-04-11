using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeronKV.Data.Parser
{
    internal interface IRESPParser
    {
        public RESPValue Parse(StringReader reader)
    }
}
