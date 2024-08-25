using System;
using Core.UILogic.Components.Figma;
using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Action = Il2CppSystem.Action;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace DofusBatteriesIncluded.Core.UI.Windows;

public class DofusBatteriesIncludedWindow : MonoBehaviour
{
    static readonly ILogger Log = DBI.Logging.Create<DofusBatteriesIncludedWindow>();

    UIDocument _uiDocument;
    VisualElement _container;
    protected WindowFigma _window;
    bool _mainSceneLoaded;

    public bool IsOpen { get; private set; }
    protected virtual string Name => "Window Header";
    protected virtual string Help => null;
    protected virtual bool HasCloseButton => true;
    protected virtual bool HasBackdrop => false;

    public void Toggle()
    {
        if (IsOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    public void Open()
    {
        if (_window == null)
        {
            throw new InvalidOperationException("Window is not initialized yet.");
        }

        _window.parent.visible = true;
        IsOpen = true;
        OnOpen();
    }

    /// <param name="force">Don't call OnBeforeClose and assume that it returned true.</param>
    public void Close(bool force = false)
    {
        if (_window == null)
        {
            throw new InvalidOperationException("Window is not initialized yet.");
        }

        if (force || OnBeforeClose())
        {
            _window.parent.visible = false;
            IsOpen = false;
            OnAfterClose();
        }
    }

    protected virtual void Build(WindowFigma window)
    {
        window.header.classList.Add("primary");
        window.name = $"{Name} Window";
        window.header.title = Name;
        window.header.showHelpButton = Help != null;
        window.helpButton.text = Help;
        window.showCloseButton = HasCloseButton;
        window.closeButton.add_Clicked((Action)(() => Close()));
    }

    protected virtual void OnOpen() { }
    protected virtual bool OnBeforeClose() => true;
    protected virtual void OnAfterClose() { }

    void Start()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (IsMainScene(scene))
        {
            _mainSceneLoaded = true;
        }
        else
        {
            _mainSceneLoaded = false;
            SceneManager.add_sceneLoaded((Action<Scene, LoadSceneMode>)OnSceneLoaded);
            Log.LogInformation("Window {Name} waiting for main scene to load...", Name);
        }
    }

    void Update()
    {
        if (!_mainSceneLoaded)
        {
            return;
        }

        TryLoadUiDocument();
        TryBuildWindow();
    }

    void TryLoadUiDocument()
    {
        if (_uiDocument)
        {
            return;
        }

        _uiDocument = Helpers.FindObjectOfType<UIDocument>();
    }

    void TryBuildWindow()
    {
        if (!_uiDocument || _window != null)
        {
            return;
        }

        VisualElement root = _uiDocument.rootVisualElement;
        VisualElement popupContainer = root?.Q<VisualElement>("RootPopup");

        if (popupContainer != null)
        {
            _container = new VisualElement
            {
                name = $"{Name} Container",
                visible = false,
                pickingMode = PickingMode.Ignore,
                style =
                {
                    position = Position.Absolute,
                    width = new Length(100, Length.Unit.Percent),
                    height = new Length(100, Length.Unit.Percent)
                }
            };
            popupContainer.Add(_container);

            if (HasBackdrop)
            {
                VisualElement backdrop = new()
                {
                    name = $"{Name} Backdrop",
                    pickingMode = PickingMode.Position,
                    style =
                    {
                        position = Position.Absolute, backgroundColor = new Color(0, 0, 0, 0.2f), width = new Length(100, Length.Unit.Percent),
                        height = new Length(100, Length.Unit.Percent)
                    }
                };
                _container.Add(backdrop);
            }

            _window = new WindowFigma();
            Build(_window);
            _container.Add(_window);

            _window.parent.RegisterCallback<GeometryChangedEvent>((Action<GeometryChangedEvent>)PositionWindow);

            Log.LogInformation("Window {Name} has been created.", Name);
        }
    }

    void PositionWindow(GeometryChangedEvent _)
    {
        _window.style.left = (_window.parent.rect.m_Width - _window.rect.m_Width) / 2;
        _window.style.top = (_window.parent.rect.m_Height - _window.rect.m_Height) / 2;
        _window.parent.UnregisterCallback<GeometryChangedEvent>((Action<GeometryChangedEvent>)PositionWindow);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode _)
    {
        if (IsMainScene(scene))
        {
            _mainSceneLoaded = true;
        }
    }

    static bool IsMainScene(Scene scene) => scene.name == "DofusUnity";
}
