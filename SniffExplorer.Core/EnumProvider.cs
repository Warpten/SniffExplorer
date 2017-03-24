using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SniffExplorer.Core
{
    public static class EnumProvider
    {
        public static IEnumerable<string> GetClientOpcodes(Assembly assembly, uint targetBuild)
        {
            return (from type in assembly.GetTypes()
                    where type.IsEnum
                    let buildAttr = type.GetCustomAttributes<OpcodeAttribute>()
                        where buildAttr.Any(attr => attr.TargetBuilds.Contains(targetBuild) && attr.Direction == 0x47534D43u)
                    select Enum.GetNames(type)).FirstOrDefault();
        }

        public static IEnumerable<string> GetServerOpcodes(Assembly assembly, uint targetBuild)
        {
            return (from type in assembly.GetTypes()
                    where type.IsEnum
                    let buildAttr = type.GetCustomAttributes<OpcodeAttribute>()
                        where buildAttr.Any(attr => attr.TargetBuilds.Contains(targetBuild) && attr.Direction == 0x47534D53u)
                    select Enum.GetNames(type)).FirstOrDefault();
        }

        public static IEnumerable<string> GetOpcodes(Assembly assembly, uint targetBuild)
        {
            return from type in assembly.GetTypes()
                   where type.IsEnum
                   let buildAttr = type.GetCustomAttributes<OpcodeAttribute>()
                       where buildAttr.Any(attr => attr.TargetBuilds.Contains(targetBuild))
                   from s in Enum.GetNames(type) select s;
        }
    }
}
