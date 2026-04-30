// ResearchProjectsListScreen.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
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
        if (!Design.IsDesignMode && Application.Current is App app)
        {
            var rosetta           = app.Services.GetRequiredService<IRosetta>();
            var navigationService = app.Services.GetRequiredService<INavigationService>();
            var repository        = app.Services.GetRequiredService<IResearchProjectRepository>();

            _vm = new ResearchProjectsListViewModel(
                rosetta, navigationService, repository,
                ResearchProjectListPending.Mode);
            DataContext = _vm;
        }

        InitializeComponent();
    }

    protected override async void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_vm is not null)
            await _vm.LoadAsync();
    }

    private async void OnSearchClicked(object? sender, RoutedEventArgs e)
    {
        if (_vm is not null) await _vm.SearchAsync();
    }

    private async void OnSearchKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Return && _vm is not null)
            await _vm.SearchAsync();
    }

    private void OnOpenClicked(object? sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        if (sender is not Button btn || btn.Tag is not ProjectListItem item) return;
        _vm.OpenProject(item);
    }

    private async void OnDeleteClicked(object? sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        if (sender is not Button btn || btn.Tag is not ProjectListItem item) return;

        var confirmed = await ShowDeleteConfirmAsync(item);
        if (!confirmed) return;

        await _vm.ConfirmDeleteAsync(item);
    }

    private async Task<bool> ShowDeleteConfirmAsync(ProjectListItem item)
    {
        if (_vm is null) return false;
        if (TopLevel.GetTopLevel(this) is not Window owner) return false;

        var dialog = new ResearchProjectDeleteConfirmWindow(
            _vm.LabelDeleteTitle,
            _vm.DeleteConfirmMessage(item.Name),
            _vm.LabelDeleteConfirm,
            _vm.LabelDeleteCancel);

        return await dialog.ShowDialog<bool>(owner);
    }

    private void OnCancelClicked(object? sender, RoutedEventArgs e) => _vm?.Cancel();

    private async void OnHelpClicked(object? sender, RoutedEventArgs e)
    {
        if (Application.Current is not App app || _vm is null) return;
        var rosetta    = app.Services.GetRequiredService<IRosetta>();
        var helpWindow = new ResearchProjectsListHelpWindow(rosetta, _vm.Mode);
        if (TopLevel.GetTopLevel(this) is Window owner)
            await helpWindow.ShowDialog(owner);
    }
}
