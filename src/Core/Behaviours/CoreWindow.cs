using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Engine.Options;
using Core.UILogic.Components;
using Core.UILogic.Components.Figma;
using Core.UILogic.Config;
using Core.UILogic.Config.OptionElement;
using Microsoft.Extensions.Logging;
using UnityEngine;
using UnityEngine.UIElements;
using Action = System.Action;
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;
using Enum = System.Enum;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace DofusBatteriesIncluded.Core.Behaviours;

public class CoreWindow : DofusBatteriesIncludedWindow
{
    static readonly ILogger Log = DBI.Logging.Create<DofusBatteriesIncludedWindow>();
    protected override string Name => "Dofus Batteries Included";

    readonly Dictionary<Category, CategorySummaryItem> _items = [];
    Category? _currentCategory;

    CategorySummaryItem _generalItem;
    CategorySummaryItem _settingsItem;

    VisualElement _generalTab;
    VisualElement _settingsTab;

    public void SelectCategory(Category category)
    {
        if (_currentCategory.HasValue)
        {
            CategorySummaryItem item = GetItem(_currentCategory.Value);
            item.isSelected = false;
            VisualElement tab = GetTab(_currentCategory.Value);
            tab.style.display = DisplayStyle.None;
        }

        _currentCategory = category;

        if (_currentCategory.HasValue)
        {
            CategorySummaryItem item = GetItem(_currentCategory.Value);
            item.isSelected = true;
            VisualElement tab = GetTab(_currentCategory.Value);
            tab.style.display = DisplayStyle.Flex;
        }
    }

    protected override void Build(WindowFigma window)
    {
        base.Build(window);

        VisualElement container = new();
        container.style.display = DisplayStyle.Flex;
        container.style.flexDirection = FlexDirection.Row;
        container.style.height = new Length(100, Length.Unit.Percent);
        window.content.Add(container);

        VisualElement sidePanel = new();
        sidePanel.style.display = DisplayStyle.Flex;
        sidePanel.style.flexDirection = FlexDirection.Column;
        sidePanel.style.width = new Length(30, Length.Unit.Percent);
        sidePanel.style.height = new Length(100, Length.Unit.Percent);
        sidePanel.classList.Add("backgroundColor_background_medium90");
        ScrollView leftScrollView = new();
        leftScrollView.showHorizontal = false;

        _generalItem = CreateItem(Category.General, sidePanel);
        _settingsItem = CreateItem(Category.Settings, sidePanel);

        sidePanel.Add(leftScrollView);
        container.Add(sidePanel);

        ScrollView rightScrollView = new();
        rightScrollView.style.flexGrow = 1;
        rightScrollView.showHorizontal = false;

        _generalTab = CreateGeneralTab();
        rightScrollView.Add(_generalTab);

        _settingsTab = CreateSettingsTab();
        rightScrollView.Add(_settingsTab);

        container.Add(rightScrollView);
    }

    VisualElement CreateGeneralTab()
    {
        VisualElement visualElement = new();
        visualElement.style.display = DisplayStyle.None;

        OptionCategory pluginsHeader = new();
        pluginsHeader.Init(new CategoryData { name = "Plugins" }, false);
        visualElement.Add(pluginsHeader);

        SectionHeader label = new();
        label.style.width = new Length(100, Length.Unit.Percent);
        label.title = "Enabling or disabling a plugin requires a restart of the game.";
        label.isActivated = false;
        label.isOpen = true;
        label.isSelected = false;
        label.canBeOpened = false;
        label.clickable.active = false;
        label.noBorder = true;
        visualElement.Add(label);

        foreach (DBIPlugin plugin in DBI.Plugins.GetAll())
        {
            Il2CppSystem.Collections.Generic.List<IOptionData> options = new();

            DBIConfiguration.Entry<bool> enabledConfigurationEntry = DBI.Configuration.Get<bool>(plugin.Name, "Enabled");
            if (enabledConfigurationEntry != null)
            {
                BoolOption data = CreateOptionData(enabledConfigurationEntry);
                options.Add(new IOptionData(data.Pointer));
            }

            OptionCategory category = new();
            category.Init(
                new CategoryData
                {
                    name = $"{plugin.Name} v{plugin.Version}", options = options, canBeOpened = false, notResettable = options.Count == 0, noAccountNeeded = true
                },
                true
            );
            visualElement.Add(category);

            AddStatusLineToCategory(plugin, category);
        }

        return visualElement;
    }

    static void AddStatusLineToCategory(DBIPlugin plugin, OptionCategory category)
    {
        VisualElement container = category.Q("ctr_categoryContent");
        switch (plugin.Status)
        {
            case PluginStatus.Started:
                AddLine(container, FigmaIcons.radioOn, Color.green, "Started", DofusUiConstants.TextWhite100);
                break;
            case PluginStatus.FailedToStart:
                AddLine(container, FigmaIcons.circleCross, Color.red, "Failed to start", DofusUiConstants.TextLightRed100);
                break;
            case PluginStatus.NotStarted:
                AddLine(container, FigmaIcons.radioOff, Color.gray, "Not started", DofusUiConstants.TextWhite65);
                break;
            default:
                AddLine(container, FigmaIcons.questionMark, "Unknown status");
                break;
        }
    }

