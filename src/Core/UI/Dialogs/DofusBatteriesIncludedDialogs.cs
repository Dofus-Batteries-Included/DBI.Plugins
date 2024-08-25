using UnityEngine;

namespace DofusBatteriesIncluded.Core.UI.Dialogs;

public class DofusBatteriesIncludedDialogs : MonoBehaviour
{
    DofusBatteriesIncludedConfirmationDialog _confirmationDialog;

    void Start()
    {
        GameObject dialogObject = new("Confirmation dialog");
        dialogObject.transform.SetParent(transform);
        _confirmationDialog = dialogObject.AddComponent<DofusBatteriesIncludedConfirmationDialog>();
    }

    public void OpenConfirmationDialog(ConfirmationDialogConfiguration configuration)
    {
        _confirmationDialog?.Configure(configuration);
        _confirmationDialog?.Open();
    }
}
