// ProgressiveCalendarHelpWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.Windows;

namespace EnigmaWin.Sources.Features.Progressive.ProgressiveCalendar.UI;

public partial class ProgressiveCalendarHelpWindow : Window
{
    public IReadOnlyList<string> HelpParagraphs { get; }
    public string LabelClose { get; }

    public ProgressiveCalendarHelpWindow(string helpText, string title, string closeLabel)
    {
        HelpParagraphs = helpText.Split(["\n\n"], System.StringSplitOptions.RemoveEmptyEntries);
        LabelClose = closeLabel;
        Title = title;
        InitializeComponent();
        DataContext = this;
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();
}
