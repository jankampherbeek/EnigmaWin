// GlyphOverlapResolverTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

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

    // MARK: - Clusters

    private static void AssertNoOverlap(double[] angles, double minDistance = 6.0)
    {
        for (var i = 0; i < angles.Length; i++)
            for (var j = i + 1; j < angles.Length; j++)
            {
                var diff = System.Math.Abs(angles[i] - angles[j]) % 360.0;
                diff = System.Math.Min(diff, 360.0 - diff);
                Assert.That(diff, Is.GreaterThanOrEqualTo(minDistance - 1e-9), $"items {i} and {j} overlap");
            }
    }

    [Test]
    public void TestResolveLargeClusterHasNoOverlap()
    {
        // Een stellium van 7 factoren binnen 5° moet volledig worden uitgespreid
        var result = GlyphOverlapResolver.ResolvedPlotAngles([100.0, 101.0, 101.5, 102.0, 103.0, 104.0, 105.0]);
        AssertNoOverlap(result);
    }

    [Test]
    public void TestResolveClusterAcrossZeroHasNoOverlap()
    {
        var result = GlyphOverlapResolver.ResolvedPlotAngles([358.0, 359.0, 0.5, 1.0, 2.0]);
        AssertNoOverlap(result);
    }

    [Test]
    public void TestResolveClusterStaysCentred()
    {
        // Drie factoren op dezelfde hoek: de middelste blijft op zijn plaats, de andere op ±6°
        var result = GlyphOverlapResolver.ResolvedPlotAngles([200.0, 200.0, 200.0]).OrderBy(a => a).ToArray();
        Assert.That(result[0], Is.EqualTo(194.0).Within(1e-9));
        Assert.That(result[1], Is.EqualTo(200.0).Within(1e-9));
        Assert.That(result[2], Is.EqualTo(206.0).Within(1e-9));
    }

    [Test]
    public void TestResolveIsolatedItemUnchangedNextToCluster()
    {
        var result = GlyphOverlapResolver.ResolvedPlotAngles([10.0, 11.0, 12.0, 90.0]);
        Assert.That(result[3], Is.EqualTo(90.0));
        AssertNoOverlap(result);
    }

    [Test]
    public void TestResolveCrowdedRingHasNoOverlap()
    {
        // 15 factoren plus 12 cuspen, verspreid met clusters: zoals de buitenring van de solar
        double[] angles = [5, 7, 9, 10, 12, 40, 41, 44, 95, 98, 100, 101, 180, 182, 300,
                           2, 30, 60, 91, 120, 150, 183, 210, 240, 270, 299, 330];
        AssertNoOverlap(GlyphOverlapResolver.ResolvedPlotAngles(angles));
    }

    [Test]
    public void TestResolveKeepsInputOrder()
    {
        var result = GlyphOverlapResolver.ResolvedPlotAngles([50.0, 10.0, 11.0]);
        Assert.That(result[0], Is.EqualTo(50.0));
        Assert.That(result[1], Is.LessThan(result[2]));
    }
}
