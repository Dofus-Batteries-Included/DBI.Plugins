namespace DofusBatteriesIncluded.Core;

// ReSharper disable once InconsistentNaming
public static class DBI
{
    public static bool Enabled { get; internal set; }
    public static DBILogging Logging { get; } = new();
    public static DBICommands Commands { get; } = new();
}
