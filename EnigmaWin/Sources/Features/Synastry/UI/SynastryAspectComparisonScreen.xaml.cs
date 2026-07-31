// SynastryAspectComparisonScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace EnigmaWin.Sources.Features.Synastry.UI;

public partial class SynastryAspectComparisonScreen : UserControl
{
    public SynastryAspectComparisonScreen() => InitializeComponent();

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new SynastryHelpWindow(rosetta, "view.synastry.help.aspectcomparison") { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    private void OnExportPdfClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not SynastryAspectComparisonViewModel vm) return;

        var dlg = new SaveFileDialog
        {
            Title      = vm.LabelExportPdf,
            FileName   = "Synastry aspects.pdf",
            Filter     = "PDF Document (*.pdf)|*.pdf",
            DefaultExt = ".pdf"
        };
        if (dlg.ShowDialog(Window.GetWindow(this)) != true) return;

        var labels = new SynastryAspectComparisonPdfLabels(
            Title:      vm.LabelTitle,
            ChartAName: vm.ChartAName,
            ChartBName: vm.ChartBName,
            ColOrb:     vm.LabelColOrb);

        SynastryAspectComparisonPdfExporter.Export(dlg.FileName, labels, vm.RowsFromA, vm.RowsFromB);
    }
}
