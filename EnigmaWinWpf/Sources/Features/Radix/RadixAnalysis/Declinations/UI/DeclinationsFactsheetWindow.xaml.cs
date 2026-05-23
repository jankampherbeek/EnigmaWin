// DeclinationsFactsheetWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.IO;
using System.Windows;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations.UI;

public partial class DeclinationsFactsheetWindow : Window
{
    public DeclinationsFactsheetWindow(IRosetta rosetta)
    {
        Title = rosetta.GetText(RbFile.RadixDeclinations, "declinations.factsheet.tooltip");
        InitializeComponent();

        CloseButton.Content = rosetta.GetText(RbFile.RadixDeclinations, "declinations.help.close");

        var langCode = rosetta.GetLanguage();
        var fileCode = langCode == "de" ? "ge" : langCode;
        var htmlPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Resources", "FactSheets", $"declinations-{fileCode}.html");

        if (!File.Exists(htmlPath))
            htmlPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources", "FactSheets", "declinations-en.html");

        WebViewControl.Source = new Uri($"file:///{htmlPath.Replace('\\', '/')}");
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();
}
