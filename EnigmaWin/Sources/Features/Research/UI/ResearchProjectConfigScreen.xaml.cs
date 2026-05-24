// ResearchProjectConfigScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
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
        var app = (App)Application.Current;
        var draft = ResearchProjectConfigRouteViewModel.PendingDraft;
        if (draft is not null)
        {
            var rosetta           = app.Services.GetRequiredService<IRosetta>();
            var navigationService = app.Services.GetRequiredService<INavigationService>();
            var repository        = app.Services.GetRequiredService<IResearchProjectRepository>();
            var configContext     = app.Services.GetRequiredService<IConfigContext>();
            _vm = new ResearchProjectConfigViewModel(rosetta, navigationService, repository, configContext, draft);
            DataContext = _vm;
        }
        InitializeComponent();
    }

    private void OnCancelClicked(object sender, RoutedEventArgs e) => _vm?.Cancel();

    private async void OnSaveClicked(object sender, RoutedEventArgs e)
    {
        if (_vm is not null) await _vm.SaveAsync();
    }

    private void OnFactorToggled(object sender, RoutedEventArgs e)
    {
        _vm?.NotifyFactorSelectionChanged();
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        MessageBox.Show(_vm.TooltipHelp, _vm.LabelTitle, MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
