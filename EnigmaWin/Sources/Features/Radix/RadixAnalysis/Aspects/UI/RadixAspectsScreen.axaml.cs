// RadixAspectsScreen.axaml.cs
// EnigmaWin
// Created by Jan Kampherbeek on 26-03-2026

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Aspects.UI;

public partial class RadixAspectsScreen : UserControl
{
    private readonly RadixAspectsViewModel _viewModel;
    private IChartSession? _chartSession;

    public RadixAspectsScreen()
    {
        var rosetta = (Application.Current as App)?.Services.GetRequiredService<IRosetta>()
                      ?? throw new InvalidOperationException("IRosetta not available");
        var configContext = (Application.Current as App)?.Services.GetRequiredService<IConfigContext>()
                            ?? throw new InvalidOperationException("IConfigContext not available");

        _viewModel = new RadixAspectsViewModel(rosetta, configContext);
        _viewModel.GridDataReady += OnGridDataReady;

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

    private void OnGridDataReady(List<Factors> factors, List<AspectGridControl.AspectCell> cells)
    {
        Dispatcher.UIThread.Post(() => AspectGrid.SetData(factors, cells));
    }

    private async void OnHelpClicked(object? sender, RoutedEventArgs e)
    {
        if (Application.Current is not App app) return;
        var rosetta = app.Services.GetRequiredService<IRosetta>();
        var helpWindow = new RadixAspectsHelpWindow(rosetta);
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
