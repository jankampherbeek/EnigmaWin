// ProgressiveCalendarFactorSelectionWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Progressive.ProgressiveCalendar.UI;

/// <summary>Reusable multi-select factor picker, shared by the four factor pickers on the
/// Progressive Calendar input screen (transit/secondary/symbolic/radix factors).</summary>
public partial class ProgressiveCalendarFactorSelectionWindow : Window
{
    private sealed class FactorItem
    {
        public string Label { get; }
        public Factors Factor { get; }
        public bool IsSelected { get; set; }
        public FactorItem(Factors f, bool selected)
        {
            Factor = f;
            Label = f.ToString();
            IsSelected = selected;
        }
    }

    private readonly List<FactorItem> _items;

    public Factors[] SelectedFactors { get; private set; } = [];

    public ProgressiveCalendarFactorSelectionWindow(
        string title, string promptText, string okLabel, string cancelLabel,
        IReadOnlyList<Factors> selectableFactors, IReadOnlyList<Factors> currentSelection)
    {
        InitializeComponent();
        Title = title;
        PromptText.Text = promptText;
        OkButton.Content = okLabel;
        CancelButton.Content = cancelLabel;

        var selected = new HashSet<Factors>(currentSelection);
        _items = selectableFactors.Select(f => new FactorItem(f, selected.Contains(f))).ToList();
        FactorList.ItemsSource = _items;
    }

    private void OnOkClicked(object sender, RoutedEventArgs e)
    {
        SelectedFactors = _items.Where(x => x.IsSelected).Select(x => x.Factor).ToArray();
        DialogResult = true;
    }

    private void OnCancelClicked(object sender, RoutedEventArgs e) => DialogResult = false;
}
