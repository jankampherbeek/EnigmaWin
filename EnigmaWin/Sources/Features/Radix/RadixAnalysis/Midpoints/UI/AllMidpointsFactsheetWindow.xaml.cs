// AllMidpointsFactsheetWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.IO;
using System.Windows;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Midpoints.UI;

public partial class AllMidpointsFactsheetWindow : Window
{
    public AllMidpointsFactsheetWindow(IRosetta rosetta)
    {
        Title = rosetta.GetText(RbFile.RadixMidpoints, "midpoints.factsheet.title");
        InitializeComponent();

        CloseButton.Content = rosetta.GetText(RbFile.RadixMidpoints, "midpoints.help.close");

        var langCode = rosetta.GetLanguage();
        var fileCode = langCode == "de" ? "ge" : langCode;
        var pdfPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Resources", "FactSheets", $"midpoints-{fileCode}.pdf");

        if (!File.Exists(pdfPath))
            pdfPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources", "FactSheets", "midpoints-en.pdf");

        WebViewControl.Source = new Uri($"file:///{pdfPath.Replace('\\', '/')}");
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();
}
