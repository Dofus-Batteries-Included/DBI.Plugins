using System;
using System.Collections.Generic;
using Core.UILogic.Components.Figma;
using Core.UILogic.Components.Tooltips;
using Core.UILogic.Components.Tooltips.Builder;
using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityEngine.UIElements;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace DofusBatteriesIncluded.Core.Behaviours;

public class DofusBatteriesIncludedGameMenu : MonoBehaviour
{
    static readonly ILogger Log = DBI.Logging.Create<DofusBatteriesIncludedGameMenu>();

    UIDocument _uiDocument;
    DofusVisualElement _gameMenuContainer;
    TooltipRoot _tooltipRoot;

    readonly List<ButtonToAdd> _toAdd = [];
    readonly List<ButtonInstance> _buttons = [];

    public void AddButton(string buttonName, Action<MouseUpEvent> callback) => _toAdd.Add(new ButtonToAdd(buttonName, callback));

    void Update()
    {
        TryLoadUiDocument();
        TryLoadGameMenuContainer();
        TryLoadTooltipRoot();
        TryAddButtons();
    }

    void TryLoadUiDocument()
    {
        if (_uiDocument)
        {
            return;
        }

        _uiDocument = Helpers.FindObjectOfType<UIDocument>();

        if (_uiDocument)
        {
            Log.LogInformation("Root UI found.");
        }
    }

    void TryLoadGameMenuContainer()
    {
        if (!_uiDocument || _gameMenuContainer != null)
        {
            return;
        }

        VisualElement root = _uiDocument.rootVisualElement;
        VisualElement gameMenu = root?.Q<VisualElement>("GameMenu");
        VisualElement menuCtr = gameMenu?.Q<VisualElement>("menuCtr");
        _gameMenuContainer = menuCtr?.Q<DofusVisualElement>();

        if (_gameMenuContainer != null)
        {
            Log.LogInformation("Game Menu found.");
        }
    }

    void TryLoadTooltipRoot()
    {
        if (!_uiDocument || _tooltipRoot != null)
        {
            return;
        }

        VisualElement root = _uiDocument.rootVisualElement;
        _tooltipRoot = root?.Q<TooltipRoot>();

        if (_tooltipRoot != null)
        {
            Log.LogInformation("Tooltip Root found.");
        }
    }

    void TryAddButtons()
    {
        if (!_uiDocument || _gameMenuContainer == null)
        {
            return;
        }

        foreach (ButtonToAdd button in _toAdd.ToArray())
        {
            ButtonInstance buttonInstance = InstantiateButton(button.Name, button.Action);
            if (buttonInstance == null)
            {
                continue;
            }

            _toAdd.Remove(button);
            _buttons.Add(buttonInstance);
        }
    }

    ButtonInstance InstantiateButton(string name, Action<MouseUpEvent> action)
    {
        if (_gameMenuContainer == null)
        {
            Log.LogInformation("Game menu not initialized yet.");
            return null;
        }

        DofusButtonCustom button = new() { name = name };
        button.clickable.onMouseUpAction += action;
        button.m_currentContent = DofusButtonCustom.ButtonContentEnum.icon;
        button.classList.Add("icon");
        button.icon = FigmaIcons.battery;
        button.mainStyle = DofusButtonCustom.ComponentStyleEnum.secondary;
        button.size = DofusButtonCustom.SizeEnum.large;
        button.status = DofusButtonCustom.ButtonStatusEnum.normal;
        button.CreateShadowContent();
        Il2CppSystem.Collections.Generic.List<ShadowData> shadow = new();
        shadow.Add(new ShadowData { color = new Color(0, 0, 0, 0.25f), offset = new Vector2(0, 1) });
        button.SetShadow(shadow);
        button.SetGapValue(10);
        _gameMenuContainer.Insert(0, button);

        TextTooltipBuilder tooltipBuilder = new(name, button);

        button.RegisterCallback<MouseEnterEvent>(
            (Action<MouseEnterEvent>)(evt =>
            {
                IBaseTooltipBuilder builder = new(tooltipBuilder.Pointer);
                _tooltipRoot?.m_tooltipService.bbxr(builder);
                _tooltipRoot?.Show();
            })
        );
        button.RegisterCallback<MouseOutEvent>((Action<MouseOutEvent>)(evt => _tooltipRoot?.Hide()));

        Log.LogInformation("Button {Name} added to game menu.", name);
        return new ButtonInstance(name, action, button);
    }

    class ButtonToAdd
    {
        public ButtonToAdd(string name, Action<MouseUpEvent> action)
        {
            Name = name;
            Action = action;
        }

        public string Name { get; }
        public Action<MouseUpEvent> Action { get; }
    }

    class ButtonInstance
    {
        public ButtonInstance(string name, Action<MouseUpEvent> action, DofusButtonCustom button)
        {
            Name = name;
            Action = action;
        }

        public string Name { get; }
        public Action<MouseUpEvent> Action { get; }
        public DofusButtonCustom Button { get; set; }
    }
}
