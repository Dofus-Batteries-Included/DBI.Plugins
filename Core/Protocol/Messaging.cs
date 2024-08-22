using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Com.Ankama.Dofus.Server.Game.Protocol;
using Com.Ankama.Dofus.Server.Game.Protocol.Treasure.Hunt;
using Google.Protobuf;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Microsoft.Extensions.Logging;

namespace DofusBatteriesIncluded.Core.Protocol;

public static class Messaging
{
    static readonly ILogger Log = DBI.Logging.Create(typeof(Messaging));

    public static event EventHandler<object> MessageReceived;

    [HarmonyPatch(typeof(MessageParser), nameof(MessageParser.ParseFrom), typeof(CodedInputStream))]
    [HarmonyPostfix]
    static void Patch(IMessage __result)
    {
        Message message = new(__result.Pointer);
        string json = message.ToString();
        JsonDocument obj = JsonDocument.Parse(json);

        if (obj.RootElement.TryGetProperty("event", out JsonElement eventElement))
        {
            HandleEventMessage(eventElement);
        }
        else
        {
            string indented = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
            Log.LogInformation("Message: {Message}", indented);
        }
    }

    static void HandleEventMessage(JsonElement eventElement)
    {
        const string mergeFromMethodName = "MergeFrom";

        if (!eventElement.TryGetProperty("content", out JsonElement contentElement))
        {
            return;
        }

        string typeName = contentElement.TryGetProperty("@type", out JsonElement vType) ? vType.GetString() : null;
        string content = contentElement.TryGetProperty("@value", out JsonElement vContent) ? vContent.GetString() : null;

        Type type = FindEventType(typeName);

        if (type == null)
        {
            Log.LogWarning("Could not find type for message of @type: {Type}", typeName);
            return;
        }

        object instance = Activator.CreateInstance(type);

        if (instance == null)
        {
            Log.LogError("Could not instantiate value of type: {Type}", type);
            return;
        }

        MethodInfo mergeFromMethod = type.GetMethod(mergeFromMethodName, BindingFlags.Public | BindingFlags.Instance, [typeof(CodedInputStream)]);
        if (mergeFromMethod == null)
        {
            Log.LogError("Could not find {MethodName} method in event type: {Type}", mergeFromMethodName, type);
            return;
        }

        byte[] contentBytes = content != null ? Convert.FromBase64String(content) : [];
        CodedInputStream stream = new(new Il2CppStructArray<byte>(contentBytes));
        try
        {
            mergeFromMethod.Invoke(instance, [stream]);
        }
        finally
        {
            stream.Dispose();
        }

        Log.LogDebug("Received message of type {Type}", type);
        MessageReceived?.Invoke(null, instance);
    }

    static Type FindEventType(string typeName)
    {
        const string gameProtocolTypePrefix = "type.ankama.com/com.ankama.dofus.server.game.protocol";

        if (typeName.StartsWith(gameProtocolTypePrefix))
        {
            Assembly assembly = typeof(TreasureHuntEvent).Assembly;
            string typeShortName = typeName.Split(".").Last();
            Type type = assembly.GetTypes().SingleOrDefault(t => t.Name == typeShortName);
            return type;
        }

        return null;
    }
}
