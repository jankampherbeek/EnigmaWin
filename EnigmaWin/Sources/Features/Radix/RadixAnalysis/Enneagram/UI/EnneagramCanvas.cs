// EnneagramCanvas.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace EnigmaWin.Sources.Features.Radix.RadixAnalysis.Enneagram.UI;

public sealed class EnneagramCanvas : Canvas
{
    public static readonly DependencyProperty CirclesProperty =
        DependencyProperty.Register(nameof(Circles), typeof(IReadOnlyList<EnneagramCircleData>), typeof(EnneagramCanvas),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnDataChanged));

    public static readonly DependencyProperty LinesProperty =
        DependencyProperty.Register(nameof(Lines), typeof(IReadOnlyList<EnneagramLineData>), typeof(EnneagramCanvas),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnDataChanged));

    public IReadOnlyList<EnneagramCircleData>? Circles
    {
        get => (IReadOnlyList<EnneagramCircleData>?)GetValue(CirclesProperty);
        set => SetValue(CirclesProperty, value);
    }

    public IReadOnlyList<EnneagramLineData>? Lines
    {
        get => (IReadOnlyList<EnneagramLineData>?)GetValue(LinesProperty);
        set => SetValue(LinesProperty, value);
    }

    private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((EnneagramCanvas)d).Redraw();

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        Redraw();
    }

    private void Redraw()
    {
        Children.Clear();
        var circles = Circles;
        var lines   = Lines;
        if (circles == null || lines == null || circles.Count == 0) return;

        var w  = ActualWidth  > 0 ? ActualWidth  : 400;
        var h  = ActualHeight > 0 ? ActualHeight : 400;
        var cr = Math.Min(w, h) * 0.08;

        // Map type -> pixel position using the unit coordinates stored in CircleData
        var pos = circles.ToDictionary(c => c.Type, c => new Point(c.X * w, c.Y * h));

        // Draw lines first (beneath circles)
        foreach (var ld in lines)
        {
            if (!pos.TryGetValue(ld.FromType, out var from) || !pos.TryGetValue(ld.ToType, out var to)) continue;
            var line = new Line
            {
                X1 = from.X, Y1 = from.Y,
                X2 = to.X,   Y2 = to.Y,
                Stroke = Brushes.DarkSlateGray,
                StrokeThickness = 1.5
            };
            Children.Add(line);
        }

        // Draw circles
        foreach (var cd in circles)
        {
            if (!pos.TryGetValue(cd.Type, out var p)) continue;
            var ellipse = new Ellipse
            {
                Width  = cr * 2,
                Height = cr * 2,
                Fill   = new SolidColorBrush(cd.Color),
                Stroke = Brushes.DarkSlateGray,
                StrokeThickness = 1.5,
                ToolTip = $"{cd.Type} - {cd.Name}\n{cd.Tooltip}"
            };
            SetLeft(ellipse, p.X - cr);
            SetTop(ellipse,  p.Y - cr);
            Children.Add(ellipse);

            // Number inside circle
            var numTb = new TextBlock
            {
                Text          = cd.Type.ToString(),
                FontSize      = cr * 0.7,
                FontWeight    = FontWeights.Bold,
                Foreground    = Brushes.White,
                TextAlignment = TextAlignment.Center,
                Width         = cr * 2
            };
            SetLeft(numTb, p.X - cr);
            SetTop(numTb,  p.Y - cr * 0.55);
            Children.Add(numTb);

            // Name below circle
            var nameTb = new TextBlock
            {
                Text         = cd.Name,
                FontSize     = Math.Max(9, cr * 0.5),
                Foreground   = Brushes.Black,
                TextAlignment = TextAlignment.Center,
                Width        = cr * 3.5,
                TextWrapping = TextWrapping.Wrap
            };
            SetLeft(nameTb, p.X - cr * 1.75);
            SetTop(nameTb,  p.Y + cr + 2);
            Children.Add(nameTb);
        }
    }
}
