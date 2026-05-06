// ResearchProjectWorkScreen.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
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

        if (_vm is not null)
            _vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is nameof(ResearchProjectWorkViewModel.InlineResult))
                    WireResultPropertyChanged();
            };
    }

    private void WireResultPropertyChanged()
    {
        var resultVm = _vm?.InlineResult;
        if (resultVm is null) return;
        resultVm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(ResearchResultViewModel.ExportMessage)
                                or nameof(ResearchResultViewModel.ExportIsError)
                                or nameof(ResearchResultViewModel.HasExportMessage))
                UpdateExportFeedback();
        };
    }

    private void UpdateExportFeedback()
    {
        var resultVm = _vm?.InlineResult;
        if (resultVm is null) return;
        var block = this.FindControl<TextBlock>("ExportFeedbackBlock");
        if (block is null) return;
        block.IsVisible  = resultVm.HasExportMessage;
        block.Foreground = resultVm.ExportIsError
            ? new SolidColorBrush(Colors.Red)
            : new SolidColorBrush(Colors.Green);
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

    private async void OnStartClicked(object? sender, RoutedEventArgs e)
        => await (_vm?.StartAsync() ?? Task.CompletedTask);

    private void OnCancelClicked(object? sender, RoutedEventArgs e) => _vm?.Cancel();

    private void OnResultToggleClicked(object? sender, RoutedEventArgs e)
    {
        var resultVm = _vm?.InlineResult;
        if (resultVm is null) return;
        resultVm.Proportional = !resultVm.Proportional;
    }

    private async void OnResultExportClicked(object? sender, RoutedEventArgs e)
    {
        var resultVm = _vm?.InlineResult;
        if (resultVm is null) return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        var options = new FilePickerSaveOptions
        {
            Title             = resultVm.LabelExport,
            SuggestedFileName = resultVm.GetDefaultExportFileName(),
            DefaultExtension  = "csv",
            FileTypeChoices   = new List<FilePickerFileType>
            {
                new("CSV") { Patterns = ["*.csv"] }
            }
        };

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(options);
        if (file is null) return;

        var path = file.Path.LocalPath;
        try
        {
            resultVm.ExportTo(path);
            resultVm.SetExportResult(path, isError: false);
        }
        catch (Exception ex)
        {
            resultVm.SetExportResult(ex.Message, isError: true);
        }
    }

    private void OnResultHelpClicked(object? sender, RoutedEventArgs e)
    {
        var resultVm = _vm?.InlineResult;
        if (resultVm is null) return;
        var dialog = new Window
        {
            Title                 = resultVm.LabelTitle,
            Width                 = 420,
            Height                = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content               = new TextBlock
            {
                Text         = resultVm.TooltipHelp,
                Margin       = new Avalonia.Thickness(16),
                TextWrapping = TextWrapping.Wrap
            }
        };
        var top = TopLevel.GetTopLevel(this) as Window;
        if (top is not null) dialog.ShowDialog(top);
        else dialog.Show();
    }
}
