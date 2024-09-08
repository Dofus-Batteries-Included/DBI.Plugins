using System;
using System.Threading.Tasks;
using BepInEx;
using DofusBatteriesIncluded.Plugins.Core.Behaviours;
using DofusBatteriesIncluded.Plugins.Core.Player;
using DofusBatteriesIncluded.Plugins.Core.Protocol;
using DofusBatteriesIncluded.Plugins.Core.UI;
using DofusBatteriesIncluded.Plugins.Core.UI.Dialogs;
using DofusBatteriesIncluded.Plugins.Core.UI.Menus;
using DofusBatteriesIncluded.Plugins.Core.UI.Windows;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;

namespace DofusBatteriesIncluded.Plugins.Core;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class CorePlugin : DBIPlugin
{
    public static bool UseScrollActions { get; private set; }
    public static event EventHandler<bool> UseScrollActionsChanged;

    public override bool CanBeDisabled => false;

    protected override Task StartAsync()
    {
        UseScrollActions = DBI.Configuration.Configure("Path Finding", "Experimental: use scroll actions", false)
            .WithDescription(
                "Scroll actions are supposed to give us the available scroll actions for maps, they take into account the actual world graph. "
                + "However they seem off so it is not recommended to enable this option. For Treasure Hunts it doesn't really matter if we don't take into account obstacles."
            )
            .RegisterChangeCallback(
                value =>
                {
                    UseScrollActions = value;
                    UseScrollActionsChanged?.Invoke(this, value);
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

        DBI.Messaging.RegisterListener<UpdateCurrentAccount>();
        DBI.Messaging.RegisterListener<UpdateCurrentPlayer>();
        DBI.Messaging.RegisterListener<UpdateCurrentPlayerMap>();

        return Task.CompletedTask;
    }
}
