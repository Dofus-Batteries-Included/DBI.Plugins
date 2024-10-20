namespace DBI.Hell.Configuration;

public enum ConfigurationChangeSource
{
    /// <summary>
    ///     Value changed by Hell itself.
    /// </summary>
    Internal,

    /// <summary>
    ///     Value changed by a user through the UI.
    /// </summary>
    Ui,

    /// <summary>
    ///     Value changed by Heaven.
    /// </summary>
    Heaven
}
