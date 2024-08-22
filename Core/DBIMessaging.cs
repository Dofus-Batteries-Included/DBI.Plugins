using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using DofusBatteriesIncluded.Core.Protocol;
using Microsoft.Extensions.Logging;

namespace DofusBatteriesIncluded.Core;

// ReSharper disable once InconsistentNaming
public class DBIMessaging : IDisposable
{
    static readonly ILogger Log = DBI.Logging.Create<DBIMessaging>();
    readonly HashSet<IMessageListener> _listeners = [];

    public DBIMessaging()
    {
        Messaging.MessageReceived += OnMessageReceived;
    }

    public void RegisterListener<T>() where T: IMessageListener, new() => _listeners.Add(new T());
    public void RegisterListener<T>(IMessageListener<T> listener) => _listeners.Add(listener);
    public void UnregisterListener<T>(IMessageListener<T> listener) => _listeners.Remove(listener);

    public MessageListener<TMessage> GetListener<TMessage>()
    {
        MessageListener<TMessage> listener = new();
        RegisterListener(listener);
        return listener;
    }

    public void Dispose()
    {
        Messaging.MessageReceived -= OnMessageReceived;
        GC.SuppressFinalize(this);
    }

    void OnMessageReceived(object sender, object message)
    {
        Type type = message.GetType();
        Type listenerType = typeof(IMessageListener<>).MakeGenericType(type);

        const string handleAsyncMethodName = nameof(IMessageListener<object>.HandleAsync);
        MethodInfo method = listenerType.GetMethod(handleAsyncMethodName, BindingFlags.Public | BindingFlags.Instance, [type]);
        if (method == null)
        {
            Log.LogError("Could not find method {Name} in type {Type}", handleAsyncMethodName, type);
            return;
        }

        foreach (IMessageListener listener in _listeners)
        {
            if (!listenerType.IsInstanceOfType(listener))
            {
                continue;
            }

            Task result = (Task)method.Invoke(listener, [message]);
            result!.GetAwaiter().GetResult();
        }
    }
}
