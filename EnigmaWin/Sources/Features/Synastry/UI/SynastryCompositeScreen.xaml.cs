// SynastryCompositeScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
using EnigmaWin.Sources.Features.ChartDrawing;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace EnigmaWin.Sources.Features.Synastry.UI;

public partial class SynastryCompositeScreen : UserControl
{
    public SynastryCompositeScreen() => InitializeComponent();

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new SynastryHelpWindow(rosetta, "view.synastry.help.composite") { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    private void OnFactsheetClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new SynastryCompositeFactsheetWindow(rosetta) { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    private void OnExportClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not SynastryCompositeViewModel vm) return;
        var chartVm = vm.ChartViewModel;

        var dlg = new SaveFileDialog
        {
            Title      = "Export chart",
            Filter     = "PNG Image (*.png)|*.png|PDF Document (*.pdf)|*.pdf",
            DefaultExt = ".png"
        };
        if (dlg.ShowDialog(Window.GetWindow(this)) != true) return;

        var filePath = dlg.FileName;
        var isPdf    = filePath.EndsWith(".pdf", System.StringComparison.OrdinalIgnoreCase);

        if (isPdf)
            WheelExportService.ExportToPdfAsync(chartVm.PlotData, chartVm.Theme, chartVm.ShowAspects, WheelCanvasType.Zodiac, filePath);
        else
            WheelExportService.ExportToPngAsync(chartVm.PlotData, chartVm.Theme, chartVm.ShowAspects, WheelCanvasType.Zodiac, filePath);
    }
}
