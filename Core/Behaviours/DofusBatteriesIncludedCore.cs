using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Logger = DofusBatteriesIncluded.Core.Logging.Logger;

namespace DofusBatteriesIncluded.Core.Behaviours;

public class DofusBatteriesIncludedCore : MonoBehaviour
{
    static readonly ILogger Log = Logger.Create<DofusBatteriesIncludedCore>();

    void Awake() => SceneManager.add_sceneLoaded((UnityAction<Scene, LoadSceneMode>)OnSceneChanged);

    void OnSceneChanged(Scene scene, LoadSceneMode _)
    {
        if (scene.name != "DofusUnity")
        {
            return;
        }

        SceneManager.remove_sceneLoaded((UnityAction<Scene, LoadSceneMode>)OnSceneChanged);
        Init();
    }

    void Init() => Log.LogInformation("DofusBatteriesIncludedCore initialized");
}
