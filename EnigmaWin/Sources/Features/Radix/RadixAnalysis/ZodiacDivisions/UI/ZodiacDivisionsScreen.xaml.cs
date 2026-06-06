// ZodiacDivisionsScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
using EnigmaWin.Sources.Features.ChartDrawing;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.ZodiacDivisions.UI;

public partial class ZodiacDivisionsScreen : UserControl
{
    public ZodiacDivisionsScreen()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            if (DataContext is ZodiacDivisionsViewModel vm)
                vm.Refresh();
        };
    }

    private void OnFactsheetClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new ZodiacDivisionsFactsheetWindow(rosetta) { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new ZodiacDivisionsHelpWindow(rosetta) { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    private void OnExportClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ZodiacDivisionsViewModel vm) return;

        var dlg = new SaveFileDialog
        {
            Title      = "Export",
            Filter     = "PNG Image (*.png)|*.png|PDF Document (*.pdf)|*.pdf",
            DefaultExt = ".png"
        };
        if (dlg.ShowDialog(Window.GetWindow(this)) != true) return;

        var path  = dlg.FileName;
        var isPdf = path.EndsWith(".pdf", System.StringComparison.OrdinalIgnoreCase);

        if (isPdf)
            WheelExportService.ExportZodiacDivisionsToPdfAsync(vm.PlotData, vm.Marks, vm.Theme, vm.ShowAspects, path);
        else
            WheelExportService.ExportZodiacDivisionsToPngAsync(vm.PlotData, vm.Marks, vm.Theme, vm.ShowAspects, path);
    }
}
