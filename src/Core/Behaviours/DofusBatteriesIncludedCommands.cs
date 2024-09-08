using Microsoft.Extensions.Logging;
using UnityEngine;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Input = UnityEngine.Input;
using KeyCode = UnityEngine.KeyCode;

namespace DofusBatteriesIncluded.Plugins.Core.Behaviours;

public class DofusBatteriesIncludedCommands : MonoBehaviour
{
    static readonly ILogger Log = DBI.Logging.Create<DofusBatteriesIncludedCommands>();

    void Update()
    {
        foreach (KeyCode key in DBI.Commands.GetRegisteredKeys())
        {
            if (Input.GetKeyDown(key))
            {
                foreach (DBICommands.Command command in DBI.Commands.GetCommands(key))
                {
                    Log.LogDebug("Command invoked: {Name}", command.Name);
                    command.Action?.Invoke();
                }
            }
        }
    }
}
