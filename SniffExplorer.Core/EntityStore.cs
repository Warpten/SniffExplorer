using SniffExplorer.Core.Packets.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SniffExplorer.Core
{
    public static class EntityStore
    {
        private static Dictionary<IObjectGuid, Entity> _nodes = new Dictionary<IObjectGuid, Entity>();
    }
}
