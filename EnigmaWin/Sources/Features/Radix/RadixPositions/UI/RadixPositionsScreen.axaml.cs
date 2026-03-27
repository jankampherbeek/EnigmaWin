// RadixPositionsScreen.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
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

        ResolveChartSession();
        if (_chartSession != null)
            _viewModel.LoadChart(_chartSession.SelectedChart);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ResolveChartSession();

        if (_chartSession is INotifyPropertyChanged notify)
            notify.PropertyChanged += OnSessionPropertyChanged;

        if (_chartSession != null)
            _viewModel.LoadChart(_chartSession.SelectedChart);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_chartSession is INotifyPropertyChanged notify)
            notify.PropertyChanged -= OnSessionPropertyChanged;

        base.OnDetachedFromVisualTree(e);
    }

    private void OnSessionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(IChartSession.SelectedChart) || _chartSession == null)
            return;

        Dispatcher.UIThread.Post(() => _viewModel.LoadChart(_chartSession.SelectedChart));
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        _viewModel.IsWideLayout = e.NewSize.Width >= CompactWidthThreshold;
    }

    private async void OnHelpClicked(object? sender, RoutedEventArgs e)
    {
        if (Application.Current is not App app) return;
        var rosetta = app.Services.GetRequiredService<IRosetta>();
        var helpWindow = new RadixPositionsHelpWindow(rosetta);
        if (TopLevel.GetTopLevel(this) is Window owner)
            await helpWindow.ShowDialog(owner);
    }

    private void ResolveChartSession()
    {
        if (_chartSession != null || Design.IsDesignMode) return;
        if (Application.Current is App app)
            _chartSession = app.Services.GetService<IChartSession>();
    }
}
