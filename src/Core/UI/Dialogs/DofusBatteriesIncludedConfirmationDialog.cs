using System;
using Ankama.AddressableUtilities.Runtime;
using Core.UILogic.Components.Figma;
using DofusBatteriesIncluded.Plugins.Core.UI.Windows;
using UnityEngine.UIElements;

namespace DofusBatteriesIncluded.Plugins.Core.UI.Dialogs;

public class DofusBatteriesIncludedConfirmationDialog : DofusBatteriesIncludedWindow
{
    Label _messageLabel;
    DofusButtonCustom _noButton;
    DofusButtonCustom _yesButton;
    ConfirmationDialogConfiguration _configuration;

    protected override string Name => "Confirmation Dialog";
    protected override bool HasCloseButton => false;
    protected override bool HasBackdrop => true;

    protected override void Build(WindowFigma window)
    {
        base.Build(window);
        _window.style.maxWidth = 500;
        _window.style.minWidth = 330;
        _window.style.height = new StyleLength(StyleKeyword.Auto);
        _window.style.maxHeight = new Length(50, Length.Unit.Percent);
        _window.isMovable = false;

        VisualElement messageContainer = new() { style = { paddingLeft = 16, paddingRight = 16, paddingTop = 16, paddingBottom = 24 } };
        _messageLabel = new Label
            { text = "Confirmation message", enableRichText = true, style = { overflow = Overflow.Hidden, textOverflow = TextOverflow.Ellipsis, whiteSpace = WhiteSpace.Normal } };
        _messageLabel.AddToClassList("LABEL_Medium");
        _messageLabel.AddToClassList("normal2_p");
        messageContainer.Add(_messageLabel);
        _window.contentContainer.Add(messageContainer);

        DofusVisualElement buttonsContainer = new()
            { gapValue = 16, style = { flexDirection = FlexDirection.Row, alignItems = Align.Stretch, paddingLeft = 16, paddingRight = 16, paddingTop = 16, paddingBottom = 16 } };
        buttonsContainer.AddToClassList("Footer");
        _noButton = new DofusButtonCustom((Action)(() => Confirm(false)), "No", new AddressableEntry())
        {
            size = DofusButtonCustom.SizeEnum.large, status = DofusButtonCustom.ButtonStatusEnum.normal, mainStyle = DofusButtonCustom.ComponentStyleEnum.secondary,
            style = { flexGrow = 1 }
        };
        _noButton.AddToClassList("secondary");
        _noButton.AddToClassList("normal");
        _noButton.AddToClassList("large");
        _noButton.AddToClassList("text");
        buttonsContainer.Add(_noButton);
        _yesButton = new DofusButtonCustom((Action)(() => Confirm(true)), "Yes", new AddressableEntry())
        {
            size = DofusButtonCustom.SizeEnum.large, status = DofusButtonCustom.ButtonStatusEnum.normal, mainStyle = DofusButtonCustom.ComponentStyleEnum.primary,
            style = { flexGrow = 1 }
        };
        _yesButton.AddToClassList("primary");
        _yesButton.AddToClassList("normal");
        _yesButton.AddToClassList("large");
        _yesButton.AddToClassList("text");
        buttonsContainer.Add(_yesButton);
        _window.contentContainer.Add(buttonsContainer);
    }

    public void Configure(ConfirmationDialogConfiguration configuration)
    {
        _configuration = configuration;
        _window.header.title = configuration.Title;
        _messageLabel.text = configuration.Message;
        _noButton.text = configuration.NoLabel;
        _yesButton.text = configuration.YesLabel;
    }

    void Confirm(bool result)
    {
        _configuration?.ClosedCallback?.Invoke(result);
        Close();
    }
}

public class ConfirmationDialogConfiguration
{
    public string Title { get; set; } = "Confirmation";
    public string Message { get; set; } = "Are you sure?";
    public string NoLabel { get; set; } = "No";
    public string YesLabel { get; set; } = "Yes";
    public Action<bool> ClosedCallback { get; set; }
}
