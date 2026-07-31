// SynastryDeclinationComparisonScreen.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace EnigmaWin.Sources.Features.Synastry.UI;

public partial class SynastryDeclinationComparisonScreen : UserControl
{
    public SynastryDeclinationComparisonScreen() => InitializeComponent();

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new SynastryHelpWindow(rosetta, "view.synastry.help.declinationcomparison") { Owner = Window.GetWindow(this) }.ShowDialog();
    }

    private void OnExportPdfClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not SynastryDeclinationComparisonViewModel vm) return;

        var dlg = new SaveFileDialog
        {
            Title      = vm.LabelExportPdf,
            FileName   = "Synastry declinations.pdf",
            Filter     = "PDF Document (*.pdf)|*.pdf",
            DefaultExt = ".pdf"
        };
        if (dlg.ShowDialog(Window.GetWindow(this)) != true) return;

        var labels = new SynastryDeclinationComparisonPdfLabels(
            Title:      vm.LabelTitle,
            ChartAName: vm.ChartAName,
            ChartBName: vm.ChartBName,
            ColOrb:     vm.LabelColOrb);

        SynastryDeclinationComparisonPdfExporter.Export(dlg.FileName, labels, vm.RowsFromA, vm.RowsFromB);
    }
}
