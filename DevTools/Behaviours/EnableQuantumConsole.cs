using QFSW.QC;
using UnityEngine;
using Helpers = DofusBatteriesIncluded.Core.Helpers;
using Input = UnityEngine.Input;

namespace DofusBatteriesIncluded.DevTools.Behaviours;

public class EnableQuantumConsole : MonoBehaviour
{
    QuantumConsole _console;

    void Start() => _console = Helpers.FindObjectOfType<QuantumConsole>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3))
        {
            _console.gameObject.SetActive(!_console.gameObject.activeSelf);
        }
    }
}
