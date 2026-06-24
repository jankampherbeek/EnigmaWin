// PrimDirHelpWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.Windows;

namespace EnigmaWin.Sources.Features.Progressive.PrimDir.UI;

public partial class PrimDirHelpWindow : Window
{
    public IReadOnlyList<string> HelpParagraphs { get; }
    public string LabelClose { get; }

    public PrimDirHelpWindow(string helpText, string title, string closeLabel)
    {
        HelpParagraphs = helpText.Split(["\n\n"], System.StringSplitOptions.RemoveEmptyEntries);
        LabelClose     = closeLabel;
        Title          = title;
        InitializeComponent();
        DataContext = this;
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();
}
