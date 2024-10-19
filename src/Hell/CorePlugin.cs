using BepInEx;
using BepInEx.Unity.IL2CPP;
using DBI.Hell.Configuration;
using DBI.Hell.HeavenInterop;
using DBI.Hell.Helpers;
using DBI.Hell.Logging;
using DBI.Hell.UI;
using DBI.Hell.UI.Dialogs;
using DBI.Hell.UI.Menus;
using DBI.Hell.UI.Windows;
using Il2CppInterop.Runtime.Injection;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace DBI.Hell;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Dofus.exe")]
class CorePlugin : BasePlugin
{
    public static bool Enabled { get; private set; }
    public static Guid? DofusBuildId { get; private set; }
    public static GameClientInformation GameClientInformation { get; private set; } = GameClientInformation.CreateFromOwnProcess();
    public static CoreLogging Logging { get; private set; } = new();
    public static CoreConfiguration Configuration { get; private set; } = new();

    public override void Load()
    {
        ILogger logger = Logging.Create<CorePlugin>();

        Enabled = Configuration.Configure("General", "Enabled", true).WithDescription("Enable or disable all Dofus Batteries Included plugins.").Hide().Bind();

        DofusBuildId = BuildMetadataHelpers.ReadDofusBuildId(logger);
        if (!DofusBuildId.HasValue)
        {
            logger.LogWarning("Could not determine actual build ID.");
        }
        else
        {
            logger.LogDebug("Found actual build ID: {Actual}.", DofusBuildId.Value);
        }

        Guid? expectedBuildId = BuildMetadataHelpers.GetExpectedBuildIdFromAssemblyAttribute<CorePlugin>();
        if (!expectedBuildId.HasValue)
        {
            logger.LogWarning("Expected build ID was not provided, the plugin will run even if it has not been built against to correct game files.");
        }
        else
        {
            logger.LogDebug("Found expected build ID: {Expected}.", expectedBuildId.Value);
        }

        string expectedVersion = BuildMetadataHelpers.GetExpectedVersionFromAssemblyAttribute<CorePlugin>();
        if (!string.IsNullOrWhiteSpace(expectedVersion))
        {
            logger.LogDebug("Found expected version: {Expected}.", expectedVersion);
        }

        if (expectedBuildId.HasValue && DofusBuildId.HasValue && expectedBuildId.Value != DofusBuildId.Value)
        {
            logger.LogInformation(
                "Expected game build ID doesn't match actual build ID: {Expected} != {Actual}. "
                + "DBI won't start, please download the version of the plugin that matches the version of the game.",
                expectedBuildId.Value,
                DofusBuildId.Value
            );
            return;
        }

        if (!Enabled)
        {
            logger.LogInformation("Dofus Batteries Included is disabled.");
            return;
        }

        LoadAsync(logger).ConfigureAwait(false);
    }

    async Task LoadAsync(ILogger logger)
    {
        logger.LogInformation("Starting Heaven...");
        if (!await HeavenInteroperability.StartHeaven())
        {
            logger.LogInformation("Could not start Heaven.");
            return;
        }

        logger.LogInformation("Heaven started successfully.");

        ClassInjector.RegisterTypeInIl2Cpp<DofusBatteriesIncludedConfirmationDialog>();
        AddComponent<DofusBatteriesIncludedDialogs>();

        ClassInjector.RegisterTypeInIl2Cpp<DofusBatteriesIncludedWindow>();
        CoreWindow window = AddComponent<CoreWindow>();

        DofusBatteriesIncludedGameMenu menu = AddComponent<DofusBatteriesIncludedGameMenu>();

        menu.AddButton("Dofus Batteries Included", _ => window.Toggle());
    }
}
