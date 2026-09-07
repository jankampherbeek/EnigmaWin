// AgePointFactsheetWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.IO;
using System.Windows;
using EnigmaWin.Sources.Features.Shared;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Progressive.AgePoint.UI;

public partial class AgePointFactsheetWindow : Window
{
    public AgePointFactsheetWindow(IRosetta rosetta)
    {
        Title = rosetta.GetText(RbFile.AgePoint, "view.agepoint.factsheet.title");
        InitializeComponent();

        CloseButton.Content = rosetta.GetText(RbFile.AgePoint, "view.agepoint.help.close");

        var langCode = rosetta.GetLanguage();
        var fileCode = langCode == "de" ? "ge" : langCode;
        var pdfPath = Path.Combine(
            AppContext.BaseDirectory,
            "Resources", "FactSheets", $"agepoint_{fileCode}.pdf");

        if (!File.Exists(pdfPath))
            pdfPath = Path.Combine(
                AppContext.BaseDirectory,
                "Resources", "FactSheets", "agepoint_en.pdf");

        Loaded += (_, _) => InitializeWebViewAsync(pdfPath);
    }

    private async void InitializeWebViewAsync(string pdfPath)
    {
        await WebView2Setup.EnsureInitializedAsync(WebViewControl);
        WebViewControl.Source = new Uri($"file:///{pdfPath.Replace('\\', '/')}");
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();
}
