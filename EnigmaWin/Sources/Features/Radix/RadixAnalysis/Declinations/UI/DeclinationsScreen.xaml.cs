// DeclinationsScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations.UI;

public partial class DeclinationsScreen : UserControl
{
    private readonly DeclinationsScreenViewModel _viewModel;
    private IChartSession? _chartSession;

    public DeclinationsScreen()
    {
        var app           = (App)Application.Current;
        var rosetta       = app.Services.GetRequiredService<IRosetta>();
        var configContext = app.Services.GetRequiredService<IConfigContext>();

        _viewModel = new DeclinationsScreenViewModel(rosetta, configContext);
        InitializeComponent();
        DataContext = _viewModel;

        _chartSession = app.Services.GetService<IChartSession>();
        if (_chartSession is INotifyPropertyChanged notify)
            notify.PropertyChanged += OnSessionPropertyChanged;

        _viewModel.LoadChart(_chartSession?.SelectedChart);

        Loaded   += (_, _) => _viewModel.LoadChart(_chartSession?.SelectedChart);
        Unloaded += (_, _) =>
        {
            if (_chartSession is INotifyPropertyChanged n)
                n.PropertyChanged -= OnSessionPropertyChanged;
        };
    }

    private void OnSessionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(IChartSession.SelectedChart) || _chartSession == null) return;
        Application.Current.Dispatcher.BeginInvoke(() => _viewModel.LoadChart(_chartSession.SelectedChart));
    }
}
