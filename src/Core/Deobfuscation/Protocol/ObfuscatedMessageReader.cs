using System;
using System.Linq;
using System.Reflection;
using DofusBatteriesIncluded.Plugins.Core.Extensions;
using Google.Protobuf;
using Il2CppSystem.Threading;
using Object = Il2CppSystem.Object;

namespace DofusBatteriesIncluded.Plugins.Core.Deobfuscation.Protocol;

class ObfuscatedMessageReader
{
    const string GameProtocolAssemblyName = "Ankama.Dofus.Protocol.Game";
    const string GameProtocolAssemblyPath = $"BepInEx/interop/{GameProtocolAssemblyName}.dll";

    readonly Type _obfuscatedMessageType;
    readonly PropertyInfo _contentOneofCaseProperty;
    readonly PropertyInfo _contentProperty;
    readonly PropertyInfo _contentPointerProperty;

    public ObfuscatedMessageReader()
    {
        Thread.Sleep(5000);

        Assembly gameAssembly = AppDomain.CurrentDomain.LoadAssemblyIfNotLoaded(GameProtocolAssemblyName, GameProtocolAssemblyPath);

        Type contentOneOfCaseNestedInMessageType = FindContentOneOfCaseTypeNestedInMessageType(gameAssembly);
        _obfuscatedMessageType = contentOneOfCaseNestedInMessageType.DeclaringType!;
        _contentOneofCaseProperty = _obfuscatedMessageType.GetProperties().First(p => p.PropertyType == contentOneOfCaseNestedInMessageType);
        _contentProperty = _obfuscatedMessageType.GetProperties().First(p => p.PropertyType == typeof(Object));
        _contentPointerProperty = _contentProperty.PropertyType.GetProperty("Pointer");
    }

    public ObfuscatedMessage GetMessage(IMessage message)
    {
        object messageInstance = Activator.CreateInstance(_obfuscatedMessageType, message.Pointer);
        return new ObfuscatedMessage(messageInstance, _contentOneofCaseProperty, _contentProperty, _contentPointerProperty);
    }

    static Type FindContentOneOfCaseTypeNestedInMessageType(Assembly assembly) =>
        assembly.GetTypes()
            .Where(t => t.IsEnum && t.DeclaringType != null)
            .Single(
                t =>
                {
                    // Find the enum containing values None, Event, Request, Response: it is the ObfuscatedMessageContentOneofCase enum defined by the Message class
                    string[] values = t.GetEnumValues().Cast<object>().Select(e => e.ToString()).ToArray();
                    return values.Length == 4 && values.Contains("None") && values.Contains("Event") && values.Contains("Response") && values.Contains("Request");
                }
            );
}
