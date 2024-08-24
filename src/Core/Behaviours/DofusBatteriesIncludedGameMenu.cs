using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.UILogic.Components.Figma;
using Core.UILogic.Components.Tooltips;
using Core.UILogic.Components.Tooltips.Builder;
using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityEngine.UIElements;
using CancellationToken = Il2CppSystem.Threading.CancellationToken;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace DofusBatteriesIncluded.Core.Behaviours;

public class DofusBatteriesIncludedGameMenu : MonoBehaviour
{
    static readonly ILogger Log = DBI.Logging.Create<DofusBatteriesIncludedGameMenu>();
    readonly MethodInfo _tooltipRootUseBuilderMethod = typeof(TooltipRoot).GetProperty(nameof(TooltipRoot.m_tooltipService), BindingFlags.Instance | BindingFlags.Public)
        ?.PropertyType.GetMethods()
        .FirstOrDefault(
            m =>
            {
                if (m.ReturnType != typeof(string))
                {
                    return false;
                }

                ParameterInfo[] parameters = m.GetParameters();
                if (parameters.Length != 2)
                {
                    return false;
                }

                if (parameters[0].ParameterType != typeof(IBaseTooltipBuilder) || parameters[1].ParameterType != typeof(CancellationToken))
                {
                    return false;
                }

                return true;
            }
        );

    UIDocument _uiDocument;
    DofusVisualElement _gameMenuContainer;
    TooltipRoot _tooltipRoot;
    bool _widgetManagerNeedsRepositioning;

    readonly List<ButtonToAdd> _toAdd = [];
    readonly List<ButtonInstance> _buttons = [];

    public void AddButton(string buttonName, Action<MouseUpEvent> callback) => _toAdd.Add(new ButtonToAdd(buttonName, callback));

    void Start()
    {
        if (_tooltipRootUseBuilderMethod == null)
        {
            Log.LogWarning("Could not find method to assign builder to Tooltip Root. Tooltips won't work.");
        }
    }

    void Update()
    {
        TryLoadUiDocument();
        TryLoadGameMenuContainer();
        TryLoadTooltipRoot();
        TryAddButtons();
        TryRepositionWidgetManager();
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
        if (!_uiDocument || _gameMenuContainer == null || _toAdd.Count == 0)
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

        _widgetManagerNeedsRepositioning = true;
    }

    void TryRepositionWidgetManager()
    {
        if (!_uiDocument || !_widgetManagerNeedsRepositioning)
        {
            return;
        }

        VisualElement widgetManager = _uiDocument.rootVisualElement.Q<VisualElement>("WidgetManager");
        VisualElement menu = widgetManager?.m_Children._items.FirstOrDefault();
        if (menu == null)
        {
            return;
        }

        menu.style.marginRight = 128 + _buttons.Count * 40;
        _widgetManagerNeedsRepositioning = false;
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

        if (_tooltipRootUseBuilderMethod != null)
        {
            button.RegisterCallback<MouseEnterEvent>(
                (Action<MouseEnterEvent>)(evt =>
                {
                    if (_tooltipRoot?.m_tooltipService == null)
                    {
                        return;
                    }

                    TextTooltipBuilder tooltipBuilder = new(name, button);
                    IBaseTooltipBuilder builder = new(tooltipBuilder.Pointer);
                    _tooltipRootUseBuilderMethod.Invoke(_tooltipRoot.m_tooltipService, [builder, CancellationToken.None]);
                    _tooltipRoot?.Show();
                })
            );
            button.RegisterCallback<MouseOutEvent>((Action<MouseOutEvent>)(evt => _tooltipRoot?.Hide()));
        }

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