    VisualElement CreateSettingsTab()
    {
        DBIConfiguration.Entry[] entries = DBI.Configuration.GetAll().Where(e => !e.Hidden).ToArray();
        IEnumerable<string> categories = entries.Select(e => e.Category).Distinct();

        VisualElement visualElement = new();
        visualElement.style.display = DisplayStyle.None;

        foreach (string category in categories)
        {
            Il2CppSystem.Collections.Generic.List<IOptionData> options = new();

            IEnumerable<DBIConfiguration.Entry> entriesInCategory = entries.Where(e => e.Category == category);
            foreach (DBIConfiguration.Entry entry in entriesInCategory)
            {
                switch (entry)
                {
                    case DBIConfiguration.Entry<bool> boolEntry:
                    {
                        BoolOption option = CreateOptionData(boolEntry);
                        options.Add(new IOptionData(option.Pointer));
                        break;
                    }
                    default:
                    {
                        if (entry.AcceptableValuesDescriptions is { Count: > 0 })
                        {
                            MultipleChoiceOption option = CreateMultipleChoiceOptionData(entry);
                            options.Add(new IOptionData(option.Pointer));
                        }
                        else
                        {
                            Log.LogWarning("Configuration type not implemented, will display option as is.");

                            TextButtonOption option = new((Action)(() => { }))
                            {
                                text = $"{entry.Key}: {entry.CurrentValueDescription}",
                                description = new DescriptionData { text = entry.Description?.Description }
                            };
                            options.Add(new IOptionData(option.Pointer));
                        }
                        break;
                    }
                }
            }

            OptionCategory c = new();
            c.Init(new CategoryData { name = category, options = options }, true);

            visualElement.Add(c);
        }

        return visualElement;
    }

    CategorySummaryItem CreateItem(Category category, VisualElement sidePanel)
    {
        string name = GetName(category);
        string displayName = GetDisplayName(category);

        CategorySummaryItem item = new();
        item.Init(name, displayName, false, false, SectionHeader.SectionHeaderDepth.one, (Action<string>)SelectCategory, (Action<string>)(str => { }));
        sidePanel.Add(item);

        return item;
    }

    protected override void OnOpen() => SelectCategory(Category.General);

    void SelectCategory(string category)
    {
        foreach (Category c in Enum.GetValues<Category>())
        {
            if (GetName(c) == category)
            {
                SelectCategory(c);
                return;
            }
        }
    }

    string GetName(Category category) =>
        category switch
        {
            Category.General => "general",
            Category.Settings => "settings",
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
        };

    string GetDisplayName(Category category) =>
        category switch
        {
            Category.General => "General",
            Category.Settings => "Settings",
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
        };

    CategorySummaryItem GetItem(Category category) =>
        category switch
        {
            Category.General => _generalItem,
            Category.Settings => _settingsItem,
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
        };

    VisualElement GetTab(Category category) =>
        category switch
        {
            Category.General => _generalTab,
            Category.Settings => _settingsTab,
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
        };

    static BoolOption CreateOptionData(DBIConfiguration.Entry<bool> entry)
    {
        BoolOption option = new(null, new Option<bool>(entry.DefaultValue) { m_value = entry.Value })
        {
            text = entry.Key,
            description = new DescriptionData { text = entry.Description?.Description },
            valueChangedCallback = (Action<bool>)entry.Set
        };
        return option;
    }

    static MultipleChoiceOption CreateMultipleChoiceOptionData(DBIConfiguration.Entry entry)
    {
        Thread.Sleep(1000);

        IReadOnlyList<ValueDescription> values = entry.AcceptableValuesDescriptions;
        ValueDescription value = entry.CurrentValueDescription;
        int valueIndex = values.Select((v, i) => new { Value = v, Index = i }).FirstOrDefault(v => v.Value.Name == value.Name)?.Index ?? 0;
        ValueDescription defaultValue = entry.DefaultValueDescription;
        int defaultValueIndex = values.Select((v, i) => new { Value = v, Index = i }).FirstOrDefault(v => v.Value.Name == defaultValue.Name)?.Index ?? 0;

        Il2CppSystem.Collections.Generic.List<Il2CppSystem.ValueTuple<int, string>> choices = new();
        for (int i = 0; i < values.Count; i++)
        {
            Il2CppSystem.ValueTuple<int, string> choice = new(i, values[i].DisplayName);
            choices.System_Collections_IList_Add(choice);
        }

        Option<int> dataSource = new(defaultValueIndex) { m_value = valueIndex, m_onChanged = (Action<int, int>)((_, newValue) => entry.SetValueWithName(values[newValue].Name)) };
        MultipleChoiceOption option = new(dataSource, null)
        {
            type = OptionType.MultipleChoice,
            text = entry.Key,
            description = new DescriptionData { text = entry.Description?.Description },
            choices = choices
        };
        return option;
    }

    static void AddLine(VisualElement container, FigmaIcons icon, string text) => AddLine(container, icon, Color.white, text, DofusUiConstants.TextWhite100);

    static void AddLine(VisualElement container, FigmaIcons icon, Color iconColor, string text, string statusColor)
    {
        DofusVisualElement element = new() { gapValue = 4, style = { display = DisplayStyle.Flex, flexDirection = FlexDirection.Row, paddingLeft = 4 } };

        element.Add(new DofusIcon { icon = icon, style = { width = 20, height = 20 }, color = iconColor });

        DofusLabel dofusLabel = new() { text = text };
        dofusLabel.AddToClassList(DofusUiConstants.TextShortLargeRegular);
        dofusLabel.AddToClassList(statusColor);
        element.Add(dofusLabel);

        container.Add(element);
    }

    public enum Category
    {
        General,
        Settings
    }
}
