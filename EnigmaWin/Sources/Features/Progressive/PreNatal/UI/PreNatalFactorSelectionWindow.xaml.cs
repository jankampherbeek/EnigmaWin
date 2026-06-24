// PreNatalFactorSelectionWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Progressive.PreNatal.UI;

public partial class PreNatalFactorSelectionWindow : Window
{
    private sealed class FactorItem
    {
        public string  Label      { get; }
        public Factors Factor     { get; }
        public bool    IsSelected { get; set; }
        public FactorItem(Factors f, bool selected) { Factor = f; Label = f.ToString(); IsSelected = selected; }
    }

    private readonly List<FactorItem> _items;

    public Factors[] SelectedFactors { get; private set; } = [];

    public PreNatalFactorSelectionWindow(Factors[] currentSelection)
    {
        InitializeComponent();
        var selected = new HashSet<Factors>(currentSelection);
        _items = PreNatalOrchestrator.SelectableFactors
            .Select(f => new FactorItem(f, selected.Contains(f)))
            .ToList();
        FactorList.ItemsSource = _items;
    }

    private void OnOkClicked(object sender, RoutedEventArgs e)
    {
        SelectedFactors = _items.Where(x => x.IsSelected).Select(x => x.Factor).ToArray();
        DialogResult = true;
    }

    private void OnCancelClicked(object sender, RoutedEventArgs e) => DialogResult = false;
}
