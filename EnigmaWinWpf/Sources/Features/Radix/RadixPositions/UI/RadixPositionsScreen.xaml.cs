// RadixPositionsScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;

namespace EnigmaWin.Sources.Features.Radix.RadixPositions.UI;

public partial class RadixPositionsScreen : UserControl
{
    private const double CompactWidthThreshold = 700;
    private readonly RadixPositionsViewModel _viewModel;
    private IChartSession? _chartSession;

    public RadixPositionsScreen()
    {
        var rosetta = (Application.Current as App)?.Services.GetRequiredService<IRosetta>()
                      ?? throw new InvalidOperationException("IRosetta not available");
        var configContext = (Application.Current as App)?.Services.GetService<IConfigContext>();
        _viewModel = new RadixPositionsViewModel(rosetta, configContext);

        InitializeComponent();
        DataContext = _viewModel;

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;

        ResolveChartSession();
        if (_chartSession != null)
            _viewModel.LoadChart(_chartSession.SelectedChart);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ResolveChartSession();
        if (_chartSession is INotifyPropertyChanged notify)
            notify.PropertyChanged += OnSessionPropertyChanged;
        if (_chartSession != null)
            _viewModel.LoadChart(_chartSession.SelectedChart);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (_chartSession is INotifyPropertyChanged notify)
            notify.PropertyChanged -= OnSessionPropertyChanged;
    }

    private void OnSessionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(IChartSession.SelectedChart) || _chartSession == null)
            return;

        Application.Current.Dispatcher.BeginInvoke(() => _viewModel.LoadChart(_chartSession.SelectedChart));
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        _viewModel.IsWideLayout = e.NewSize.Width >= CompactWidthThreshold;
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var msg = _viewModel.TooltipHelp;
        MessageBox.Show(Window.GetWindow(this), msg, "Help", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ResolveChartSession()
    {
        if (_chartSession != null) return;
        if (Application.Current is App app)
            _chartSession = app.Services.GetService<IChartSession>();
    }
}
