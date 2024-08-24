using System;
using System.Linq;
using Core.UILogic.Components;
using Core.UILogic.Components.Figma;
using DofusBatteriesIncluded.Core;
using Il2CppSystem.Collections.Generic;
using Microsoft.Extensions.Logging;
using UnityEngine.UIElements;

namespace DofusBatteriesIncluded.TreasureSolver;

public static class TreasureHuntWindowAccessor
{
    static readonly ILogger Log = DBI.Logging.Create(typeof(TreasureHuntWindowAccessor));
    const string AdditionalTextContainerName = "DBI_TreasureSolver_AdditionalText";
    static VisualElement _treasureHuntWindow;

    public static bool TrySetStepAdditionalText(int stepIndex, string text) => SetStepAdditionalTextImpl(stepIndex, text);
    public static bool TryHideStepAdditionalText(int stepIndex) => SetStepAdditionalTextImpl(stepIndex, null);
    public static bool TryClear() => ClearImpl();

    static bool SetStepAdditionalTextImpl(int stepIndex, string text)
    {
        if (!FetchWindow())
        {
            Log.LogError("Treasure hunt window doesn't exist yet.");
            return false;
        }

        List<VisualElement> steps = _treasureHuntWindow.Query("ItemSteps").ToList();

        if (steps.Count <= stepIndex)
        {
            Log.LogError("Step {Index} is out of range.", stepIndex);
            return false;
        }

        VisualElement step = steps._items[stepIndex];

        DofusVisualElement additionalTextContainer = step.Q<DofusVisualElement>(AdditionalTextContainerName);
        if (additionalTextContainer == null)
        {
            VisualElement lastChild = step.m_Children._items.LastOrDefault(i => i != null);
            if (lastChild != null)
            {
                lastChild.style.paddingBottom = 0;
            }

            additionalTextContainer = CreateAdditionalTextContainer();
            step.Add(additionalTextContainer);

            VisualElement pad = new();
            pad.style.paddingBottom = 4;
            step.Add(pad);
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

        return true;
    }

    static bool ClearImpl()
    {
        if (!FetchWindow())
        {
            Log.LogError("Treasure hunt window doesn't exist yet.");
            return false;
        }

        List<VisualElement> steps = _treasureHuntWindow.Query("ItemSteps").ToList();

        foreach (VisualElement step in steps)
        {
            DofusVisualElement additionalTextContainer = step.Q<DofusVisualElement>(AdditionalTextContainerName);
            if (additionalTextContainer == null)
            {
                continue;
            }

            additionalTextContainer.style.display = DisplayStyle.None;
        }

        return true;
    }

    static bool FetchWindow()
    {
        if (_treasureHuntWindow != null)
        {
            return true;
        }

        _treasureHuntWindow = TryFindTreasureHuntWindow();
        return _treasureHuntWindow != null;
    }

    static VisualElement TryFindTreasureHuntWindow()
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
        DofusVisualElement result = new() { name = AdditionalTextContainerName, gapValue = 4, style = { flexDirection = FlexDirection.Row, paddingLeft = 16, paddingRight = 16 } };

        DofusLabel label = new() { style = { paddingLeft = 24 } };
        label.AddToClassList(DofusUiConstants.TextShortSmallRegular);
        label.AddToClassList(DofusUiConstants.TextHighlightPrimary100);
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
