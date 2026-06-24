// PreNatalHelpWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Windows;

namespace EnigmaWin.Sources.Features.Progressive.PreNatal.UI;

public partial class PreNatalHelpWindow : Window
{
    public IReadOnlyList<string> HelpParagraphs { get; }
    public string LabelClose { get; }

    public PreNatalHelpWindow(string helpText, string title, string closeLabel)
    {
        HelpParagraphs = helpText.Split(["\n\n"], StringSplitOptions.RemoveEmptyEntries);
        LabelClose     = closeLabel;
        Title          = title;
        InitializeComponent();
        DataContext = this;
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();
}
