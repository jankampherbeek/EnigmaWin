// HarmonicsFactsheetWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.IO;
using System.Windows;
using EnigmaWin.Sources.Features.Shared;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Harmonics.UI;

public partial class HarmonicsFactsheetWindow : Window
{
    public HarmonicsFactsheetWindow(IRosetta rosetta)
    {
        Title = rosetta.GetText(RbFile.RadixHarmonics, "harmonics.factsheet.title");
        InitializeComponent();

        CloseButton.Content = rosetta.GetText(RbFile.RadixHarmonics, "harmonics.help.close");

        var langCode = rosetta.GetLanguage();
        var fileCode = langCode == "de" ? "ge" : langCode;
        var pdfPath = Path.Combine(
            AppContext.BaseDirectory,
            "Resources", "FactSheets", $"harmonics-{fileCode}.pdf");

        if (!File.Exists(pdfPath))
            pdfPath = Path.Combine(
                AppContext.BaseDirectory,
                "Resources", "FactSheets", "harmonics-en.pdf");

        Loaded += (_, _) => InitializeWebViewAsync(pdfPath);
    }

    private async void InitializeWebViewAsync(string pdfPath)
    {
        await WebView2Setup.EnsureInitializedAsync(WebViewControl);
        WebViewControl.Source = new Uri($"file:///{pdfPath.Replace('\\', '/')}");
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();
}
