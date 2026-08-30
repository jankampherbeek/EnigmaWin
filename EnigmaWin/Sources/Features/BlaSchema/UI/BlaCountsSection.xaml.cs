// BlaCountsSection.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ScottPlot.WPF;

namespace EnigmaWin.Sources.Features.BlaSchema.UI;

public partial class BlaCountsSection : UserControl
{
    private readonly WpfPlot _elementsPlot = new();
    private readonly WpfPlot _crossesPlot = new();
    private readonly WpfPlot _quadrantsPlot = new();
    private BlaSchemaViewModel? _vm;

    public BlaCountsSection()
    {
        InitializeComponent();
        ElementsPiePanel.Children.Add(_elementsPlot);
        CrossesPiePanel.Children.Add(_crossesPlot);
        QuadrantsPiePanel.Children.Add(_quadrantsPlot);
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_vm is not null) _vm.PropertyChanged -= OnVmPropertyChanged;
        _vm = DataContext as BlaSchemaViewModel;
        if (_vm is not null) _vm.PropertyChanged += OnVmPropertyChanged;
        RenderPies();
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(BlaSchemaViewModel.HasData)) RenderPies();
    }

    private void RenderPies()
    {
        if (_vm is null || !_vm.HasData)
        {
            BlaPieChartHelper.Render(_elementsPlot, ElementsLegendPanel, []);
            BlaPieChartHelper.Render(_crossesPlot, CrossesLegendPanel, []);
            BlaPieChartHelper.Render(_quadrantsPlot, QuadrantsLegendPanel, []);
            return;
        }

        BlaPieChartHelper.Render(_elementsPlot, ElementsLegendPanel, _vm.ElementsCounts.Select(r => (r.Name, (double)r.Total)).ToList());
        BlaPieChartHelper.Render(_crossesPlot, CrossesLegendPanel, _vm.CrossesCounts.Select(r => (r.Name, (double)r.Total)).ToList());
        BlaPieChartHelper.Render(_quadrantsPlot, QuadrantsLegendPanel, _vm.QuadrantCounts.Select(r => (r.Name, (double)r.Count)).ToList());
    }
}
