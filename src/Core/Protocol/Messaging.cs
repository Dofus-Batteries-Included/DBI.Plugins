using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using DofusBatteriesIncluded.Plugins.Core.Deobfuscation.Protocol;
using Google.Protobuf;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Microsoft.Extensions.Logging;

namespace DofusBatteriesIncluded.Plugins.Core.Protocol;

public static class Messaging
{
    static readonly ILogger Log = DBI.Logging.Create(typeof(Messaging));

    public static event EventHandler<object> MessageReceived;

    static readonly ObfuscatedMessageReader MessageReader = new();
    static readonly ObfuscatedResponseReader ResponseReader = new();

    [HarmonyPatch(typeof(MessageParser), nameof(MessageParser.ParseFrom), typeof(CodedInputStream))]
    [HarmonyPostfix]
    static void Patch(IMessage __result)
    {
        ObfuscatedMessage message = MessageReader.GetMessage(__result);

        switch (message.ContentOneOfCase)
        {
            case ObfuscatedMessageContentOneOfCase.Event:
            {
                string json = message.ToString();
                JsonDocument obj = JsonDocument.Parse(json);

                if (obj.RootElement.TryGetProperty("event", out JsonElement eventElement))
                {
                    HandleEventMessage(eventElement);
                }
                else
                {
                    Log.LogWarning($"Invalid event:\n{json}");
                }
                break;
            }
            case ObfuscatedMessageContentOneOfCase.Response:
            {
                HandleResponseMessage(message);
                break;
            }
            case ObfuscatedMessageContentOneOfCase.None:
            case ObfuscatedMessageContentOneOfCase.Request:
            default:
            {
                string json = message.ToString();
                JsonDocument obj = JsonDocument.Parse(json);
                string indented = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
                Log.LogInformation("Message: {Message}", indented);
                break;
            }
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

        Log.LogDebug("Received event of type {Type}.", type);
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

    static void HandleResponseMessage(ObfuscatedMessage message)
    {
        IntPtr? pointer = message.ContentPointer;
        if (!pointer.HasValue)
        {
            Log.LogError("Could not find content of response message.\n{Response}.", message);
            return;
        }

        ObfuscatedResponse response = ResponseReader.GetResponse(pointer.Value);
        if (response == null)
        {
            Log.LogError("Could not read response.\n{Response}", message);
            return;
        }

        object content;
        switch (response.GetContentOneOfCaseInResponse)
        {
            case ObfuscatedResponseContentOneOfCase.Identification:
                content = response.Identification;
                break;
            case ObfuscatedResponseContentOneOfCase.Pong:
                content = response.Pong;
                break;
            case ObfuscatedResponseContentOneOfCase.SelectServer:
                content = response.SelectServer;
                break;
            case ObfuscatedResponseContentOneOfCase.ForceAccount:
                content = response.ForceAccount;
                break;
            case ObfuscatedResponseContentOneOfCase.FriendList:
                content = response.FriendList;
                break;
            case ObfuscatedResponseContentOneOfCase.AcquaintanceServersResponse:
                content = response.AcquaintanceServersResponse;
                break;
            case ObfuscatedResponseContentOneOfCase.None:
            default:
                Log.LogError("Could not find type of response.");
                return;
        }

        if (content == null)
        {
            Log.LogError("Response content is null.\n{Response}.", response);
            return;
        }

        Log.LogInformation("Received response of type {Type}.", content.GetType());
        MessageReceived?.Invoke(null, content);
    }
}

class TreasureHuntEvent
{
}
