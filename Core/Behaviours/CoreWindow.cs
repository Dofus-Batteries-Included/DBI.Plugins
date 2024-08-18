using System;
using System.Collections.Generic;
using System.Linq;
using Core.UILogic.Components.Figma;
using Core.UILogic.Config;
using Microsoft.Extensions.Logging;
using UnityEngine.UIElements;

namespace DofusBatteriesIncluded.Core.Behaviours;

public class CoreWindow : DofusBatteriesIncludedWindow
{
    static readonly ILogger Log = DBI.Logging.Create<DofusBatteriesIncludedWindow>();
    protected override string Name => "Dofus Batteries Included";

    readonly List<Category> _categories =
    [
        new Category("core", "Core"),
        new Category("test", "Test category"),
        new Category("other-test", "Other Test category")
    ];

    readonly Dictionary<string, CategorySummaryItem> _items = [];
    string _currentCategory;

    protected override void Build(WindowFigma window)
    {
        base.Build(window);

        VisualElement container = new();
        container.style.display = DisplayStyle.Flex;
        container.style.width = new Length(30, Length.Unit.Percent);
        container.style.height = new Length(100, Length.Unit.Percent);
        window.content.Add(container);

        VisualElement sidePanel = new();
        sidePanel.style.display = DisplayStyle.Flex;
        sidePanel.style.flexDirection = FlexDirection.Column;
        sidePanel.style.height = new Length(100, Length.Unit.Percent);
        sidePanel.classList.Add("backgroundColor_background_medium90");
        ScrollView scrollView = new();
        scrollView.showHorizontal = false;
        foreach (Category category in _categories)
        {
            CategorySummaryItem item = new();
            item.Init(category.Name, category.DisplayName, false, false, SectionHeader.SectionHeaderDepth.one, (Action<string>)SelectCategory, (Action<string>)(str => { }));
            _items[category.Name] = item;
            sidePanel.Add(item);
        }
        sidePanel.Add(scrollView);
        container.Add(sidePanel);

        VisualElement content = new();
        container.Add(content);
    }

    protected override void OnOpen()
    {
        Category first = _categories.FirstOrDefault();
        if (first != null)
        {
            SelectCategory(first.Name);
        }
    }

    public void SelectCategory(string name)
    {
        if (_currentCategory != null)
        {
            CategorySummaryItem item = _items.GetValueOrDefault(_currentCategory);
            item.isSelected = false;
        }

        _currentCategory = name;

        if (name != null)
        {
            CategorySummaryItem item = _items.GetValueOrDefault(_currentCategory);
            item.isSelected = true;
        }
    }

    public class Category
    {
        public Category(string name, string displayName)
        {
            Name = name;
            DisplayName = displayName;
        }

        public string Name { get; }
        public string DisplayName { get; }
    }
}
