// LongTimeEphemerisResultView.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace EnigmaWin.Sources.Features.Periods.LongTimeEphemeris.UI;

public partial class LongTimeEphemerisResultView : UserControl
{
    private LongTimeEphemerisResultViewModel? _vm;

    public LongTimeEphemerisResultView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_vm is not null)
            _vm.PropertyChanged -= OnVmPropertyChanged;

        _vm = DataContext as LongTimeEphemerisResultViewModel;

        if (_vm is not null)
            _vm.PropertyChanged += OnVmPropertyChanged;

        RebuildGrid();
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(LongTimeEphemerisResultViewModel.Columns) or nameof(LongTimeEphemerisResultViewModel.TableRows))
            RebuildGrid();
    }

    private void RebuildGrid()
    {
        ResultsGrid.Columns.Clear();
        ResultsGrid.ItemsSource = null;
        if (_vm is null || !_vm.HasResults) return;

        ResultsGrid.Columns.Add(new DataGridTextColumn
        {
            Header  = _vm.LabelDateHeader,
            Binding = new System.Windows.Data.Binding(nameof(LongTimeEphemerisDisplayRow.DateTimeText))
        });

        foreach (var factor in _vm.Columns)
        {
            ResultsGrid.Columns.Add(new DataGridTextColumn
            {
                Header  = _vm.ColumnHeader(factor),
                Binding = new System.Windows.Data.Binding($"Cells[{factor}]"),
                ElementStyle = RightAlignedCellStyle()
            });
        }

        ResultsGrid.ItemsSource = _vm.TableRows;
    }

    private static Style RightAlignedCellStyle()
    {
        var style = new Style(typeof(TextBlock));
        style.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right));
        return style;
    }

    // ── CSV export ────────────────────────────────────────────────────────────

    private void OnExportCsvClick(object sender, RoutedEventArgs e)
    {
        if (_vm is null || !_vm.HasResults) return;
        var dlg = new SaveFileDialog
        {
            Title      = _vm.LabelExportCsv,
            FileName   = "longtimeephemeris.csv",
            Filter     = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
            DefaultExt = ".csv"
        };
        if (dlg.ShowDialog(Window.GetWindow(this)) != true) return;

        var sb = new StringBuilder();
        var headerFields = new[] { _vm.LabelDateHeader }.Concat(_vm.Columns.Select(_vm.ColumnHeader));
        sb.AppendLine(CsvLine(headerFields));

        foreach (var row in _vm.TableRows)
        {
            var fields = new[] { row.DateTimeText }.Concat(_vm.Columns.Select(f => row.Cells.GetValueOrDefault(f, string.Empty)));
            sb.AppendLine(CsvLine(fields));
        }

        File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
    }

    private static string CsvLine(IEnumerable<string> fields) =>
        string.Join(",", fields.Select(f => "\"" + f.Replace("\"", "\"\"") + "\""));
}
