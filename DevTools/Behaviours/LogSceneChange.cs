using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace DofusBatteriesIncluded.DevTools.Behaviours;

public class LogSceneChange : MonoBehaviour
{
    void Awake() => SceneManager.add_activeSceneChanged((UnityAction<Scene, Scene>)OnSceneChanged);

    void OnSceneChanged(Scene oldScene, Scene newScene) => Plugin.Log.LogInfo($"Scene changed from {GetSceneDisplayName(oldScene)} to {GetSceneDisplayName(newScene)}");

    string GetSceneDisplayName(Scene scene) => $"{scene.name ?? scene.path ?? "???"} ({scene.handle})";
}
