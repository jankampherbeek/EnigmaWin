// BlaSchemaScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;

namespace EnigmaWin.Sources.Features.BlaSchema.UI;

public partial class BlaSchemaScreen : UserControl
{
    private readonly BlaSchemaViewModel _viewModel;
    private readonly IChartSession? _chartSession;
    private readonly IRosetta _rosetta;

    public BlaSchemaScreen()
    {
        var app = (App)Application.Current;
        var rosetta = app.Services.GetRequiredService<IRosetta>();
        var configContext = app.Services.GetRequiredService<IConfigContext>();
        var chartSession = app.Services.GetRequiredService<IChartSession>();

        _viewModel = new BlaSchemaViewModel(rosetta, configContext, chartSession);
        _chartSession = chartSession;
        _rosetta = rosetta;

        InitializeComponent();
        DataContext = _viewModel;

        if (_chartSession is INotifyPropertyChanged notify)
            notify.PropertyChanged += OnSessionPropertyChanged;

        Loaded += (_, _) => _viewModel.LoadChart(_chartSession?.Selected);
        Unloaded += (_, _) =>
        {
            if (_chartSession is INotifyPropertyChanged n) n.PropertyChanged -= OnSessionPropertyChanged;
        };
    }

    private void OnSessionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(IChartSession.SelectedChart) || _chartSession == null) return;
        Application.Current.Dispatcher.BeginInvoke(() => _viewModel.LoadChart(_chartSession.Selected));
    }

    private void OnFactsheetClicked(object sender, RoutedEventArgs e)
    {
        new BlaSchemaFactsheetWindow(_rosetta) { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        new BlaSchemaHelpWindow(_rosetta, _viewModel.CurrentSectionTitle, _viewModel.CurrentSectionHelpText)
        {
            Owner = Window.GetWindow(this)
        }.ShowDialog();
    }
}
