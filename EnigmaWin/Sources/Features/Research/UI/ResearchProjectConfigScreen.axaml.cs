// ResearchProjectConfigScreen.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Features.Research.ResearchProjects.Persistency;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using EnigmaWin.ViewModels.Routes;

namespace EnigmaWin.Sources.Features.Research.UI;

public partial class ResearchProjectConfigScreen : UserControl
{
    private ResearchProjectConfigViewModel? _vm;

    public ResearchProjectConfigScreen()
    {
        if (!Design.IsDesignMode && Application.Current is App app)
        {
            var draft = ResearchProjectConfigRouteViewModel.PendingDraft
                        ?? new ResearchProjectDraft("", "", default, 1, "");

            var rosetta           = app.Services.GetRequiredService<IRosetta>();
            var navigationService = app.Services.GetRequiredService<INavigationService>();
            var repository        = app.Services.GetRequiredService<IResearchProjectRepository>();
            var configContext     = app.Services.GetRequiredService<IConfigContext>();

            _vm = new ResearchProjectConfigViewModel(rosetta, navigationService, repository, configContext, draft);
            DataContext = _vm;
        }

        InitializeComponent();
    }

    private void OnCancelClicked(object? sender, RoutedEventArgs e) => _vm?.Cancel();

    private async void OnSaveClicked(object? sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        try
        {
            await _vm.SaveAsync();
        }
        catch (Exception ex)
        {
            _vm.ErrorMessage = $"Save failed: {ex.Message}";
        }
    }

    private void OnFactorToggled(object? sender, RoutedEventArgs e) =>
        _vm?.NotifyFactorSelectionChanged();

    private async void OnHelpClicked(object? sender, RoutedEventArgs e)
    {
        if (Application.Current is not App app) return;
        var rosetta    = app.Services.GetRequiredService<IRosetta>();
        var helpWindow = new ResearchProjectConfigHelpWindow(rosetta);
        if (TopLevel.GetTopLevel(this) is Window owner)
            await helpWindow.ShowDialog(owner);
    }
}
