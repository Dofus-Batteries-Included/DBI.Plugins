using BepInEx;
using BepInEx.Unity.IL2CPP;
using DBI.Hell.Configuration;
using DBI.Hell.HeavenInterop;
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
    public bool Enabled { get; private set; }
    public Guid? DofusBuildId { get; private set; }
    public static CoreLogging Logging { get; private set; } = new();
    public static CoreConfiguration Configuration { get; private set; } = new();

    public override void Load()
    {
        ILogger logger = Logging.Create<CorePlugin>();

        Enabled = Configuration.Configure("General", "Enabled", true).WithDescription("Enable or disable all Dofus Batteries Included plugins.").Hide().Bind();

        DofusBuildId = ReadDofusBuildId(logger);
        if (!DofusBuildId.HasValue)
        {
            logger.LogWarning("Could not determine actual build ID.");
        }
        else
        {
            logger.LogDebug("Found actual build ID: {Actual}.", DofusBuildId.Value);
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

        menu.AddButton("Dofus Batteries Included", evt => window.Toggle());
    }

    static Guid? ReadDofusBuildId(ILogger logger)
    {
        const string bootConfigFilePathRelativeToDofusExe = "Dofus_Data/boot.config";
        string dofusExePath = FindDofusExePath();
        string bootConfigPath = Path.Join(dofusExePath, bootConfigFilePathRelativeToDofusExe);
        if (!File.Exists(bootConfigPath))
        {
            logger.LogWarning("Could not find boot.config, looked at: {Path}", bootConfigPath);
            return null;
        }

        logger.LogDebug("Found boot.config at {Path}", bootConfigPath);

        string[] lines = File.ReadAllLines(bootConfigPath);
        Dictionary<string, string> props = new();
        foreach (string line in lines)
        {
            string[] parts = line.Split('=');
            if (parts.Length != 2)
            {
                continue;
            }

            props[parts[0]] = parts[1];
        }

        string guidStr = props.GetValueOrDefault("build-guid");
        if (!Guid.TryParse(guidStr, out Guid guid))
        {
            logger.LogWarning("Could not find build-guid in boot.config file (path: {Path})", bootConfigPath);
            return null;
        }

        return guid;
    }

    static string FindDofusExePath()
    {
        string current = Path.GetDirectoryName(Path.GetFullPath(typeof(CorePlugin).Assembly.Location));
        while (current != null && !File.Exists(Path.Join(current, "Dofus.exe")))
        {
            current = Path.GetDirectoryName(current);
        }

        return current;
    }
}
