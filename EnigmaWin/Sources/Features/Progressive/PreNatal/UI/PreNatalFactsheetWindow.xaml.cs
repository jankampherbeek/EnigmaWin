// PreNatalFactsheetWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.IO;
using System.Windows;
using EnigmaWin.Sources.Features.Shared;

namespace EnigmaWin.Sources.Features.Progressive.PreNatal.UI;

public partial class PreNatalFactsheetWindow : Window
{
    public PreNatalFactsheetWindow(string title)
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
            AppContext.BaseDirectory,
            "Resources", "FactSheets", $"prenatal_{fileCode}.pdf");

        if (!File.Exists(pdfPath))
            pdfPath = Path.Combine(
                AppContext.BaseDirectory,
                "Resources", "FactSheets", "prenatal_en.pdf");

        if (File.Exists(pdfPath))
            Loaded += (_, _) => InitializeWebViewAsync(pdfPath);
    }

    private async void InitializeWebViewAsync(string pdfPath)
    {
        await WebView2Setup.EnsureInitializedAsync(WebViewControl);
        WebViewControl.Source = new Uri($"file:///{pdfPath.Replace('\\', '/')}");
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();
}
