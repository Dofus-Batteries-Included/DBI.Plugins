using System;
using Core.UILogic.Components.Figma;
using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityEngine.UIElements;
using Action = Il2CppSystem.Action;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace DofusBatteriesIncluded.Core.Behaviours;

public class DofusBatteriesIncludedWindow : MonoBehaviour
{
    static readonly ILogger Log = DBI.Logging.Create<DofusBatteriesIncludedWindow>();

    UIDocument _uiDocument;
    WindowFigma _window;

    public bool IsOpen { get; private set; }
    protected virtual string Name => "Window Header";
    protected virtual string Help => null;

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

    public void Close()
    {
        if (_window == null)
        {
            throw new InvalidOperationException("Window is not initialized yet.");
        }

        _window.parent.visible = false;
        IsOpen = false;
        OnClose();
    }

    protected virtual void Build(WindowFigma window)
    {
        window.header.classList.Add("primary");
        window.name = $"{Name} Window";
        window.header.title = Name;
        window.header.showHelpButton = Help != null;
        window.helpButton.text = Help;
        window.showCloseButton = true;
        window.closeButton.add_Clicked((Action)(() => Close()));
        window.style.width = new Length(50, Length.Unit.Percent);
        window.style.height = new Length(50, Length.Unit.Percent);
    }

    protected virtual void OnOpen() { }
    protected virtual void OnClose() { }

    void Update()
    {
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
            VisualElement container = new() { name = $"{Name} Container" };
            container.visible = false;
            container.pickingMode = PickingMode.Ignore;
            container.style.width = new Length(100, Length.Unit.Percent);
            container.style.height = new Length(100, Length.Unit.Percent);
            popupContainer.Add(container);

            _window = new WindowFigma();
            Build(_window);
            container.Add(_window);

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
}
