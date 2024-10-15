using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DofusBatteriesIncluded.Plugins.Core.Extensions;

namespace DofusBatteriesIncluded.Plugins.Core.Deobfuscation.Protocol;

class ObfuscatedResponseReader
{
    const string ConnectionProtocolAssemblyName = "Ankama.Dofus.Protocol.Connection";
    const string ConnectionProtocolAssemblyPath = $"BepInEx/interop/{ConnectionProtocolAssemblyName}.dll";

    readonly Type _obfuscatedResponseType;
    readonly PropertyInfo _contentOneofCaseProperty;
    readonly IReadOnlyCollection<PropertyInfo> _contentProperties;

    public ObfuscatedResponseReader()
    {
        Assembly connectionAssembly = AppDomain.CurrentDomain.LoadAssemblyIfNotLoaded(ConnectionProtocolAssemblyName, ConnectionProtocolAssemblyPath);

        Type contentOneOfCaseNestedInResponseType = FindContentOneOfCaseTypeNestedInResponseType(connectionAssembly);
        _obfuscatedResponseType = contentOneOfCaseNestedInResponseType.DeclaringType!;
        _contentOneofCaseProperty = _obfuscatedResponseType.GetProperties().First(p => p.PropertyType == contentOneOfCaseNestedInResponseType);
        _contentProperties = _obfuscatedResponseType.GetProperties()
            .Where(p => p.PropertyType.Assembly == connectionAssembly && p.PropertyType != contentOneOfCaseNestedInResponseType)
            .ToArray();
    }

    public ObfuscatedResponse GetResponse(IntPtr pointer)
    {
        object responseInstance = Activator.CreateInstance(_obfuscatedResponseType, pointer);
        return new ObfuscatedResponse(responseInstance, _contentOneofCaseProperty, _contentProperties);
    }

    static Type FindContentOneOfCaseTypeNestedInResponseType(Assembly assembly) =>
        assembly.GetTypes()
            .Where(t => t.IsEnum && t.DeclaringType != null)
            .Single(
                t =>
                {
                    // Find the enum containing values None, Event, Request, Response: it is the ObfuscatedMessageContentOneofCase enum defined by the Message class
                    string[] values = t.GetEnumValues().Cast<object>().Select(e => e.ToString()).ToArray();
                    return values.Length == 7
                           && values.Contains("None")
                           && values.Contains("Identification")
                           && values.Contains("Pong")
                           && values.Contains("SelectServer")
                           && values.Contains("ForceAccount")
                           && values.Contains("FriendList")
                           && values.Contains("AcquaintanceServersResponse");
                }
            );
}
