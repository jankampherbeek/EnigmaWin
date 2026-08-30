// BlaDispositorsSection.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using EnigmaWin.Sources.Features.Shared.Charts;
using ScottPlot.WPF;

namespace EnigmaWin.Sources.Features.BlaSchema.UI;

public partial class BlaDispositorsSection : UserControl
{
    private readonly WpfPlot _dispositorsPlot = new();
    private BlaSchemaViewModel? _vm;

    public BlaDispositorsSection()
    {
        InitializeComponent();
        DispositorsPiePanel.Children.Add(_dispositorsPlot);
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_vm is not null) _vm.PropertyChanged -= OnVmPropertyChanged;
        _vm = DataContext as BlaSchemaViewModel;
        if (_vm is not null) _vm.PropertyChanged += OnVmPropertyChanged;
        RenderPie();
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BlaSchemaViewModel.HasData)) RenderPie();
    }

    private void RenderPie()
    {
        if (_vm is null || !_vm.HasData)
        {
            PieChartHelper.Render(_dispositorsPlot, DispositorsLegendPanel, []);
            return;
        }

        PieChartHelper.Render(_dispositorsPlot, DispositorsLegendPanel, _vm.Dispositors.Select(r => (r.RulerPairName, (double)r.Total)).ToList());
    }
}
