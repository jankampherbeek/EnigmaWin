// AboutWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.About.UI;

public partial class AboutWindow : Window
{
    public string LabelDescription  { get; }
    public string LabelContributors { get; }
    public string LabelNames        { get; }
    public string LabelClose        { get; }

    public AboutWindow(IRosetta rosetta)
    {
        Title            = rosetta.GetText(RbFile.About, AboutKeys.Title);
        LabelDescription  = rosetta.GetText(RbFile.About, AboutKeys.Description);
        LabelContributors = rosetta.GetText(RbFile.About, AboutKeys.Contributors);
        LabelNames        = rosetta.GetText(RbFile.About, AboutKeys.Names);
        LabelClose        = rosetta.GetText(RbFile.About, AboutKeys.Close);
        InitializeComponent();
        DataContext = this;
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();
}
