using System;
using System.Reflection;
using System.Windows.Forms;

namespace SniffExplorer.Utils
{
    public static class MiscExtensions
    {
        public static void InvokeIfRequired(this Form obj, MethodInvoker action)
        {
            if (obj.InvokeRequired)
            {
                var args = new object[0];
                obj.Invoke(action, args);
            }
            else
            {
                action();
            }
        }

        public static MethodInfo GetMethod(this Type type, string methodName, Type types)
        {
            return type.GetMethod(methodName, new[] { types });
        }

        public static MethodInfo GetMethod(this Type type, string methodName, params Type[] types)
        {
            return type.GetMethod(methodName, types);
        }
    }
}
