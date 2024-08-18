using Microsoft.Extensions.Logging;

namespace DofusBatteriesIncluded.Core.Logging;

public static class Logger
{
    static readonly BepInExLoggerProvider Provider = new();

    public static ILogger Create(string name) => Provider.CreateLogger(name);
    public static ILogger Create<T>() => Provider.CreateLogger(typeof(T).Name);
}
