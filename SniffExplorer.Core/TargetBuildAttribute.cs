using System;

namespace SniffExplorer.Core
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Struct, AllowMultiple = true)]
    public class TargetBuildAttribute : Attribute
    {
        public int Build { get; }

        public TargetBuildAttribute(int versionBuild)
        {
            Build = versionBuild;
        }
    }
}
