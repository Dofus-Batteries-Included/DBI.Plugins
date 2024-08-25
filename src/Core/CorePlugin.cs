using System;
using System.Reflection;
using System.Threading.Tasks;
using BepInEx;
using DofusBatteriesIncluded.Core.Behaviours;
using DofusBatteriesIncluded.Core.Metadata;
using DofusBatteriesIncluded.Core.Player;
using DofusBatteriesIncluded.Core.Protocol;
using DofusBatteriesIncluded.Core.UI;
using DofusBatteriesIncluded.Core.UI.Dialogs;
using DofusBatteriesIncluded.Core.UI.Menus;
using DofusBatteriesIncluded.Core.UI.Windows;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using Guid = System.Guid;

namespace DofusBatteriesIncluded.Core;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class CorePlugin : DBIPlugin
{
    public static bool DontUseScrollActions { get; private set; }
    public static event EventHandler<bool> DontUseScrollActionsChanged;

    public CorePlugin() : base(GetExpectedBuildIdFromAssemblyAttribute()) { }

    public override bool CanBeDisabled => false;

    protected override Task StartAsync()
    {
        DontUseScrollActions = DBI.Configuration.Configure("Path Finding", "Do not use scroll actions", false)
            .WithDescription(
                "Scroll actions seem off, this toggle is used to remove them and assume that the adjacent maps of a given map are the ones directly adjacent to it coordinate-wise. "
                + "It will be off whenever the adjacent is not directly next to the map, e.g. if it is above or below. It will also fail to take into account obstacles between maps."
            )
            .RegisterChangeCallback(
                value =>
                {
                    DontUseScrollActions = value;
                    DontUseScrollActionsChanged?.Invoke(this, value);
                }
            )
            .Bind();

        AddComponent<DofusBatteriesIncludedCommands>();

        ClassInjector.RegisterTypeInIl2Cpp<DofusBatteriesIncludedConfirmationDialog>();
        AddComponent<DofusBatteriesIncludedDialogs>();

        ClassInjector.RegisterTypeInIl2Cpp<DofusBatteriesIncludedWindow>();
        CoreWindow window = AddComponent<CoreWindow>();

        DofusBatteriesIncludedGameMenu menu = AddComponent<DofusBatteriesIncludedGameMenu>();

        menu.AddButton("Dofus Batteries Included", evt => window.Toggle());

        Harmony.CreateAndPatchAll(typeof(Messaging));

        DBI.Messaging.RegisterListener<UpdateCurrentPlayer>();
        DBI.Messaging.RegisterListener<UpdateCurrentPlayerMap>();

        return Task.CompletedTask;
    }

    static Guid? GetExpectedBuildIdFromAssemblyAttribute() => typeof(CorePlugin).Assembly.GetCustomAttribute<ExpectedDofusBuildIdAttribute>()?.BuildId;
}
