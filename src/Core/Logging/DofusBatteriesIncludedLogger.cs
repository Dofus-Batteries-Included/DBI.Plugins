using System;
using BepInEx.Logging;
using Microsoft.Extensions.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace DofusBatteriesIncluded.Plugins.Core.Logging;

public class DofusBatteriesIncludedLogger : ILogger
{
    readonly ManualLogSource _log;
    readonly string _name;

    public DofusBatteriesIncludedLogger(ManualLogSource log, string name = null)
    {
        _log = log;
        _name = name;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        BepInEx.Logging.LogLevel level = GetBepInExLogLevel(logLevel);

        string content = formatter(state, exception);
        if (!string.IsNullOrWhiteSpace(_name))
        {
            content = $"{_name} - {content}";
        }

        _log.Log(level, content);
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    static BepInEx.Logging.LogLevel GetBepInExLogLevel(LogLevel logLevel) =>
        logLevel switch
        {
            LogLevel.Trace => BepInEx.Logging.LogLevel.Debug,
            LogLevel.Debug => BepInEx.Logging.LogLevel.Debug,
            LogLevel.Information => BepInEx.Logging.LogLevel.Info,
            LogLevel.Warning => BepInEx.Logging.LogLevel.Warning,
            LogLevel.Error => BepInEx.Logging.LogLevel.Error,
            LogLevel.Critical => BepInEx.Logging.LogLevel.Fatal,
            LogLevel.None => BepInEx.Logging.LogLevel.None,
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null)
        };

    public IDisposable BeginScope<TState>(TState state) => default!;
}
