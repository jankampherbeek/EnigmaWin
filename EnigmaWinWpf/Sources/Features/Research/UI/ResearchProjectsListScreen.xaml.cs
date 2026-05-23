// ResearchProjectsListScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.Features.Research.ResearchProjects.Persistency;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using EnigmaWin.ViewModels.Routes;

namespace EnigmaWin.Sources.Features.Research.UI;

public partial class ResearchProjectsListScreen : UserControl
{
    private ResearchProjectsListViewModel? _vm;

    public ResearchProjectsListScreen()
    {
        var app = (App)Application.Current;
        var rosetta           = app.Services.GetRequiredService<IRosetta>();
        var navigationService = app.Services.GetRequiredService<INavigationService>();
        var repository        = app.Services.GetRequiredService<IResearchProjectRepository>();

        _vm = new ResearchProjectsListViewModel(
            rosetta, navigationService, repository, ResearchProjectListPending.Mode);
        DataContext = _vm;
        InitializeComponent();
        Loaded += async (_, _) => { if (_vm is not null) await _vm.LoadAsync(); };
    }

    private async void OnSearchClicked(object sender, RoutedEventArgs e)
    {
        if (_vm is not null) await _vm.SearchAsync();
    }

    private async void OnSearchKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Return && _vm is not null)
            await _vm.SearchAsync();
    }

    private void OnOpenClicked(object sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        if (sender is not Button btn || btn.Tag is not ProjectListItem item) return;
        _vm.OpenProject(item);
    }

    private async void OnDeleteClicked(object sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        if (sender is not Button btn || btn.Tag is not ProjectListItem item) return;

        var result = MessageBox.Show(
            _vm.DeleteConfirmMessage(item.Name),
            _vm.LabelDeleteTitle,
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;
        await _vm.ConfirmDeleteAsync(item);
    }

    private void OnCancelClicked(object sender, RoutedEventArgs e) => _vm?.Cancel();

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        MessageBox.Show(_vm.TooltipHelp, _vm.LabelTitle, MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
