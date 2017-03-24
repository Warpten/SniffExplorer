using System;
using System.Reflection;

namespace SniffExplorer.Core.Utils
{
    public static class MiscExtensions
    {
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
