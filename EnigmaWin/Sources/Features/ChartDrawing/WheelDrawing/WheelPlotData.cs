// WheelPlotData.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using Avalonia.Media;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>A single aspect line ready to be drawn inside the aspect circle.</summary>
public record WheelAspectItem(
    double Angle1,       // mundane angle of the first factor
    double Angle2,       // mundane angle of the second factor
    Color Color,         // from AspectSettings
    double Exactness,    // 1.0 = exact, 0.0 = at orb edge — drives line width
    Aspects Aspect);     // aspect type

/// <summary>A single planet/factor ready to be drawn on the wheel.</summary>
public record WheelPlotItem(
    Factors Factor,
    string Glyph,                  // from GlyphSelector
    double EclipticLongitude,      // exact ecliptic position (for connect line + position text)
    double MundaneAngle,           // angle on wheel (longitude - ascLong + 90), used for connect line endpoint
    double PlotAngle,              // visual angle after anti-overlap adjustment
    string PositionText,           // e.g. "15°23'"
    SpeedType SpeedType)           // movement speed classification
{
    /// <summary>Returns a copy with an updated PlotAngle.</summary>
    public WheelPlotItem WithPlotAngle(double plotAngle) =>
        this with { PlotAngle = plotAngle };
}

/// <summary>All data needed to draw a complete chart wheel.</summary>
public record WheelPlotData(
    double AscendantLongitude,     // for wheel rotation
    double McLongitude,
    double[] CuspLongitudes,       // 12 cusps
    WheelPlotItem[] PlanetItems,
    bool HasTime,                  // false = no houses, no ASC/MC shown
    WheelAspectItem[] AspectItems)
{
    /// <summary>An empty plot (nothing loaded yet).</summary>
    public static readonly WheelPlotData Empty = new(
        AscendantLongitude: 0,
        McLongitude: 0,
        CuspLongitudes: [],
        PlanetItems: [],
        HasTime: false,
        AspectItems: []);
}
