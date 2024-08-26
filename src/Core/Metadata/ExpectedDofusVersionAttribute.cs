using System;

namespace DofusBatteriesIncluded.Core.Metadata;

[AttributeUsage(AttributeTargets.Assembly)]
public sealed class ExpectedDofusVersionAttribute : Attribute
{
    public string Version { get; }

    public ExpectedDofusVersionAttribute(string version)
    {
        Version = version;
    }
}
