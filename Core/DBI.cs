namespace DofusBatteriesIncluded.Core;

// ReSharper disable once InconsistentNaming
public static class DBI
{
    public static bool Enabled { get; internal set; }
    public static DBIConfiguration Configuration { get; } = new();
    public static DBIPlugins Plugins { get; } = new();
    public static DBILogging Logging { get; } = new();
    public static DBICommands Commands { get; } = new();
}
