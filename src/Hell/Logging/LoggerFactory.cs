using Microsoft.Extensions.Logging;

namespace DBI.Hell.Logging;

public class LoggerFactory
{
    readonly DofusBatteriesIncludedLoggerProvider _provider = new();

    internal LoggerFactory() { }

    public ILogger Create() => Create("");
    public ILogger Create(string name) => _provider.CreateLogger(name);
    public ILogger Create(Type type) => Create(type.Name);
    public ILogger Create<T>() => Create(typeof(T));
}
