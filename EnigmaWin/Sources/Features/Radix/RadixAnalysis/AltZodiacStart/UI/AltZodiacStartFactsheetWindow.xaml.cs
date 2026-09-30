// AltZodiacStartFactsheetWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.IO;
using System.Windows;
using EnigmaWin.Sources.Features.Shared;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.AltZodiacStart.UI;

public partial class AltZodiacStartFactsheetWindow : Window
{
    public AltZodiacStartFactsheetWindow(IRosetta rosetta)
    {
        Title = rosetta.GetText(RbFile.RadixAltZodiacStart, "altzodiacstart.factsheet.title");
        InitializeComponent();

        CloseButton.Content = rosetta.GetText(RbFile.RadixAltZodiacStart, "altzodiacstart.help.close");

        var langCode = rosetta.GetLanguage();
        var fileCode = langCode == "de" ? "ge" : langCode;
        var pdfPath = Path.Combine(
            AppContext.BaseDirectory,
            "Resources", "FactSheets", $"altzodiacstart_{fileCode}.pdf");

        if (!File.Exists(pdfPath))
            pdfPath = Path.Combine(
                AppContext.BaseDirectory,
                "Resources", "FactSheets", "altzodiacstart_en.pdf");

        Loaded += (_, _) => InitializeWebViewAsync(pdfPath, rosetta);
    }

    private async void InitializeWebViewAsync(string pdfPath, IRosetta rosetta)
    {
        if (await WebView2Setup.EnsureInitializedAsync(WebViewControl, rosetta))
            WebViewControl.Source = new Uri($"file:///{pdfPath.Replace('\\', '/')}");
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();
}
