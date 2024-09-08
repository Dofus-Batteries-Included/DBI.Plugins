using System;

namespace DofusBatteriesIncluded.Plugins.Core.Metadata;

[AttributeUsage(AttributeTargets.Assembly)]
public sealed class ExpectedDofusVersionAttribute : Attribute
{
    public string Version { get; }

    public ExpectedDofusVersionAttribute(string version)
    {
        Version = version;
    }
}
