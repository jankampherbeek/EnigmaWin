// PreNatalAspectSelectionWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Progressive.PreNatal.UI;

public partial class PreNatalAspectSelectionWindow : Window
{
    private sealed class AspectItem
    {
        public string  Label      { get; }
        public Aspects Aspect     { get; }
        public bool    IsSelected { get; set; }
        public AspectItem(Aspects a, bool selected) { Aspect = a; Label = a.ToString(); IsSelected = selected; }
    }

    private readonly List<AspectItem> _items;

    public Aspects[] SelectedAspects { get; private set; } = [];

    public PreNatalAspectSelectionWindow(Aspects[] currentSelection)
    {
        InitializeComponent();
        var selected = new HashSet<Aspects>(currentSelection);
        _items = Enum.GetValues<Aspects>()
            .Select(a => new AspectItem(a, selected.Contains(a)))
            .ToList();
        AspectList.ItemsSource = _items;
    }

    private void OnOkClicked(object sender, RoutedEventArgs e)
    {
        SelectedAspects = _items.Where(x => x.IsSelected).Select(x => x.Aspect).ToArray();
        DialogResult = true;
    }

    private void OnCancelClicked(object sender, RoutedEventArgs e) => DialogResult = false;
}
