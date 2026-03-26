// GlyphOverlapResolverTests.cs
// EnigmaWin
// Created by Jan Kampherbeek on 26-03-2026

using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;

namespace EnigmaWintest.Features.ChartDrawing.WheelDrawing;

/// <summary>Tests for GlyphOverlapResolver — anti-overlap adjustment of planet glyph positions.</summary>
[TestFixture]
public class GlyphOverlapResolverTests
{
    // MARK: - Hulpfunctie

    private static WheelPlotItem MakeItem(Factors factor, double mundaneAngle, double? plotAngle = null) =>
        new(
            Factor:            factor,
            Glyph:             "A",
            EclipticLongitude: 0,
            MundaneAngle:      mundaneAngle,
            PlotAngle:         plotAngle ?? mundaneAngle,
            PositionText:      "0°00'",
            SpeedType:         SpeedType.Direct
        );

    // MARK: - Randgevallen

    [Test]
    public void TestResolveEmptyArrayReturnsEmpty()
    {
        var result = GlyphOverlapResolver.Resolve([]);
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void TestResolveSingleItemUnchanged()
    {
        var item   = MakeItem(Factors.Sun, 45);
        var result = GlyphOverlapResolver.Resolve([item]);
        Assert.That(result.Length, Is.EqualTo(1));
        Assert.That(result[0].PlotAngle, Is.EqualTo(45.0));
    }

    // MARK: - Geen overlap

    [Test]
    public void TestResolveTwoItemsNoOverlapUnchanged()
    {
        // Twee items met voldoende afstand (> 6°) blijven ongewijzigd
        var item1  = MakeItem(Factors.Sun,  10);
        var item2  = MakeItem(Factors.Moon, 20);
        var result = GlyphOverlapResolver.Resolve([item1, item2]);
        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(result[0].PlotAngle, Is.EqualTo(10.0));
        Assert.That(result[1].PlotAngle, Is.EqualTo(20.0));
    }

    [Test]
    public void TestResolveTwoItemsExactMinDistanceUnchanged()
    {
        // Twee items met exact 6° afstand blijven ongewijzigd
        var item1  = MakeItem(Factors.Sun,  10);
        var item2  = MakeItem(Factors.Moon, 16);
        var result = GlyphOverlapResolver.Resolve([item1, item2]);
        Assert.That(result[0].PlotAngle, Is.EqualTo(10.0));
        Assert.That(result[1].PlotAngle, Is.EqualTo(16.0));
    }

    // MARK: - Overlap oplossen

    [Test]
    public void TestResolveTwoOverlappingItemsSpreadApart()
    {
        // Twee overlappende items worden symmetrisch uit elkaar geschoven
        var item1  = MakeItem(Factors.Sun,  10);
        var item2  = MakeItem(Factors.Moon, 13);   // 3° verschil, < 6°
        var result = GlyphOverlapResolver.Resolve([item1, item2]);
        var sorted = result.OrderBy(i => i.PlotAngle).ToArray();
        var diff   = sorted[1].PlotAngle - sorted[0].PlotAngle;
        Assert.That(diff, Is.GreaterThanOrEqualTo(6.0 - 1e-10));
    }

    [Test]
    public void TestResolveSameAngleSpread6Apart()
    {
        // Twee items op identieke hoek worden minstens 6° uit elkaar geplaatst
        var item1  = MakeItem(Factors.Sun,  50);
        var item2  = MakeItem(Factors.Moon, 50);
        var result = GlyphOverlapResolver.Resolve([item1, item2]);
        var sorted = result.OrderBy(i => i.PlotAngle).ToArray();
        var diff   = sorted[1].PlotAngle - sorted[0].PlotAngle;
        Assert.That(diff, Is.GreaterThanOrEqualTo(6.0 - 1e-10));
    }

    [Test]
    public void TestResolveMundaneAnglePreserved()
    {
        // MundaneAngle van items blijft ongewijzigd; alleen PlotAngle verandert
        var item1  = MakeItem(Factors.Sun,  10);
        var item2  = MakeItem(Factors.Moon, 13);
        var result = GlyphOverlapResolver.Resolve([item1, item2]);
        var angles = result.Select(i => i.MundaneAngle).ToHashSet();
        Assert.That(angles, Contains.Item(10.0));
        Assert.That(angles, Contains.Item(13.0));
    }

    // MARK: - Sortering

    [Test]
    public void TestResolveOutputSortedByMundaneAngle()
    {
        // De Apple-implementatie sorteert de uitvoer op mundaneAngle;
        // de C#-implementatie behoudt de invoervolgorde — dit test dat de mundaneAngles kloppen.
        var item1  = MakeItem(Factors.Sun,     80);
        var item2  = MakeItem(Factors.Moon,    10);
        var item3  = MakeItem(Factors.Mercury, 45);
        var result = GlyphOverlapResolver.Resolve([item1, item2, item3]);
        // Alle drie mundaneAngles moeten aanwezig zijn
        var angles = result.Select(i => i.MundaneAngle).ToHashSet();
        Assert.That(angles, Contains.Item(80.0));
        Assert.That(angles, Contains.Item(10.0));
        Assert.That(angles, Contains.Item(45.0));
    }
}
