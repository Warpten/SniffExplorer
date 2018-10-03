using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SniffExplorer.Core.Packets.Parsing
{
    public interface IOpcodeProvider
    {
        Opcodes ValueToOpcode(uint value, PacketDirection direction);
        uint OpcodeToValue(Opcodes opcode, PacketDirection direction);
    }
}
