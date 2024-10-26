using System.Diagnostics.CodeAnalysis;
using Grpc.Core;

namespace DBI.Heaven.Application.GrpcUtils;

public class GrpcServerStreamingBroadcast<T>(ILogger logger)
{
    readonly List<IServerStreamWriter<T>> _writers = [];
    readonly List<T> _buffer = [];
    readonly object _bufferLock = new();
    readonly CancellationTokenSource _cancellationTokenSource = new();
    Task? _runTask;

    public void RegisterWriter(IServerStreamWriter<T> writer) => _writers.Add(writer);

    public void Broadcast(T value)
    {
        lock (_bufferLock)
        {
            _buffer.Add(value);
        }
    }

    public Task Run() => _runTask ??= RunImplAsync();

    async Task RunImplAsync()
    {
        CancellationToken cancellationToken = _cancellationTokenSource.Token;
        while (!cancellationToken.IsCancellationRequested)
        {
            if (!HasMessagesToSend(out T[]? messages))
            {
                continue;
            }

            foreach (T message in messages)
            {
                await SendMessageToAllWriters(message, cancellationToken);
            }

            await Task.Delay(1000, cancellationToken);
        }
    }

    bool HasMessagesToSend([NotNullWhen(true)] out T[]? messages)
    {
        messages = null;

        lock (_bufferLock)
        {
            if (_buffer.Count == 0)
            {
                return false;
            }

            messages = ((IEnumerable<T>)_buffer).ToArray();
            _buffer.Clear();
            return true;
        }
    }

    async Task SendMessageToAllWriters(T message, CancellationToken cancellationToken)
    {
        List<IServerStreamWriter<T>> writersToRemove = [];

        foreach (IServerStreamWriter<T> writer in _writers)
        {
            try
            {
                await writer.WriteAsync(message, cancellationToken);
            }
            catch (Exception exn)
            {
                logger.LogError(exn, "Error while writing message to gRpc channel.");
                writersToRemove.Add(writer);
            }
        }

        foreach (IServerStreamWriter<T> writer in writersToRemove)
        {
            _writers.Remove(writer);
        }
    }
}
