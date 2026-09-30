// ImportExportView.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace EnigmaWin.Sources.Features.ImportExport.UI;

public partial class ImportExportView : UserControl
{
    public ImportExportView()
    {
        InitializeComponent();
    }

    private async void OnExportClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ImportExportViewModel vm) return;
        var dlg = new SaveFileDialog
        {
            Title      = vm.LabelExport,
            FileName   = vm.DefaultFileName,
            Filter     = vm.FileFilter,
            DefaultExt = vm.DefaultExtension
        };
        if (dlg.ShowDialog(Window.GetWindow(this)) != true) return;
        await vm.ExportToFileAsync(dlg.FileName);
    }

    private async void OnImportClicked(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ImportExportViewModel vm) return;
        var dlg = new OpenFileDialog
        {
            Title           = vm.LabelImport,
            Filter          = vm.FileFilter,
            DefaultExt      = vm.DefaultExtension,
            CheckFileExists = true
        };
        if (dlg.ShowDialog(Window.GetWindow(this)) != true) return;
        await vm.ImportFromFileAsync(dlg.FileName);
    }
}
