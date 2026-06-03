// LogTimeScaleFactsheetWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.IO;
using System.Windows;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Progressive.LogTimeScale.UI;

public partial class LogTimeScaleFactsheetWindow : Window
{
    public LogTimeScaleFactsheetWindow(IRosetta rosetta)
    {
        Title = rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.factsheet.title");
        InitializeComponent();

        CloseButton.Content = rosetta.GetText(RbFile.LogTimeScale, "view.logtimescale.help.close");

        var langCode = rosetta.GetLanguage();
        var fileCode = langCode == "de" ? "ge" : langCode;
        var pdfPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Resources", "FactSheets", $"logtimescale_{fileCode}.pdf");

        if (!File.Exists(pdfPath))
            pdfPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources", "FactSheets", "logtimescale_en.pdf");

        WebViewControl.Source = new Uri($"file:///{pdfPath.Replace('\\', '/')}");
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();
}
