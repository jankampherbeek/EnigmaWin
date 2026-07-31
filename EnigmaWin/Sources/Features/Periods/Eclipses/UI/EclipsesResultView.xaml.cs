// EclipsesResultView.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace EnigmaWin.Sources.Features.Periods.Eclipses.UI;

public partial class EclipsesResultView : UserControl
{
    private EclipsesResultViewModel? _vm;

    public EclipsesResultView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => _vm = DataContext as EclipsesResultViewModel;
    }

    private void OnExportPdfClick(object sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        var dlg = new SaveFileDialog
        {
            Title      = _vm.LabelExportPdf,
            FileName   = "eclipses.pdf",
            Filter     = "PDF Document (*.pdf)|*.pdf",
            DefaultExt = ".pdf"
        };
        if (dlg.ShowDialog(Window.GetWindow(this)) != true) return;

        var labels = new EclipsesPdfLabels(
            Title:      _vm.LabelResultsTitle,
            ColDate:    _vm.LabelColDate,
            ColPosition:_vm.LabelColPosition,
            ColType:    _vm.LabelColType,
            ColVisible: _vm.LabelColVisible,
            ColSaros:   _vm.LabelColSaros);

        EclipsesPdfExporter.Export(dlg.FileName, labels, _vm.HasLocation, _vm.Rows);
    }

    private void OnHelpClicked(object sender, RoutedEventArgs e)
    {
        var rosetta = ((App)Application.Current).Services.GetRequiredService<IRosetta>();
        new EclipsesHelpWindow(rosetta, "view.eclipses.help.results") { Owner = Window.GetWindow(this) }.ShowDialog();
    }
}
