// ResearchProjectWorkScreen.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using EnigmaWin.ViewModels.Routes;

namespace EnigmaWin.Sources.Features.Research.UI;

public partial class ResearchProjectWorkScreen : UserControl
{
    private ResearchProjectWorkViewModel? _vm;

    public ResearchProjectWorkScreen()
    {
        if (!Design.IsDesignMode && Application.Current is App app)
        {
            var routeVm = ResearchProjectOpenRouteViewModel.Pending;
            if (routeVm?.Project is not null)
            {
                var rosetta           = app.Services.GetRequiredService<IRosetta>();
                var navigationService = app.Services.GetRequiredService<INavigationService>();
                _vm = new ResearchProjectWorkViewModel(rosetta, navigationService, routeVm.Project);
                DataContext = _vm;
            }
        }

        InitializeComponent();
    }

    private void OnBackClicked(object? sender, RoutedEventArgs e) => _vm?.Back();

    private async void OnSelectFileClicked(object? sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                Title         = _vm.LabelSelectFile,
                AllowMultiple = false
            });

        if (files.Count > 0)
            _vm.SelectedFilePath = files[0].Path.LocalPath;
    }

    private void OnStartClicked(object? sender, RoutedEventArgs e)
    {
        // Pipeline execution is future work — placeholder.
    }
}
