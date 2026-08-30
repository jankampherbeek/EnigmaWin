// HarmonicOrbsScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
using EnigmaWin.Sources.Features.ChartDrawing;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.HarmonicOrbs.UI;

public partial class HarmonicOrbsScreen : UserControl
{
    private HarmonicOrbsViewModel? _vm;

    public HarmonicOrbsScreen()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_vm is not null) _vm.GridDataReady -= OnGridDataReady;
        _vm = DataContext as HarmonicOrbsViewModel;
        if (_vm is not null) _vm.GridDataReady += OnGridDataReady;
    }

    private void OnGridDataReady(System.Collections.Generic.List<Domain.Factors> factors,
        System.Collections.Generic.List<Aspects.UI.AspectGridControl.AspectCell> cells)
    {
        Application.Current.Dispatcher.BeginInvoke(() => AspectGrid.SetData(factors, cells));
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new HarmonicOrbsHelpWindow(rosetta) { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    private void OnExportClicked(object sender, RoutedEventArgs e)
    {
        if (_vm is null) return;

        var dlg = new SaveFileDialog
        {
            Title = "Export chart",
            Filter = "PNG Image (*.png)|*.png|PDF Document (*.pdf)|*.pdf",
            DefaultExt = ".png"
        };

        if (dlg.ShowDialog(Window.GetWindow(this)) != true) return;

        var filePath = dlg.FileName;
        var isPdf = filePath.EndsWith(".pdf", System.StringComparison.OrdinalIgnoreCase);

        if (isPdf)
            WheelExportService.ExportToPdfAsync(_vm.PlotData, _vm.Theme, true, WheelCanvasType.Zodiac, filePath);
        else
            WheelExportService.ExportToPngAsync(_vm.PlotData, _vm.Theme, true, WheelCanvasType.Zodiac, filePath);
    }
}
