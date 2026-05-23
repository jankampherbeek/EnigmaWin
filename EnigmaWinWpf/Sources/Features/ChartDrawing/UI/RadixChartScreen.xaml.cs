// RadixChartScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using EnigmaWin.Sources.Features.Config;
using Microsoft.Win32;

namespace EnigmaWin.Sources.Features.ChartDrawing.UI;

public partial class RadixChartScreen : UserControl
{
    public RadixChartScreen() => InitializeComponent();

    private RadixChartViewModel Vm => (RadixChartViewModel)DataContext;

    private void OnDialTypeClicked(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton btn) return;
        var tag = btn.Tag as string;
        var newType = tag switch
        {
            "360" => DrawingTypes.Dial360,
            "90"  => DrawingTypes.Dial90,
            "45"  => DrawingTypes.Dial45,
            _     => DrawingTypes.Dial360
        };
        Vm.SetDrawingType(newType);
    }

    private void OnExportClicked(object sender, RoutedEventArgs e)
    {
        var vm = Vm;
        var dlg = new SaveFileDialog
        {
            Title  = "Export chart",
            Filter = "PNG Image (*.png)|*.png|PDF Document (*.pdf)|*.pdf",
            DefaultExt = ".png"
        };

        if (dlg.ShowDialog(Window.GetWindow(this)) != true) return;

        var filePath  = dlg.FileName;
        var isPdf     = filePath.EndsWith(".pdf", System.StringComparison.OrdinalIgnoreCase);
        var canvasType = vm.DrawingType switch
        {
            DrawingTypes.HouseBased => WheelCanvasType.House,
            DrawingTypes.French     => WheelCanvasType.French,
            DrawingTypes.Ring       => WheelCanvasType.Ring,
            DrawingTypes.Dial360    => WheelCanvasType.Dial360,
            DrawingTypes.Dial90     => WheelCanvasType.Dial90,
            DrawingTypes.Dial45     => WheelCanvasType.Dial45,
            _                       => WheelCanvasType.Zodiac
        };

        if (isPdf)
            WheelExportService.ExportToPdfAsync(vm.PlotData, vm.Theme, vm.ShowAspects, canvasType, filePath);
        else
            WheelExportService.ExportToPngAsync(vm.PlotData, vm.Theme, vm.ShowAspects, canvasType, filePath);
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var vm = Vm;
        var helpWindow = new ChartWheelHelpWindow(vm.Rosetta, vm.DrawingType)
        {
            Owner = Window.GetWindow(this)
        };
        helpWindow.ShowDialog();
    }
}
