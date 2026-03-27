// TransitsFactorRowViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Config.UI;

public sealed partial class TransitsFactorRowViewModel : ObservableObject
{
    private readonly Action _onChange;

    public Factors Factor     { get; }
    public string  Glyph      { get; }
    public string  FactorName { get; }

    [ObservableProperty]
    private bool _isSelected;

    partial void OnIsSelectedChanged(bool value) => _onChange();

    public TransitsFactorRowViewModel(
        Factors factor,
        string  name,
        bool    isSelected,
        Action  onChange)
    {
        Factor      = factor;
        FactorName  = name;
        Glyph       = GlyphCandidates.ForFactor(factor)[0];
        _isSelected = isSelected;
        _onChange   = onChange;
    }

    internal void ResetTo(bool isSelected) => IsSelected = isSelected;
}
