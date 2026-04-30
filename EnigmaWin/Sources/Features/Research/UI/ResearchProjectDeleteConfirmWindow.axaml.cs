// ResearchProjectDeleteConfirmWindow.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace EnigmaWin.Sources.Features.Research.UI;

public partial class ResearchProjectDeleteConfirmWindow : Window
{
    public string LabelTitle   { get; }
    public string Message      { get; }
    public string LabelConfirm { get; }
    public string LabelCancel  { get; }

    public ResearchProjectDeleteConfirmWindow(
        string title, string message, string labelConfirm, string labelCancel)
    {
        LabelTitle   = title;
        Message      = message;
        LabelConfirm = labelConfirm;
        LabelCancel  = labelCancel;

        Title = title;
        InitializeComponent();
        DataContext = this;
    }

    private void OnConfirmClicked(object? sender, RoutedEventArgs e) => Close(true);
    private void OnCancelClicked(object? sender, RoutedEventArgs e)  => Close(false);
}
