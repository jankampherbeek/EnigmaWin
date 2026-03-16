using EnigmaWin.Sources.Domain;
using System.Collections.Generic;

namespace EnigmaWin.Sources.Features.Shared.I18n;

/// <summary>Localization keys for RoddenRating enum values.</summary>
public static class RoddenRatingKeys
{
    private static readonly Dictionary<RoddenRating, string> Keys = new()
    {
        [RoddenRating.None] = "enum.roddenrating.none",
        [RoddenRating.AA]   = "enum.roddenrating.aa",
        [RoddenRating.A]    = "enum.roddenrating.a",
        [RoddenRating.B]    = "enum.roddenrating.b",
        [RoddenRating.C]    = "enum.roddenrating.c",
        [RoddenRating.DD]   = "enum.roddenrating.dd",
        [RoddenRating.X]    = "enum.roddenrating.x",
        [RoddenRating.XX]   = "enum.roddenrating.xx",
    };

    public static string Key(RoddenRating rating) => Keys.GetValueOrDefault(rating, "");
}
