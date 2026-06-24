// PrimDirFactsheetWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.IO;
using System.Windows;

namespace EnigmaWin.Sources.Features.Progressive.PrimDir.UI;

public partial class PrimDirFactsheetWindow : Window
{
    public PrimDirFactsheetWindow(string title)
    {
        Title = title;
        InitializeComponent();
        CloseButton.Content = "Close";
        LoadPdf();
    }

    private void LoadPdf()
    {
        var langCode = System.Threading.Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
        var fileCode = langCode == "de" ? "ge" : langCode;
        var pdfPath  = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Resources", "FactSheets", $"primdir_{fileCode}.pdf");

        if (!File.Exists(pdfPath))
            pdfPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources", "FactSheets", "primdir_en.pdf");

        if (File.Exists(pdfPath))
            WebViewControl.Source = new Uri($"file:///{pdfPath.Replace('\\', '/')}");
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();
}
