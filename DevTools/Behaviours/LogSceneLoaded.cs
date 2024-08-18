using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Logger = DofusBatteriesIncluded.Core.Logging.Logger;

namespace DofusBatteriesIncluded.DevTools.Behaviours;

public class LogSceneLoaded : MonoBehaviour
{
    static readonly ILogger Log = Logger.Create<LogSceneLoaded>();

    void Awake() => SceneManager.add_sceneLoaded((UnityAction<Scene, LoadSceneMode>)OnSceneChanged);
    void OnSceneChanged(Scene scene, LoadSceneMode mode) => Log.LogInformation($"Scene loaded: {GetSceneDisplayName(scene)} in mode {mode}");
    string GetSceneDisplayName(Scene scene) => $"{scene.name ?? scene.path ?? "???"} ({scene.handle})";
}
