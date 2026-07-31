// SynastryMidpointComparisonScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace EnigmaWin.Sources.Features.Synastry.UI;

public partial class SynastryMidpointComparisonScreen : UserControl
{
    public SynastryMidpointComparisonScreen() => InitializeComponent();

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new SynastryHelpWindow(rosetta, "view.synastry.help.midpointcomparison") { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    private void OnExportPdfClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not SynastryMidpointComparisonViewModel vm) return;

        var dlg = new SaveFileDialog
        {
            Title      = vm.LabelExportPdf,
            FileName   = "Synastry midpoints.pdf",
            Filter     = "PDF Document (*.pdf)|*.pdf",
            DefaultExt = ".pdf"
        };
        if (dlg.ShowDialog(Window.GetWindow(this)) != true) return;

        var labels = new SynastryMidpointComparisonPdfLabels(
            Title:      vm.LabelTitle,
            ChartAName: vm.ChartAName,
            ChartBName: vm.ChartBName,
            ColOrb:     vm.LabelColOrb,
            ColExactness: vm.LabelColExactness);

        SynastryMidpointComparisonPdfExporter.Export(dlg.FileName, labels, vm.RowsA, vm.RowsB);
    }
}
