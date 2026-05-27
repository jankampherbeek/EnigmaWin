// TransitScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
using EnigmaWin.Sources.Features.ChartDrawing;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace EnigmaWin.Sources.Features.Progressive.TransitSecDir.UI;

public partial class TransitScreen : UserControl
{
    public TransitScreen()
    {
        InitializeComponent();
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new TransitHelpWindow(rosetta, "view.transitresults.help") { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    private void OnExportClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not TransitViewModel vm) return;

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
            WheelExportService.ExportDualWheelToPdfAsync(vm.RadixPlotData, vm.TransitPlotItems, vm.Theme, vm.ShowAspects, filePath);
        else
            WheelExportService.ExportDualWheelToPngAsync(vm.RadixPlotData, vm.TransitPlotItems, vm.Theme, vm.ShowAspects, filePath);
    }
}
