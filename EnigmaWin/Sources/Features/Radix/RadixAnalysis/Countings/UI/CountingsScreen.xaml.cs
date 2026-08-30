// CountingsScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Features.Shared.Charts;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;
using ScottPlot.WPF;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Countings.UI;

public partial class CountingsScreen : UserControl
{
    private readonly CountingsViewModel _viewModel;
    private readonly IChartSession? _chartSession;
    private readonly IRosetta _rosetta;
    private readonly WpfPlot _elementsPlot = new();
    private readonly WpfPlot _crossesPlot = new();

    public CountingsScreen()
    {
        var app = (App)Application.Current;
        var rosetta = app.Services.GetRequiredService<IRosetta>();
        var configContext = app.Services.GetRequiredService<IConfigContext>();
        var chartSession = app.Services.GetRequiredService<IChartSession>();

        _viewModel = new CountingsViewModel(rosetta, configContext);
        _chartSession = chartSession;
        _rosetta = rosetta;

        InitializeComponent();
        DataContext = _viewModel;
        ElementsPiePanel.Children.Add(_elementsPlot);
        CrossesPiePanel.Children.Add(_crossesPlot);

        _viewModel.PropertyChanged += OnViewModelPropertyChanged;

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

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CountingsViewModel.HasData)) RenderPies();
    }

    private void RenderPies()
    {
        if (!_viewModel.HasData)
        {
            PieChartHelper.RenderWithColors(_elementsPlot, ElementsLegendPanel, []);
            PieChartHelper.RenderWithColors(_crossesPlot, CrossesLegendPanel, []);
            return;
        }

        PieChartHelper.RenderWithColors(_elementsPlot, ElementsLegendPanel,
            _viewModel.ElementsCounts.Select(r => (r.Name, (double)r.Count, CountingsColors.ForGroup(r.Group))).ToList());
        PieChartHelper.RenderWithColors(_crossesPlot, CrossesLegendPanel,
            _viewModel.CrossesCounts.Select(r => (r.Name, (double)r.Count, CountingsColors.ForGroup(r.Group))).ToList());
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        new CountingsHelpWindow(_rosetta) { Owner = Window.GetWindow(this) }.ShowDialog();
    }
}
