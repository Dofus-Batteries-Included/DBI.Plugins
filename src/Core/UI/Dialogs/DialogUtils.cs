using System;
using DofusBatteriesIncluded.Plugins.Core.Behaviours;
using Microsoft.Extensions.Logging;

namespace DofusBatteriesIncluded.Plugins.Core.UI.Dialogs;

public static class DialogUtils
{
    static readonly ILogger Log = DBI.Logging.Create(typeof(DialogUtils));

    public static void OpenConfirmationDialog(Action<ConfirmationDialogConfiguration> configure = null)
    {
        DofusBatteriesIncludedDialogs dialogs = UnityHelpers.FindObjectOfType<DofusBatteriesIncludedDialogs>();
        if (dialogs is null)
        {
            Log.LogWarning("Could not find instance of {Type}.", nameof(DofusBatteriesIncludedDialogs));
            return;
        }

        ConfirmationDialogConfiguration configuration = new();
        configure?.Invoke(configuration);
        dialogs.OpenConfirmationDialog(configuration);
    }
}
