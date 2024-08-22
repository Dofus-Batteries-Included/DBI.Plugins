using DofusBatteriesIncluded.Core.Protocol;

namespace DofusBatteriesIncluded.Core;

// ReSharper disable once InconsistentNaming
public class DBIMessaging
{
    public MessageListener<TMessage> GetListener<TMessage>() => new();
}
