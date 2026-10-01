// SolarScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
using EnigmaWin.Sources.Features.ChartDrawing;
using Microsoft.Win32;

namespace EnigmaWin.Sources.Features.Progressive.Solar.UI;

public partial class SolarScreen : UserControl
{
    public SolarScreen()
    {
        InitializeComponent();
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not SolarViewModel vm) return;
        new SolarHelpWindow(vm.LabelHelp, vm.LabelHelpTitle, vm.LabelHelpClose)
        {
            Owner = Window.GetWindow(this)
        }.ShowDialog();
    }

    private void OnExportClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not SolarViewModel vm) return;

        var dlg = new SaveFileDialog
        {
            Title      = "Export chart",
            Filter     = "PNG Image (*.png)|*.png|PDF Document (*.pdf)|*.pdf",
            DefaultExt = ".png"
        };
        if (dlg.ShowDialog(Window.GetWindow(this)) != true) return;

        var filePath = dlg.FileName;
        var isPdf    = filePath.EndsWith(".pdf", System.StringComparison.OrdinalIgnoreCase);

        // Tab 0 = single solar wheel (zodiac style), tab 1 = dual (combined) wheel
        if (vm.ActiveTab == 0)
        {
            if (isPdf)
                WheelExportService.ExportChartToPdfAsync(vm.SolarPlotData, vm.DrawingType, vm.Theme, vm.ShowAspects, filePath);
            else
                WheelExportService.ExportChartToPngAsync(vm.SolarPlotData, vm.DrawingType, vm.Theme, vm.ShowAspects, filePath);
        }
        else
        {
            if (isPdf)
                WheelExportService.ExportDualWheelToPdfAsync(vm.RadixPlotData, vm.SolarPlotItems, vm.Theme, vm.ShowAspects, filePath, drawingType: vm.DrawingType);
            else
                WheelExportService.ExportDualWheelToPngAsync(vm.RadixPlotData, vm.SolarPlotItems, vm.Theme, vm.ShowAspects, filePath, drawingType: vm.DrawingType);
        }
    }
}
