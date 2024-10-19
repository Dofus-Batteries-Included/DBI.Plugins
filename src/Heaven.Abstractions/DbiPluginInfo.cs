namespace Heaven.Abstractions;

/// <summary>
///     Information about a plugin.
/// </summary>
public class DbiPluginInfo
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DbiPluginInfo" /> class.
    /// </summary>
    /// <param name="name">The unique name of the plugin.</param>
    /// <param name="displayName">The display name of the plugin.</param>
    /// <param name="version">The version of the plugin.</param>
    /// <param name="expectedGameBuildId">Optional. The expected game build ID that this plugin is compatible with.</param>
    /// <param name="expectedGameVersion">Optional. The expected game version associated with the game build ID.</param>
    public DbiPluginInfo(string name, string displayName, Version version, Guid? expectedGameBuildId = null, string? expectedGameVersion = null)
    {
        Name = name;
        DisplayName = displayName;
        Version = version;
        ExpectedGameBuildId = expectedGameBuildId;
        ExpectedGameVersion = expectedGameVersion;
    }

    /// <summary>
    ///     The unique name of the plugin.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    ///     The display name of the plugin.
    /// </summary>
    public string DisplayName { get; init; }

    /// <summary>
    ///     The version of the plugin.
    /// </summary>
    public Version Version { get; init; }

    /// <summary>
    ///     The expected game build ID that the plugin is compatible with.
    ///     If this value is set, the plugin will not be loaded when the actual game build ID differs.
    /// </summary>
    public Guid? ExpectedGameBuildId { get; init; }

    /// <summary>
    ///     The expected game version associated with the specified build ID.
    ///     This value is informational and indicates to the user the game version corresponding to the build ID.
    /// </summary>
    public string? ExpectedGameVersion { get; init; }
}
