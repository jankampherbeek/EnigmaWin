// WheelRenderer.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using System.Windows.Media;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Features.ChartDrawing.UI;

/// <summary>
/// Draws a chart wheel in any drawing type, either full size or as the inner wheel of a chart with an
/// outer ring (dual wheels, Age Point, Log Time Scale, Zodiac Divisions).
/// Each drawing type has its own outermost elements, so the inner wheel is scaled per type and the
/// outer ring starts at a type-specific radius. Mirrors InnerWheelLayout in the Apple version.
/// </summary>
public static class WheelRenderer
{
    /// <summary>Draws the chart. <paramref name="data"/> must already be projected for the drawing type,
    /// see <see cref="WheelProjection.ProjectChart"/>.</summary>
    public static void Render(DrawingTypes type, DrawingContext ctx, Point center, double radius,
                              WheelPlotData data, WheelTheme theme, bool showAspects)
    {
        switch (type)
        {
            case DrawingTypes.HouseBased: HouseWheelCanvas.Render(ctx, center, radius, data, theme, showAspects);   break;
            case DrawingTypes.French:     FrenchWheelCanvas.Render(ctx, center, radius, data, theme, showAspects);  break;
            case DrawingTypes.Ring:       RingWheelCanvas.Render(ctx, center, radius, data, theme, showAspects);    break;
            case DrawingTypes.Dial360:    Dial360WheelCanvas.Render(ctx, center, radius, data, theme, showAspects); break;
            case DrawingTypes.Dial90:     Dial90WheelCanvas.Render(ctx, center, radius, data, theme);              break;
            case DrawingTypes.Dial45:     Dial45WheelCanvas.Render(ctx, center, radius, data, theme);              break;
            default:                      ZodiacWheelCanvas.Render(ctx, center, radius, data, theme, showAspects); break;
        }
    }

    /// <summary>Factor applied to a screen's sign-based inner wheel scale, so the outermost elements of
    /// the given type end at about the same radius as those of the sign-based wheel.</summary>
    public static double ScaleFactor(DrawingTypes type) => type switch
    {
        DrawingTypes.French => 0.66 / 0.78,   // position texts reach past the outer edge
        DrawingTypes.Ring   => 0.70 / 0.78,   // position texts (with sign glyph) outside the ring
        DrawingTypes.Dial360 or DrawingTypes.Dial90 or DrawingTypes.Dial45 => 0.72 / 0.78,   // label ring up to 0.99
        _                   => 1.0            // sign ring 0.89, cardinal labels 0.93
    };

    /// <summary>Fraction of the inner wheel radius where its content ends. Connect lines, arrows and
    /// tick marks of the outer ring end here.</summary>
    public static double ContentFraction(DrawingTypes type) => type switch
    {
        DrawingTypes.French => 1.08,   // just outside the position texts
        DrawingTypes.Ring   => 1.02,   // just outside the position texts
        DrawingTypes.Dial360 or DrawingTypes.Dial90 or DrawingTypes.Dial45 => 0.99,
        _                   => WheelMetrics.OuterSign
    };

    /// <summary>Fraction of the inner wheel radius where the outer ring starts.</summary>
    public static double RingStartFraction(DrawingTypes type) =>
        type is DrawingTypes.SignBased or DrawingTypes.HouseBased ? 1.0 : ContentFraction(type);

    /// <summary>Fills the background of the outer ring up to <paramref name="radius"/>.
    /// The ring type keeps a white background.</summary>
    public static void DrawRingBackground(DrawingContext ctx, DrawingTypes type, Point center,
                                          double radius, WheelTheme theme)
    {
        var color = type == DrawingTypes.Ring ? Colors.White : theme.OuterCircleBackground;
        ctx.DrawEllipse(new SolidColorBrush(color), null, center, radius, radius);
    }

    /// <summary>French and ring wheels place their planets outside their main circles, so a boundary
    /// circle keeps the inner planets visually apart from the outer ring.</summary>
    public static void DrawBoundaryIfNeeded(DrawingContext ctx, DrawingTypes type, Point center,
                                            double radius, double fullRadius, WheelTheme theme)
    {
        if (type is not (DrawingTypes.French or DrawingTypes.Ring)) return;
        var pen = new Pen(new SolidColorBrush(theme.CircleStroke),
                          WheelMetrics.StrokeWidth(WheelMetrics.StrokeFraction, fullRadius));
        ctx.DrawEllipse(null, pen, center, radius, radius);
    }

    /// <summary>Drawing type for the specialised wheels (Log Time Scale, Age Point, Alternative Zodiac
    /// Start, VSP): sign-based, French, ring and the 360° dial. House-based falls back to sign-based,
    /// the 90° and 45° dials to the 360° dial.</summary>
    public static DrawingTypes Specialised(DrawingTypes configured) => configured switch
    {
        DrawingTypes.HouseBased                     => DrawingTypes.SignBased,
        DrawingTypes.Dial90 or DrawingTypes.Dial45  => DrawingTypes.Dial360,
        _                                           => configured
    };

    /// <summary>Drawing type for Zodiac Divisions and Harmonic Orbs: as <see cref="Specialised"/>,
    /// but the house-based wheel is supported as well.</summary>
    public static DrawingTypes SpecialisedWithHouses(DrawingTypes configured) =>
        configured == DrawingTypes.HouseBased ? DrawingTypes.HouseBased : Specialised(configured);
}
