using System;
using System.Reflection;

namespace DofusBatteriesIncluded.Plugins.Core.Deobfuscation.Protocol;

public class ObfuscatedMessage
{
    readonly object _message;
    readonly PropertyInfo _contentOneofCaseProperty;
    readonly PropertyInfo _contentProperty;
    readonly PropertyInfo _contentPointerProperty;

    public ObfuscatedMessage(object message, PropertyInfo contentOneofCaseProperty, PropertyInfo contentProperty, PropertyInfo contentPointerProperty)
    {
        _message = message;
        _contentOneofCaseProperty = contentOneofCaseProperty;
        _contentProperty = contentProperty;
        _contentPointerProperty = contentPointerProperty;
    }

    public ObfuscatedMessageContentOneOfCase ContentOneOfCase {
        get {
            object result = _contentOneofCaseProperty.GetValue(_message);
            string resultString = result?.ToString();
            return resultString == null ? ObfuscatedMessageContentOneOfCase.None : Enum.Parse<ObfuscatedMessageContentOneOfCase>(resultString);
        }
    }
    public IntPtr? ContentPointer {
        get {
            object content = _contentProperty.GetValue(_message);
            object contentPointer = _contentPointerProperty?.GetValue(content);
            return (IntPtr?)contentPointer;
        }
    }
}
