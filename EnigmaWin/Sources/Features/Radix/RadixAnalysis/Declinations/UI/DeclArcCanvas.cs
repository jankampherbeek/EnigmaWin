// DeclArcCanvas.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Declinations.UI;

public sealed class DeclArcCanvas : FrameworkElement
{
    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(nameof(Items), typeof(IReadOnlyList<DeclArcItem>),
            typeof(DeclArcCanvas),
            new FrameworkPropertyMetadata(
                (IReadOnlyList<DeclArcItem>)Array.Empty<DeclArcItem>(),
                FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ThemeProperty =
        DependencyProperty.Register(nameof(Theme), typeof(WheelTheme), typeof(DeclArcCanvas),
            new FrameworkPropertyMetadata(WheelTheme.Color, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty NorthLabelProperty =
        DependencyProperty.Register(nameof(NorthLabel), typeof(string), typeof(DeclArcCanvas),
            new FrameworkPropertyMetadata("N", FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SouthLabelProperty =
        DependencyProperty.Register(nameof(SouthLabel), typeof(string), typeof(DeclArcCanvas),
            new FrameworkPropertyMetadata("S", FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty OrbProperty =
        DependencyProperty.Register(nameof(Orb), typeof(double), typeof(DeclArcCanvas),
            new FrameworkPropertyMetadata(0.75, FrameworkPropertyMetadataOptions.AffectsRender));

    public IReadOnlyList<DeclArcItem> Items
    {
        get => (IReadOnlyList<DeclArcItem>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public WheelTheme Theme
    {
        get => (WheelTheme)GetValue(ThemeProperty);
        set => SetValue(ThemeProperty, value);
    }

    public string NorthLabel
    {
        get => (string)GetValue(NorthLabelProperty);
        set => SetValue(NorthLabelProperty, value);
    }

    public string SouthLabel
    {
        get => (string)GetValue(SouthLabelProperty);
        set => SetValue(SouthLabelProperty, value);
    }

    public double Orb
    {
        get => (double)GetValue(OrbProperty);
        set => SetValue(OrbProperty, value);
    }

    private Factors? _hoveredFactor;
    private Factors? _pinnedFactor;
    private Factors? ActiveFactor => _pinnedFactor ?? _hoveredFactor;

    public DeclArcCanvas()
    {
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (availableSize.Width > 0 && !double.IsInfinity(availableSize.Width))
        {
            var h = availableSize.Width / 1.30;
            if (!double.IsInfinity(availableSize.Height))
                h = Math.Min(h, availableSize.Height);
            return new Size(availableSize.Width, h);
        }
        return new Size(500, 385);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        var (center, r) = DrawDeclArc.ArcGeometry(new Size(ActualWidth, ActualHeight));
        var pos = e.GetPosition(this);
        var hit = DrawDeclArc.NearestFactor(pos, center, r, Items);
        if (hit != _hoveredFactor)
        {
            _hoveredFactor = hit;
            InvalidateVisual();
        }
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (_hoveredFactor != null)
        {
            _hoveredFactor = null;
            InvalidateVisual();
        }
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        var (center, r) = DrawDeclArc.ArcGeometry(new Size(ActualWidth, ActualHeight));
        var pos    = e.GetPosition(this);
        var tapped = DrawDeclArc.NearestFactor(pos, center, r, Items);
        _pinnedFactor = (tapped != null && tapped == _pinnedFactor) ? null : tapped;
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext ctx)
    {
        var size = new Size(ActualWidth, ActualHeight);
        if (size.Width <= 0 || size.Height <= 0) return;

        var (center, R) = DrawDeclArc.ArcGeometry(size);
        if (R <= 0) return;

        ctx.DrawRectangle(Brushes.White, null, new Rect(size));

        var items = Items;
        var theme = Theme;

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
