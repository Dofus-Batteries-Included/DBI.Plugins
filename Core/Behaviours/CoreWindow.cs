using System;
using System.Collections.Generic;
using System.Linq;
using Core.Engine.Options;
using Core.UILogic.Components.Figma;
using Core.UILogic.Config;
using Microsoft.Extensions.Logging;
using UnityEngine.UIElements;

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

    VisualElement CreateSettingsTab()
    {
        DBIConfiguration.Entry[] entries = DBI.Configuration.GetAll().ToArray();
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
                        BoolOption option = new(null, new Option<bool>(boolEntry.DefaultValue) { m_value = boolEntry.Value })
                        {
                            text = entry.Key,
                            description = new DescriptionData { text = entry.Description.Description },
                            valueChangedCallback = (Action<bool>)(newValue => DBI.Configuration.Set(entry.Category, entry.Key, newValue))
                        };
                        options.Add(new IOptionData(option.Pointer));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(entry));
                }
            }

            OptionCategory c = new();
            c.Init(new CategoryData { name = category, options = options }, true);

            visualElement.Add(c);
        }

        return visualElement;
    }

    VisualElement CreateGeneralTab()
    {
        VisualElement visualElement = new();
        visualElement.style.display = DisplayStyle.None;

        OptionCategory pluginsHeader = new();
        pluginsHeader.Init(new CategoryData { name = "Plugins" }, false);
        visualElement.Add(pluginsHeader);

        foreach (DBIPlugin plugin in DBI.Plugins.GetAll())
        {
            OptionCategory category = new();
            category.Init(
                new CategoryData { name = $"{plugin.Name} v{plugin.Version}", canBeOpened = false, notResettable = true, noAccountNeeded = true, isSubCategory = true },
                true
            );
            visualElement.Add(category);
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

    public enum Category
    {
        General,
        Settings
    }
}
