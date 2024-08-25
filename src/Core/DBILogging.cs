using System;
using DofusBatteriesIncluded.Core.Logging;
using Microsoft.Extensions.Logging;

namespace DofusBatteriesIncluded.Core;

// ReSharper disable once InconsistentNaming
public class DBILogging
{
    readonly DofusBatteriesIncludedLoggerProvider _provider = new();

    internal DBILogging() { }

    public ILogger Create() => Create("");
    public ILogger Create(string name) => _provider.CreateLogger(name);
    public ILogger Create(Type type) => Create(type.Name);
    public ILogger Create<T>() => Create(typeof(T));
}
