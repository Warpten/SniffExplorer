using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SniffExplorer.Core.Events
{
    public static class EventDispatcher<T>
    {
        private static List<Action<T>> _handlers = new List<Action<T>>();

        public static void Dispatch(T element)
        {
            for (var i = 0; i < _handlers.Count; ++i)
                _handlers[i](element);
        }

        public static void Clear()
        {
            _handlers = null;
        }
    }
}
