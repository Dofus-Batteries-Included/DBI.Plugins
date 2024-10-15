using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DofusBatteriesIncluded.Plugins.Core.Deobfuscation.Protocol;

public class ObfuscatedResponse
{
    readonly object _response;
    readonly PropertyInfo _contentOneofCaseProperty;
    readonly IReadOnlyCollection<PropertyInfo> _contentProperties;

    public ObfuscatedResponse(object response, PropertyInfo contentOneofCaseProperty, IReadOnlyCollection<PropertyInfo> contentProperties)
    {
        _response = response;
        _contentOneofCaseProperty = contentOneofCaseProperty;
        _contentProperties = contentProperties;
    }

    public ObfuscatedResponseContentOneOfCase GetContentOneOfCaseInResponse {
        get {
            object result = _contentOneofCaseProperty.GetValue(_response);
            string resultString = result?.ToString();
            return resultString == null ? ObfuscatedResponseContentOneOfCase.None : Enum.Parse<ObfuscatedResponseContentOneOfCase>(resultString);
        }
    }

    public object Identification => GetNonNullContentProperty();
    public object Pong => GetNonNullContentProperty();
    public object SelectServer => GetNonNullContentProperty();
    public object ForceAccount => GetNonNullContentProperty();
    public object FriendList => GetNonNullContentProperty();
    public object AcquaintanceServersResponse => GetNonNullContentProperty();

    object GetNonNullContentProperty() => _contentProperties.Select(p => p.GetValue(_response)).FirstOrDefault(r => r != null);
}
