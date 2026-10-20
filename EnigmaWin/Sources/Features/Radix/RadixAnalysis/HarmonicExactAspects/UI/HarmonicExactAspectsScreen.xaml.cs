// HarmonicExactAspectsScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.HarmonicExactAspects.UI;

public partial class HarmonicExactAspectsScreen : UserControl
{
    private readonly HarmonicExactAspectsViewModel _viewModel;
    private readonly IChartSession? _chartSession;
    private readonly IRosetta _rosetta;

    public HarmonicExactAspectsScreen()
    {
        var app = (App)Application.Current;
        var rosetta = app.Services.GetRequiredService<IRosetta>();
        var configContext = app.Services.GetRequiredService<IConfigContext>();
        var chartSession = app.Services.GetRequiredService<IChartSession>();

        _viewModel = new HarmonicExactAspectsViewModel(rosetta, configContext);
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

    // Commit the maximum harmonic on Enter, not only when the text box loses focus.
    private void OnMaxHarmonicKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || sender is not TextBox textBox) return;
        textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
    }

    private void OnFactsheetClicked(object sender, RoutedEventArgs e)
    {
        new HarmonicExactAspectsFactsheetWindow(_rosetta) { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        new HarmonicExactAspectsHelpWindow(_rosetta) { Owner = Window.GetWindow(this) }.ShowDialog();
    }
}
