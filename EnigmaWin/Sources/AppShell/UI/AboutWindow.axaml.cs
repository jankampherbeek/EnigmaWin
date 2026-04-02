// AboutWindow.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using Avalonia.Controls;
using Avalonia.Interactivity;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.AppShell.UI;

public partial class AboutWindow : Window
{
    public string LabelTitle  { get; }
    public string LabelIntro  { get; }
    public string LabelFree   { get; }
    public string LabelThanks { get; }
    public string LabelNames  { get; }
    public string LabelClose  { get; }

    public AboutWindow(IRosetta rosetta)
    {
        LabelTitle  = rosetta.GetText(RbFile.Localizable, "about.title");
        LabelIntro  = rosetta.GetText(RbFile.Localizable, "about.intro");
        LabelFree   = rosetta.GetText(RbFile.Localizable, "about.free");
        LabelThanks = rosetta.GetText(RbFile.Localizable, "about.thanks");
        LabelNames  = rosetta.GetText(RbFile.Localizable, "about.names");
        LabelClose  = rosetta.GetText(RbFile.Localizable, "about.close");
        Title       = rosetta.GetText(RbFile.Localizable, "about.title");

        InitializeComponent();
        DataContext = this;
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e) => Close();
}
