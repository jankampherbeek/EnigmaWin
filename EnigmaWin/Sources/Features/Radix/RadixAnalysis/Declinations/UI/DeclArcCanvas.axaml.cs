// DeclArcCanvas.axaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations.UI;

/// <summary>
/// Interactive canvas for the 240° declination arc diagram.
/// Supports hover and click to show midpoint overlay lines.
/// </summary>
public partial class DeclArcCanvas : UserControl
{
    // ── Styled properties ─────────────────────────────────────────────────────

    public static readonly StyledProperty<IReadOnlyList<DeclArcItem>> ItemsProperty =
        AvaloniaProperty.Register<DeclArcCanvas, IReadOnlyList<DeclArcItem>>(
            nameof(Items), []);

    public static readonly StyledProperty<WheelTheme> ThemeProperty =
        AvaloniaProperty.Register<DeclArcCanvas, WheelTheme>(
            nameof(Theme), WheelTheme.Color);

    public static readonly StyledProperty<string> NorthLabelProperty =
        AvaloniaProperty.Register<DeclArcCanvas, string>(nameof(NorthLabel), "N");

    public static readonly StyledProperty<string> SouthLabelProperty =
        AvaloniaProperty.Register<DeclArcCanvas, string>(nameof(SouthLabel), "S");

    public static readonly StyledProperty<double> OrbProperty =
        AvaloniaProperty.Register<DeclArcCanvas, double>(nameof(Orb), 0.75);

    public IReadOnlyList<DeclArcItem> Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public WheelTheme Theme
    {
        get => GetValue(ThemeProperty);
        set => SetValue(ThemeProperty, value);
    }

    public string NorthLabel
    {
        get => GetValue(NorthLabelProperty);
        set => SetValue(NorthLabelProperty, value);
    }

    public string SouthLabel
    {
        get => GetValue(SouthLabelProperty);
        set => SetValue(SouthLabelProperty, value);
    }

    public double Orb
    {
        get => GetValue(OrbProperty);
        set => SetValue(OrbProperty, value);
    }

    // ── Interaction state ─────────────────────────────────────────────────────

    private Factors? _hoveredFactor;
    private Factors? _pinnedFactor;
    private Factors? ActiveFactor => _pinnedFactor ?? _hoveredFactor;

    // ── Constructor ───────────────────────────────────────────────────────────

    public DeclArcCanvas()
    {
        InitializeComponent();

        ItemsProperty.Changed.AddClassHandler<DeclArcCanvas>((c, _) => c.InvalidateVisual());
        ThemeProperty.Changed.AddClassHandler<DeclArcCanvas>((c, _) => c.InvalidateVisual());
    }

    // ── Pointer handling ──────────────────────────────────────────────────────

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        var (center, r) = DrawDeclArc.ArcGeometry(Bounds.Size);
        var hit = DrawDeclArc.NearestFactor(e.GetPosition(this), center, r, Items);
        if (hit != _hoveredFactor)
        {
            _hoveredFactor = hit;
            InvalidateVisual();
        }
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        if (_hoveredFactor != null)
        {
            _hoveredFactor = null;
            InvalidateVisual();
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        var (center, r) = DrawDeclArc.ArcGeometry(Bounds.Size);
        var tapped = DrawDeclArc.NearestFactor(e.GetPosition(this), center, r, Items);
        _pinnedFactor = (tapped != null && tapped == _pinnedFactor) ? null : tapped;
        InvalidateVisual();
    }

    // ── Rendering ─────────────────────────────────────────────────────────────

    // ── Desired size ──────────────────────────────────────────────────────────

    protected override Size MeasureOverride(Size availableSize)
    {
        // Arc aspect ratio: width : height ≈ 1.30
        if (availableSize.Width > 0 && !double.IsInfinity(availableSize.Width))
        {
            var h = availableSize.Width / 1.30;
            if (!double.IsInfinity(availableSize.Height))
                h = System.Math.Min(h, availableSize.Height);
            return new Size(availableSize.Width, h);
        }
        return new Size(500, 385);
    }

    public override void Render(DrawingContext ctx)
    {
        base.Render(ctx);

        var size = Bounds.Size;
        if (size.Width <= 0 || size.Height <= 0) return;

        var (center, R) = DrawDeclArc.ArcGeometry(size);
        if (R <= 0) return;

        var items = Items;
        var theme = Theme;

        ctx.FillRectangle(Brushes.White, new Rect(size));

        DrawDeclArc.DrawBackground   (ctx, center, R, theme);
        DrawDeclArc.DrawDegreeLabels (ctx, center, R, theme);
        DrawDeclArc.Draw1DegTicks    (ctx, center, R, theme);
        DrawDeclArc.DrawHalfDegTicks (ctx, center, R, theme);
        DrawDeclArc.DrawRingStrokes  (ctx, center, R, theme);
        DrawDeclArc.DrawCenterCross  (ctx, center, R, theme);
        DrawDeclArc.DrawArrow        (ctx, center, R, theme);
        DrawDeclArc.DrawConnectLines (ctx, center, R, items, theme);
        DrawDeclArc.DrawGlyphs       (ctx, center, R, items, theme);
        DrawDeclArc.DrawNSLabels     (ctx, center, R, NorthLabel, SouthLabel, theme);

        if (ActiveFactor is { } active)
            DrawDeclArc.DrawOverlay(ctx, center, R, items, active, Orb);
    }
}
