// ProgressiveMethods.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;

namespace EnigmaWin.Sources.Domain;

/// <summary>Supported progressive techniques.</summary>
public enum ProgressiveMethods
{
    Transit    = 0,
    SecDir     = 1,
    SymDir     = 2,
    PrimDir    = 3,
    Solar      = 4,
    Profection = 5,
    Firdaria   = 6
}

/// <summary>Extension methods for the ProgressiveMethods enum.</summary>
public static class ProgressiveMethodsExtensions
{
    /// <summary>Localized name key for this progressive method.</summary>
    public static string LocalizedName(this ProgressiveMethods method)
    {
        return method switch
        {
            ProgressiveMethods.Transit    => "enum.progressivemethod.transit",
            ProgressiveMethods.SecDir     => "enum.progressivemethod.secdir",
            ProgressiveMethods.SymDir     => "enum.progressivemethod.symdir",
            ProgressiveMethods.PrimDir    => "enum.progressivemethod.primdir",
            ProgressiveMethods.Solar      => "enum.progressivemethod.solar",
            ProgressiveMethods.Profection => "enum.progressivemethod.profection",
            ProgressiveMethods.Firdaria   => "enum.progressivemethod.firdaria",
            _ => string.Empty
        };
    }

    /// <summary>Get a ProgressiveMethods enum value from an index.</summary>
    public static ProgressiveMethods? FromIndex(int index)
    {
        var allCases = Enum.GetValues<ProgressiveMethods>();
        if (index < 0 || index >= allCases.Length)
            return null;
        return allCases[index];
    }
}
