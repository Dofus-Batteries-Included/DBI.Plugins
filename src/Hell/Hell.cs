using System.Runtime.Loader;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using DBI.Hell.Configuration;
using DBI.Hell.GameInterop.UI.Dialogs;
using DBI.Hell.GameInterop.UI.Menus;
using DBI.Hell.GameInterop.UI.Windows;
using DBI.Hell.HeavenInterop;
using DBI.Hell.Logging;
using DBI.Hell.Messages;
using DBI.Hell.Metadata;
using DBI.Hell.Plugins;
using DBI.Hell.RedirectMessages;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace DBI.Hell;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Dofus.exe")]
class Hell : BasePlugin
{
    public static bool Enabled { get; private set; }
    public static Guid? DofusBuildId { get; private set; }
    public static LoggerFactory Logging { get; private set; }
    public static HellBackgroundJobs BackgroundJobs { get; private set; }
    public static GameClientInformation GameClientInformation { get; private set; }
    public static HeavenHandle Heaven { get; private set; }
    public static MessagesManager Messages { get; private set; }
    public static PluginsManager Plugins { get; private set; }
    public static ConfigurationManager Configuration { get; private set; }

    readonly ILogger _logger;

    public Hell()
    {
        Logging = new LoggerFactory();
        BackgroundJobs = new HellBackgroundJobs();
        GameClientInformation = GameClientInformation.CreateFromOwnProcess();
        Heaven = new HeavenHandle();
        Plugins = new PluginsManager();
        Configuration = new ConfigurationManager();

        _logger = Logging.Create<Hell>();

        // Load the real Google.Protobuf assembly in the default load context
        // and the proxy one in a new load context alongside the RedirectMessages assembly

        string thisAssemblyPath = System.AppContext.BaseDirectory;
        string thisAssemblyDirectory = Path.GetDirectoryName(thisAssemblyPath);

        AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Join(thisAssemblyDirectory, "Google.Protobuf.dll"));

        AssemblyLoadContext alc = new("DBI.Hell.RedirectMessages");
        alc.LoadFromAssemblyPath(Path.Join(thisAssemblyDirectory, "../../../interop/Google.Protobuf.dll"));
        alc.LoadFromAssemblyPath(Path.Join(thisAssemblyDirectory, "DBI.Hell.RedirectMessages.dll"));
    }

    public override void Load()
    {
        Enabled = Configuration.Configure("General", "Enabled", true).WithDescription("Enable or disable all Dofus Batteries Included plugins.").Hide().Bind();

        if (!Enabled)
        {
            _logger.LogInformation("Dofus Batteries Included is disabled.");
            return;
        }

        DofusBuildId = ReadDofusBuildId(_logger);
        Guid? expectedBuildId = ReadExpectedBuildId(_logger);

        if (expectedBuildId.HasValue && DofusBuildId.HasValue && expectedBuildId.Value != DofusBuildId.Value)
        {
            _logger.LogInformation(
                "Expected game build ID doesn't match actual build ID: {Expected} != {Actual}. "
                + "DBI won't start, please download the version of the plugin that matches the version of the game.",
                expectedBuildId.Value,
                DofusBuildId.Value
            );
            return;
        }

        MessageInterceptor interceptor = new(Logging.Create<MessageInterceptor>());
        Messages = new MessagesManager(interceptor);

        InitializeCoreComponents();

        BackgroundJobs.Start(LoadAsync, nameof(LoadAsync));
    }

    async Task<bool> LoadAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Connecting to Heaven...");
        if (!await Heaven.ConnectToHeavenAsync())
        {
            _logger.LogError("Could not initialize connection to Heaven, Hell will stop.");
            return false;
        }

        _logger.LogInformation("Installing Message patcher...");
        Harmony.CreateAndPatchAll(typeof(MessagesManager));

        _logger.LogInformation("Initializing plugins...");
        await Plugins.Initialize();

        return true;
    }

    static Guid? ReadExpectedBuildId(ILogger logger)
    {
        Guid? expectedBuildId = BuildMetadataHelpers.GetExpectedBuildIdFromAssemblyAttribute<Hell>();
        if (!expectedBuildId.HasValue)
        {
            logger.LogWarning("Expected build ID was not provided, the plugin will run even if it has not been built against to correct game files.");
        }
        else
        {
            logger.LogDebug("Found expected build ID: {Expected}.", expectedBuildId.Value);
        }

        string expectedVersion = BuildMetadataHelpers.GetExpectedVersionFromAssemblyAttribute<Hell>();
        if (!string.IsNullOrWhiteSpace(expectedVersion))
        {
            logger.LogDebug("Found expected version: {Expected}.", expectedVersion);
        }

        return expectedBuildId;
    }

    static Guid? ReadDofusBuildId(ILogger logger)
    {
        Guid? value = BuildMetadataHelpers.ReadDofusBuildId(logger);
        if (!DofusBuildId.HasValue)
        {
            logger.LogWarning("Could not determine actual build ID.");
        }
        else
        {
            logger.LogDebug("Found actual build ID: {Actual}.", DofusBuildId.Value);
        }

        return value;
    }

    void InitializeCoreComponents()
    {
        ClassInjector.RegisterTypeInIl2Cpp<DofusBatteriesIncludedConfirmationDialog>();
        ClassInjector.RegisterTypeInIl2Cpp<DofusBatteriesIncludedWindow>();

        AddComponent<DofusBatteriesIncludedDialogs>();
        DofusBatteriesIncludedSettingsMainWindow window = AddComponent<DofusBatteriesIncludedSettingsMainWindow>();
        DofusBatteriesIncludedGameMenu menu = AddComponent<DofusBatteriesIncludedGameMenu>();

        menu.AddButton("Dofus Batteries Included", _ => window.Toggle());
    }
}
