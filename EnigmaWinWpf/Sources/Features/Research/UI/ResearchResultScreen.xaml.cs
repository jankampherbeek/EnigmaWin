// ResearchResultScreen.xaml.cs
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

public partial class ResearchResultScreen : UserControl
{
    private ResearchResultViewModel? _vm;

    public ResearchResultScreen()
    {
        var app = (App)Application.Current;
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

    private void OnBackClicked(object sender, RoutedEventArgs e) => _vm?.Back();

    private void OnToggleClicked(object sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        _vm.Proportional = !_vm.Proportional;
    }

    private void OnExportClicked(object sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        var dlg = new SaveFileDialog
        {
            Title      = _vm.LabelExport,
            FileName   = _vm.GetDefaultExportFileName(),
            DefaultExt = "csv",
            Filter     = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
        };
        if (dlg.ShowDialog() != true) return;
        var path = dlg.FileName;
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

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        var owner = Window.GetWindow(this);
        var dialog = new Window
        {
            Title                 = _vm.LabelTitle,
            Width                 = 420,
            Height                = 200,
            ResizeMode            = ResizeMode.NoResize,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner                 = owner,
            Content               = new TextBlock
            {
                Text         = _vm.TooltipHelp,
                Margin       = new Thickness(16),
                TextWrapping = TextWrapping.Wrap
            }
        };
        dialog.ShowDialog();
    }

    private void UpdateExportFeedback()
    {
        if (_vm is null) return;
        var block = (TextBlock?)FindName("ExportFeedbackBlock");
        if (block is null) return;
        block.Visibility = _vm.HasExportMessage ? Visibility.Visible : Visibility.Collapsed;
        block.Foreground = _vm.ExportIsError
            ? new SolidColorBrush(Colors.Red)
            : new SolidColorBrush(Colors.Green);
    }
}
