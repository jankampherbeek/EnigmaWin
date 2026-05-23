// ResearchProjectsOverviewScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Research.UI;

public partial class ResearchProjectsOverviewScreen : UserControl
{
    private ResearchProjectsOverviewViewModel? _vm;

    public ResearchProjectsOverviewScreen()
    {
        var app = (App)Application.Current;
        var rosetta           = app.Services.GetRequiredService<IRosetta>();
        var navigationService = app.Services.GetRequiredService<INavigationService>();
        _vm = new ResearchProjectsOverviewViewModel(rosetta, navigationService);
        DataContext = _vm;
        InitializeComponent();
    }

    private void OnNewProjectClicked(object sender, RoutedEventArgs e)    => _vm?.NewProject();
    private void OnSearchProjectClicked(object sender, RoutedEventArgs e) => _vm?.SearchProject();
    private void OnAllProjectsClicked(object sender, RoutedEventArgs e)   => _vm?.ShowAllProjects();

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        MessageBox.Show(_vm.TooltipHelp, _vm.LabelTitle, MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
