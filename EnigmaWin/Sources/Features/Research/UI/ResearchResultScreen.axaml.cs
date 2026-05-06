// ResearchResultScreen.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using EnigmaWin.ViewModels.Routes;

namespace EnigmaWin.Sources.Features.Research.UI;

public partial class ResearchResultScreen : UserControl
{
    private ResearchResultViewModel? _vm;

    public ResearchResultScreen()
    {
        if (!Design.IsDesignMode && Application.Current is App app)
        {
            var routeVm = ResearchResultRouteViewModel.Pending;
            if (routeVm?.Result is not null && routeVm.Project is not null)
            {
                var rosetta           = app.Services.GetRequiredService<IRosetta>();
                var navigationService = app.Services.GetRequiredService<INavigationService>();
                _vm = new ResearchResultViewModel(
                    rosetta,
                    navigationService,
                    routeVm.Result,
                    routeVm.Project,
                    routeVm.CgMultiplier);
                DataContext = _vm;
            }
        }

        InitializeComponent();

        if (_vm is not null)
            _vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is nameof(ResearchResultViewModel.ExportMessage)
                                    or nameof(ResearchResultViewModel.ExportIsError)
                                    or nameof(ResearchResultViewModel.HasExportMessage))
                    UpdateExportFeedback();
            };
    }

    private void OnBackClicked(object? sender, RoutedEventArgs e)    => _vm?.Back();
    private void OnToggleClicked(object? sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        _vm.Proportional = !_vm.Proportional;
    }

    private async void OnExportClicked(object? sender, RoutedEventArgs e)
    {
        if (_vm is null) return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null) return;

        var options = new Avalonia.Platform.Storage.FilePickerSaveOptions
        {
            Title                  = _vm.LabelExport,
            SuggestedFileName      = _vm.GetDefaultExportFileName(),
            DefaultExtension       = "csv",
            FileTypeChoices        = new List<Avalonia.Platform.Storage.FilePickerFileType>
            {
                new("CSV") { Patterns = ["*.csv"] }
            }
        };

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(options);
        if (file is null) return;

        var path = file.Path.LocalPath;
        try
        {
            _vm.ExportTo(path);
            _vm.SetExportResult(path, isError: false);
        }
        catch (Exception ex)
        {
            _vm.SetExportResult(ex.Message, isError: true);
        }
    }

    private void OnHelpClicked(object? sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        var dialog = new Window
        {
            Title                 = _vm.LabelTitle,
            Width                 = 420,
            Height                = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content               = new TextBlock
            {
                Text         = _vm.TooltipHelp,
                Margin       = new Thickness(16),
                TextWrapping = TextWrapping.Wrap
            }
        };
        var top = TopLevel.GetTopLevel(this) as Window;
        if (top is not null) dialog.ShowDialog(top);
        else dialog.Show();
    }

    private void UpdateExportFeedback()
    {
        if (_vm is null) return;
        var block = this.FindControl<TextBlock>("ExportFeedbackBlock");
        if (block is null) return;
        block.IsVisible  = _vm.HasExportMessage;
        block.Foreground = _vm.ExportIsError
            ? new SolidColorBrush(Colors.Red)
            : new SolidColorBrush(Colors.Green);
    }
}
