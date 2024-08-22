using System.Threading.Tasks;

namespace DofusBatteriesIncluded.Core.Protocol;

public interface IMessageListener
{
}

public interface IMessageListener<in TMessage> : IMessageListener
{
    Task HandleAsync(TMessage message);
}
