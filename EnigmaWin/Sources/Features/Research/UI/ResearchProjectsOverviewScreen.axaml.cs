// ResearchProjectsOverviewScreen.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Research.UI;

public partial class ResearchProjectsOverviewScreen : UserControl
{
    private ResearchProjectsOverviewViewModel? _vm;

    public ResearchProjectsOverviewScreen()
    {
        InitializeComponent();

        if (Design.IsDesignMode || Application.Current is not App app)
            return;

        var rosetta            = app.Services.GetRequiredService<IRosetta>();
        var navigationService  = app.Services.GetRequiredService<INavigationService>();
        _vm = new ResearchProjectsOverviewViewModel(rosetta, navigationService);
        DataContext = _vm;
    }

    private void OnNewProjectClicked(object? sender, RoutedEventArgs e) => _vm?.NewProject();
    private void OnSearchProjectClicked(object? sender, RoutedEventArgs e) => _vm?.SearchProject();
    private void OnAllProjectsClicked(object? sender, RoutedEventArgs e) => _vm?.ShowAllProjects();

    private async void OnHelpClicked(object? sender, RoutedEventArgs e)
    {
        if (Application.Current is not App app) return;
        var rosetta = app.Services.GetRequiredService<IRosetta>();
        var helpWindow = new ResearchProjectsHelpWindow(rosetta);
        if (TopLevel.GetTopLevel(this) is Window owner)
            await helpWindow.ShowDialog(owner);
    }
}
