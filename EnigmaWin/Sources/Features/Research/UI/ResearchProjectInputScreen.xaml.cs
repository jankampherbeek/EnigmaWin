// ResearchProjectInputScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.Features.Research.ResearchProjects.Persistency;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Research.UI;

public partial class ResearchProjectInputScreen : UserControl
{
    private ResearchProjectInputViewModel? _vm;

    public ResearchProjectInputScreen()
    {
        var app = (App)Application.Current;
        var rosetta           = app.Services.GetRequiredService<IRosetta>();
        var navigationService = app.Services.GetRequiredService<INavigationService>();
        var repository        = app.Services.GetRequiredService<IResearchProjectRepository>();
        _vm = new ResearchProjectInputViewModel(rosetta, navigationService, repository);
        DataContext = _vm;
        InitializeComponent();
    }

    private void OnCancelClicked(object sender, RoutedEventArgs e) => _vm?.Cancel();

    private async void OnNextClicked(object sender, RoutedEventArgs e)
    {
        if (_vm is not null) await _vm.ProceedAsync();
    }

    private void OnSelectFolderClicked(object sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        var dlg = new OpenFolderDialog { Title = _vm.LabelSelectPath };
        if (dlg.ShowDialog() == true)
            _vm.SelectedPath = dlg.FolderName;
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        MessageBox.Show(_vm.TooltipHelp, _vm.LabelTitle, MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
