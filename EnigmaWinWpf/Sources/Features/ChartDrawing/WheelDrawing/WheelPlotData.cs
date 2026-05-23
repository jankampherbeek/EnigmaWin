// WheelPlotData.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows.Media;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

/// <summary>A single aspect line ready to be drawn inside the aspect circle.</summary>
public record WheelAspectItem(
    double Angle1,
    double Angle2,
    Color Color,
    double Exactness,
    Aspects Aspect);

/// <summary>A single planet/factor ready to be drawn on the wheel.</summary>
public record WheelPlotItem(
    Factors Factor,
    string Glyph,
    double EclipticLongitude,
    double MundaneAngle,
    double PlotAngle,
    string PositionText,
    SpeedType SpeedType)
{
    public WheelPlotItem WithPlotAngle(double plotAngle) =>
        this with { PlotAngle = plotAngle };
}

/// <summary>All data needed to draw a complete chart wheel.</summary>
public record WheelPlotData(
    double AscendantLongitude,
    double McLongitude,
    double[] CuspLongitudes,
    WheelPlotItem[] PlanetItems,
    bool HasTime,
    WheelAspectItem[] AspectItems)
{
    public static readonly WheelPlotData Empty = new(
        AscendantLongitude: 0,
        McLongitude: 0,
        CuspLongitudes: [],
        PlanetItems: [],
        HasTime: false,
        AspectItems: []);
}
