// ResearchProjectWorkScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using EnigmaWin.ViewModels.Routes;

namespace EnigmaWin.Sources.Features.Research.UI;

public partial class ResearchProjectWorkScreen : UserControl
{
    private ResearchProjectWorkViewModel? _vm;

    public ResearchProjectWorkScreen()
    {
        var app = (App)Application.Current;
        var routeVm = ResearchProjectOpenRouteViewModel.Pending;
        if (routeVm?.Project is not null)
        {
            var rosetta           = app.Services.GetRequiredService<IRosetta>();
            var navigationService = app.Services.GetRequiredService<INavigationService>();
            _vm = new ResearchProjectWorkViewModel(rosetta, navigationService, routeVm.Project);
            DataContext = _vm;
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
        var block = (TextBlock?)FindName("ExportFeedbackBlock");
        if (block is null) return;
        block.Visibility = resultVm.HasExportMessage ? Visibility.Visible : Visibility.Collapsed;
        block.Foreground = resultVm.ExportIsError
            ? new SolidColorBrush(Colors.Red)
            : new SolidColorBrush(Colors.Green);
    }

    private void OnBackClicked(object sender, RoutedEventArgs e) => _vm?.Back();

    private void OnSelectFileClicked(object sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        var dlg = new OpenFileDialog
        {
            Title         = _vm.LabelSelectFile,
            Multiselect   = false
        };
        if (dlg.ShowDialog() == true)
            _vm.SelectedFilePath = dlg.FileName;
    }

    private async void OnStartClicked(object sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        if (_vm.HasResult)
            _vm.Close();
        else
            await _vm.StartAsync();
    }

    private void OnCancelClicked(object sender, RoutedEventArgs e) => _vm?.Cancel();

    private void OnResultToggleClicked(object sender, RoutedEventArgs e)
    {
        var resultVm = _vm?.InlineResult;
        if (resultVm is null) return;
        resultVm.Proportional = !resultVm.Proportional;
    }

    private void OnResultExportClicked(object sender, RoutedEventArgs e)
    {
        var resultVm = _vm?.InlineResult;
        if (resultVm is null) return;

        var dlg = new SaveFileDialog
        {
            Title            = resultVm.LabelExport,
            FileName         = resultVm.GetDefaultExportFileName(),
            DefaultExt       = "csv",
            Filter           = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
        };

        if (dlg.ShowDialog() != true) return;
        var path = dlg.FileName;
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

    private void OnResultHelpClicked(object sender, RoutedEventArgs e)
    {
        var resultVm = _vm?.InlineResult;
        if (resultVm is null) return;
        var owner = Window.GetWindow(this);
        var dialog = new Window
        {
            Title                 = resultVm.LabelTitle,
            Width                 = 420,
            Height                = 200,
            ResizeMode            = ResizeMode.NoResize,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner                 = owner,
            Content               = new TextBlock
            {
                Text         = resultVm.TooltipHelp,
                Margin       = new Thickness(16),
                TextWrapping = TextWrapping.Wrap
            }
        };
        dialog.ShowDialog();
    }
}
