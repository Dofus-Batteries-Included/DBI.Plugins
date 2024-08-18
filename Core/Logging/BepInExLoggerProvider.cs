using System;
using BepInEx.Logging;
using Microsoft.Extensions.Logging;

namespace DofusBatteriesIncluded.Core.Logging;

public class BepInExLoggerProvider : ILoggerProvider
{
    readonly ManualLogSource _log = BepInEx.Logging.Logger.CreateLogSource("DBI");

    public ILogger CreateLogger(string categoryName) => new BepInExLogger(_log, categoryName);
    public void Dispose() => GC.SuppressFinalize(this);
}
