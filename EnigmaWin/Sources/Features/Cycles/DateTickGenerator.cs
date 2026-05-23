// DateTickGenerator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using ScottPlot;
using ScottPlot.TickGenerators;

namespace EnigmaWin.Sources.Features.Cycles;

/// <summary>
/// Wraps DateTimeFixedInterval and ensures all tick labels (major and minor) use yyyy/MM/dd.
/// The base class hardcodes Interval.GetDateTimeFormatString() for minor ticks and ignores
/// LabelFormatter for them; this wrapper patches the Ticks array after Regenerate runs.
/// Implements IDateTimeTickGenerator so DateTimeXAxis accepts it.
/// </summary>
public sealed class DateTickGenerator : IDateTimeTickGenerator
{
    private readonly DateTimeFixedInterval _inner;

    public Tick[] Ticks { get; private set; } = [];
    public int MaxTickCount { get; set; } = 10_000;

    public DateTickGenerator(
        ITimeUnit majorUnit, int majorStep,
        ITimeUnit minorUnit, int minorStep,
        Func<DateTime, DateTime> snap)
    {
        _inner = new DateTimeFixedInterval(majorUnit, majorStep, minorUnit, minorStep, snap)
        {
            LabelFormatter = dt => dt.ToString("yyyy/MM/dd"),
            MaxTickCount = MaxTickCount
        };
    }

    public IEnumerable<double> ConvertToCoordinateSpace(IEnumerable<DateTime> dates)
        => dates.Select(NumericConversion.ToNumber);

    public void Regenerate(CoordinateRange range, Edge edge, PixelLength size, Paint paint, LabelStyle labelStyle)
    {
        _inner.MaxTickCount = MaxTickCount;
        _inner.Regenerate(range, edge, size, paint, labelStyle);

        Ticks = _inner.Ticks
            .Select(t => new Tick(t.Position, NumericConversion.ToDateTime(t.Position).ToString("yyyy/MM/dd"), t.IsMajor))
            .ToArray();
    }
}
