// WheelProjection.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Shared.Glyphs;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>
/// Converts zodiac-based wheel data (as produced by <see cref="WheelPlotDataBuilder"/>) to the
/// angles used by other drawing types, so any chart presentation can follow the configured drawing type.
/// </summary>
public static class WheelProjection
{
    /// <summary>The drawing type that is actually used: house-based wheels need house cusps,
    /// otherwise the sign-based wheel is used.</summary>
    public static DrawingTypes Effective(DrawingTypes type, WheelPlotData data) =>
        type == DrawingTypes.HouseBased && (!data.HasTime || data.CuspLongitudes.Length < 12)
            ? DrawingTypes.SignBased
            : type;

    /// <summary>Screen angle for an ecliptic longitude in the given drawing type.</summary>
    public static double LongitudeToAngle(DrawingTypes type, double longitude, double asc, double[] cusps) =>
        type switch
        {
            DrawingTypes.HouseBased => HouseWheelPlotDataBuilder.EclipticToHouseAngle(longitude, cusps),
            DrawingTypes.Dial360    => WheelGeometry.Normalise(longitude),
            DrawingTypes.Dial90     => WheelGeometry.Normalise(longitude) % 90.0 * 4.0,
            DrawingTypes.Dial45     => WheelGeometry.Normalise(longitude) % 45.0 * 8.0,
            _                       => WheelGeometry.MundaneAngle(longitude, asc)
        };

    /// <summary>Converts an angle of a zodiac-based wheel to the angle in the given drawing type.</summary>
    public static double ZodiacAngleToAngle(DrawingTypes type, double zodiacAngle, double asc, double[] cusps) =>
        LongitudeToAngle(type, ZodiacAngleToLongitude(zodiacAngle, asc), asc, cusps);

    /// <summary>Converts zodiac-based chart data to data for the given drawing type.</summary>
    public static WheelPlotData ProjectChart(WheelPlotData data, DrawingTypes type)
    {
        type = Effective(type, data);
        if (!NeedsProjection(type)) return data;

        var asc   = data.AscendantLongitude;
        var cusps = data.CuspLongitudes;
        var items = new List<WheelPlotItem>(ProjectItemsUnresolved(data.PlanetItems, type, asc, cusps));

        if (IsDial(type) && data.HasTime)
        {
            items.Add(AngleItem(Factors.Ascendant, data.AscendantLongitude, type, asc, cusps));
            items.Add(AngleItem(Factors.Mc,        data.McLongitude,        type, asc, cusps));
        }

        return data with
        {
            CuspLongitudes = IsDial(type) ? [] : cusps,
            PlanetItems    = GlyphOverlapResolver.Resolve(items),
            // The 360° dial can show aspect lines; the 90° and 45° dials do not.
            AspectItems    = HasNoAspects(type) ? [] : ProjectAspects(data.AspectItems, type, asc, cusps)
        };
    }

    /// <summary>Converts zodiac-based items of an outer ring to the angles of the given drawing type.</summary>
    public static WheelPlotItem[] ProjectItems(WheelPlotItem[] items, DrawingTypes type, double asc, double[] cusps) =>
        NeedsProjection(type)
            ? GlyphOverlapResolver.Resolve(ProjectItemsUnresolved(items, type, asc, cusps).ToList())
            : items;

    /// <summary>Converts zodiac-based aspect lines to the angles of the given drawing type.</summary>
    public static WheelAspectItem[] ProjectAspects(WheelAspectItem[] aspects, DrawingTypes type,
                                                   double asc, double[] cusps) =>
        NeedsProjection(type)
            ? [.. aspects.Select(a => a with
              {
                  Angle1 = ZodiacAngleToAngle(type, a.Angle1, asc, cusps),
                  Angle2 = ZodiacAngleToAngle(type, a.Angle2, asc, cusps)
              })]
            : aspects;

    public static bool IsDial(DrawingTypes type) =>
        type is DrawingTypes.Dial360 or DrawingTypes.Dial90 or DrawingTypes.Dial45;

    /// <summary>Drawing types that never show aspect lines.</summary>
    public static bool HasNoAspects(DrawingTypes type) =>
        type is DrawingTypes.Dial90 or DrawingTypes.Dial45;

    private static bool NeedsProjection(DrawingTypes type) =>
        type is DrawingTypes.HouseBased || IsDial(type);

    private static IEnumerable<WheelPlotItem> ProjectItemsUnresolved(
        IEnumerable<WheelPlotItem> items, DrawingTypes type, double asc, double[] cusps) =>
        items.Select(item =>
        {
            var angle = LongitudeToAngle(type, item.EclipticLongitude, asc, cusps);
            return item with
            {
                MundaneAngle = angle,
                PlotAngle    = angle,
                PositionText = PositionText(type, item.EclipticLongitude, item.SpeedType, item.PositionText)
            };
        });

    private static WheelPlotItem AngleItem(Factors factor, double longitude, DrawingTypes type,
                                           double asc, double[] cusps)
    {
        var angle = LongitudeToAngle(type, longitude, asc, cusps);
        return new WheelPlotItem(
            Factor:            factor,
            Glyph:             GlyphSelector.GetGlyphForFactor(factor),
            EclipticLongitude: longitude,
            MundaneAngle:      angle,
            PlotAngle:         angle,
            PositionText:      PositionText(type, longitude, SpeedType.Direct, string.Empty),
            SpeedType:         SpeedType.Direct);
    }

    /// <summary>Dials 90 and 45 show the position within the dial; other types keep the original text.</summary>
    private static string PositionText(DrawingTypes type, double longitude, SpeedType speedType, string original)
    {
        var range = type switch
        {
            DrawingTypes.Dial90 => 90.0,
            DrawingTypes.Dial45 => 45.0,
            DrawingTypes.Dial360 when string.IsNullOrEmpty(original) => 30.0,
            _ => 0.0
        };
        if (range == 0.0) return original;

        var totalMin = (int)(WheelGeometry.Normalise(longitude) % range * 60);
        var text     = $"{totalMin / 60}°{totalMin % 60:D2}'";
        return speedType == SpeedType.Direct ? text : $"{text} {speedType.Abbreviation()}";
    }

    private static double ZodiacAngleToLongitude(double zodiacAngle, double asc) =>
        WheelGeometry.Normalise(zodiacAngle - 90.0 + asc);
}
