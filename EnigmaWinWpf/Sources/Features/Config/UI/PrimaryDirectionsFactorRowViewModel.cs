// PrimaryDirectionsFactorRowViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.Config.UI;

public sealed partial class PrimaryDirectionsFactorRowViewModel : ObservableObject
{
    private readonly Action _onChange;

    public Factors Factor     { get; }
    public string  Glyph      { get; }
    public string  FactorName { get; }

    private bool _origIsPromissor;
    private bool _origIsSignificator;

    [ObservableProperty]
    private bool _isPromissor;

    [ObservableProperty]
    private bool _isSignificator;

    partial void OnIsPromissorChanged(bool value)    => _onChange();
    partial void OnIsSignificatorChanged(bool value) => _onChange();

    public PrimaryDirectionsFactorRowViewModel(
        Factors factor,
        string  name,
        bool    isPromissor,
        bool    isSignificator,
        Action  onChange)
    {
        Factor           = factor;
        FactorName       = name;
        Glyph            = GlyphCandidates.ForFactor(factor)[0];
        _isPromissor     = isPromissor;
        _isSignificator  = isSignificator;
        _origIsPromissor    = isPromissor;
        _origIsSignificator = isSignificator;
        _onChange        = onChange;
    }

    internal void ResetTo(bool isPromissor, bool isSignificator)
    {
        IsPromissor    = isPromissor;
        IsSignificator = isSignificator;
        SaveOriginals();
    }

    internal void SaveOriginals()
    {
        _origIsPromissor    = IsPromissor;
        _origIsSignificator = IsSignificator;
    }
}
