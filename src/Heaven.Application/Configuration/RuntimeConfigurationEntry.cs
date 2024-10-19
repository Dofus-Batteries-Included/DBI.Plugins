namespace DBI.Heaven.Application.Configuration;

public class RuntimeConfigurationEntry(string name)
{
    /// <summary>
    ///     The name of the entry
    /// </summary>
    public string Name { get; internal set; } = name;

    /// <summary>
    ///     The description of the entry.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    ///     Is the entry hidden.
    /// </summary>
    public bool Hidden { get; init; }
}

public class RuntimeConfigurationEntry<T>(string name) : RuntimeConfigurationEntry(name)
{
    /// <summary>
    ///     The possible values of this entry.
    /// </summary>
    public IReadOnlyList<T>? PossibleValues { get; init; }

    /// <summary>
    ///     The default value of this entry.
    /// </summary>
    public T? DefaultValue { get; init; }
}
