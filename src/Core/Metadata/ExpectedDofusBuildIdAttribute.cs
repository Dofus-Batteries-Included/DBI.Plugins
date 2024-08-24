using System;

namespace DofusBatteriesIncluded.Core.Metadata;

[AttributeUsage(AttributeTargets.Assembly)]
public sealed class ExpectedDofusBuildIdAttribute : Attribute
{
    public Guid? BuildId { get; }

    public ExpectedDofusBuildIdAttribute(string id)
    {
        BuildId = Guid.TryParse(id, out Guid guid) ? guid : null;
    }
}
