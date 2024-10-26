using System.Net;
using System.Net.Sockets;

namespace DBI.Hell.HeavenInterop;

class UnixDomainSocketsConnectionFactory
{
    readonly EndPoint _endPoint;

    public UnixDomainSocketsConnectionFactory(EndPoint endPoint)
    {
        _endPoint = endPoint;
    }

    public async ValueTask<Stream> ConnectAsync(SocketsHttpConnectionContext _, CancellationToken cancellationToken = default)
    {
        Socket socket = new(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

        try
        {
            await socket.ConnectAsync(_endPoint, cancellationToken).ConfigureAwait(false);
            return new NetworkStream(socket, true);
        }
        catch
        {
            socket.Dispose();
            throw;
        }
    }
}
