// RadixAspectsScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
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
        var app = (App)Application.Current;
        var rosetta       = app.Services.GetRequiredService<IRosetta>();
        var configContext = app.Services.GetRequiredService<IConfigContext>();

        _viewModel = new RadixAspectsViewModel(rosetta, configContext);
        _viewModel.GridDataReady += OnGridDataReady;

        InitializeComponent();
        DataContext = _viewModel;

        _chartSession = app.Services.GetService<IChartSession>();
        if (_chartSession is INotifyPropertyChanged notify)
            notify.PropertyChanged += OnSessionPropertyChanged;

        _viewModel.LoadChart(_chartSession?.SelectedChart);

        Loaded   += (_, _) => _viewModel.LoadChart(_chartSession?.SelectedChart);
        Unloaded += (_, _) =>
        {
            if (_chartSession is INotifyPropertyChanged n) n.PropertyChanged -= OnSessionPropertyChanged;
        };
    }

    private void OnSessionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(IChartSession.SelectedChart) || _chartSession == null) return;
        Application.Current.Dispatcher.BeginInvoke(() => _viewModel.LoadChart(_chartSession.SelectedChart));
    }

    private void OnGridDataReady(List<Factors> factors, List<AspectGridControl.AspectCell> cells)
    {
        Application.Current.Dispatcher.BeginInvoke(() => AspectGrid.SetData(factors, cells));
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var app = (App)Application.Current;
        var rosetta = app.Services.GetRequiredService<IRosetta>();
        var helpWindow = new RadixAspectsHelpWindow(rosetta)
        {
            Owner = Window.GetWindow(this)
        };
        helpWindow.ShowDialog();
    }
}
