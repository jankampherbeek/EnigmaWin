// ResearchProjectInputScreen.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.Features.Research.ResearchProjects.Persistency;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Research.UI;

public partial class ResearchProjectInputScreen : UserControl
{
    private ResearchProjectInputViewModel? _vm;

    public ResearchProjectInputScreen()
    {
        if (!Design.IsDesignMode && Application.Current is App app)
        {
            var rosetta           = app.Services.GetRequiredService<IRosetta>();
            var navigationService = app.Services.GetRequiredService<INavigationService>();
            var repository        = app.Services.GetRequiredService<IResearchProjectRepository>();

            _vm = new ResearchProjectInputViewModel(rosetta, navigationService, repository);
            DataContext = _vm;
        }

        InitializeComponent();
    }

    private void OnCancelClicked(object? sender, RoutedEventArgs e) => _vm?.Cancel();

    private async void OnNextClicked(object? sender, RoutedEventArgs e)
    {
        if (_vm is not null) await _vm.ProceedAsync();
    }

    private async void OnSelectFolderClicked(object? sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title            = _vm.LabelSelectPath,
                AllowMultiple    = false
            });

        if (folders.Count > 0)
            _vm.SelectedPath = folders[0].Path.LocalPath;
    }

    private async void OnHelpClicked(object? sender, RoutedEventArgs e)
    {
        if (Application.Current is not App app) return;
        var rosetta = app.Services.GetRequiredService<IRosetta>();
        var helpWindow = new ResearchProjectInputHelpWindow(rosetta);
        if (TopLevel.GetTopLevel(this) is Window owner)
            await helpWindow.ShowDialog(owner);
    }
}
