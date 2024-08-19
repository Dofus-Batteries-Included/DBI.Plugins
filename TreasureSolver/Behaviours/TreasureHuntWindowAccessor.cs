using System;
using System.Linq;
using Core.UILogic.Components;
using Core.UILogic.Components.Figma;
using Core.UILogic.UIConsts;
using DofusBatteriesIncluded.Core;
using Il2CppSystem.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace DofusBatteriesIncluded.TreasureSolver.Behaviours;

public class TreasureHuntWindowAccessor : MonoBehaviour
{
    const string AdditionalTextContainerName = "DBI_TreasureSolver_AdditionalText";

    static TreasureHuntWindowAccessor _instance;
    VisualElement _treasureHuntWindow;

    void Awake() => _instance = this;

    public static void SetStepAdditionalText(int stepIndex, string text) => _instance.SetStepAdditionalTextImpl(stepIndex, text);
    public static void HideStepAdditionalText(int stepIndex) => _instance.SetStepAdditionalTextImpl(stepIndex, null);

    void SetStepAdditionalTextImpl(int stepIndex, string text)
    {
        if (_treasureHuntWindow == null)
        {
            _treasureHuntWindow = TryFindTreasureHuntWindow();

            if (_treasureHuntWindow == null)
            {
                throw new InvalidOperationException("Treasure hunt window doesn't exist yet.");
            }
        }

        List<VisualElement> steps = _treasureHuntWindow.Query("ItemSteps").ToList();
        if (steps.Count <= stepIndex)
        {
            throw new InvalidOperationException($"Step {stepIndex} is out of range.");
        }

        VisualElement step = steps._items[stepIndex];

        VisualElement lastChild = step.m_Children._items.LastOrDefault(i => i != null);
        if (lastChild != null)
        {
            lastChild.style.paddingBottom = 0;
        }

        DofusVisualElement additionalTextContainer = step.Q<DofusVisualElement>(AdditionalTextContainerName);
        if (additionalTextContainer == null)
        {

            additionalTextContainer = CreateAdditionalTextContainer();
            step.Add(additionalTextContainer);
        }

        if (text == null)
        {
            additionalTextContainer.style.display = DisplayStyle.None;
        }
        else
        {
            additionalTextContainer.style.display = DisplayStyle.Flex;
            SetAdditionalText(additionalTextContainer, text);
        }
    }

    VisualElement TryFindTreasureHuntWindow()
    {
        UIDocument uiRoot = Helpers.FindObjectOfType<UIDocument>();
        if (!uiRoot)
        {
            return null;
        }

        VisualElement window = uiRoot.rootVisualElement.Q("TreasureHunt");
        return window;
    }

    static DofusVisualElement CreateAdditionalTextContainer()
    {
        DofusVisualElement result = new()
            { name = AdditionalTextContainerName, gapValue = 4, style = { flexDirection = FlexDirection.Row, paddingLeft = 16, paddingRight = 16, paddingBottom = 4 } };

        DofusLabel label = new() { style = { paddingLeft = 24 } };
        label.AddToClassList(ThemeConstants.TextShortSmallRegular);
        label.AddToClassList("textColor_highlight_primary100");
        result.Add(label);

        return result;
    }

    static void SetAdditionalText(VisualElement container, string text)
    {
        DofusLabel label = container.Q<DofusLabel>();
        if (label == null)
        {
            throw new InvalidOperationException($"Could not find {nameof(DofusLabel)} in additional text container.");
        }

        label.text = text;
    }
}
