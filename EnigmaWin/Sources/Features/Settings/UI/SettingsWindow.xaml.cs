// SettingsWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using Microsoft.Win32;

namespace EnigmaWin.Sources.Features.Settings.UI;

public partial class SettingsWindow : Window
{
    private readonly SettingsViewModel _vm;

    public SettingsWindow(SettingsViewModel vm)
    {
        _vm = vm;
        DataContext = vm;
        InitializeComponent();
    }

    private void OnChooseResearchFolderClicked(object sender, RoutedEventArgs e)
    {
        var folder = ChooseFolder(_vm.LabelResearchFolder, _vm.ResearchFolderStart);
        if (folder is not null) _vm.SetResearchFolder(folder);
    }

    private void OnChooseLogFolderClicked(object sender, RoutedEventArgs e)
    {
        var folder = ChooseFolder(_vm.LabelLogFile, _vm.LogFolderStart);
        if (folder is not null) _vm.SetLogFolder(folder);
    }

    private string? ChooseFolder(string title, string? initialFolder)
    {
        var dlg = new OpenFolderDialog { Title = title, InitialDirectory = initialFolder ?? string.Empty };
        return dlg.ShowDialog(this) == true ? dlg.FolderName : null;
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();
}
