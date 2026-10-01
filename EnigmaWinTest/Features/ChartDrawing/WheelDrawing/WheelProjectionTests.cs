// WheelProjectionTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Linq;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.ChartDrawing.WheelDrawing;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWintest.Features.ChartDrawing.WheelDrawing;

/// <summary>Tests for WheelProjection — conversion of zodiac-based wheel data to other drawing types.</summary>
[TestFixture]
public class WheelProjectionTests
{
    private const double Delta = 1e-9;
    private const double Asc   = 100.0;
    private const double Mc    = 10.0;

    // Unequal houses, starting at the ascendant
    private static readonly double[] Cusps = [100, 130, 160, 190, 220, 250, 280, 310, 340, 10, 40, 70];

    private static WheelPlotItem MakeItem(Factors factor, double longitude) =>
        new(
            Factor:            factor,
            Glyph:             "A",
            EclipticLongitude: longitude,
            MundaneAngle:      WheelGeometry.MundaneAngle(longitude, Asc),
            PlotAngle:         WheelGeometry.MundaneAngle(longitude, Asc),
            PositionText:      "zodiac",
            SpeedType:         SpeedType.Direct);

    private static WheelPlotData MakeData(bool hasTime = true, params WheelPlotItem[] items) =>
        new(Asc, Mc, hasTime ? Cusps : [], items, hasTime, []);

    // MARK: - LongitudeToAngle

    [Test]
    public void LongitudeToAngle_SignBased_UsesMundaneAngle()
    {
        Assert.That(WheelProjection.LongitudeToAngle(DrawingTypes.SignBased, 130.0, Asc, Cusps),
            Is.EqualTo(WheelGeometry.MundaneAngle(130.0, Asc)).Within(Delta));
    }

    [Test]
    public void LongitudeToAngle_FrenchAndRing_UseMundaneAngle()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(WheelProjection.LongitudeToAngle(DrawingTypes.French, 250.0, Asc, Cusps),
                Is.EqualTo(WheelGeometry.MundaneAngle(250.0, Asc)).Within(Delta));
            Assert.That(WheelProjection.LongitudeToAngle(DrawingTypes.Ring, 250.0, Asc, Cusps),
                Is.EqualTo(WheelGeometry.MundaneAngle(250.0, Asc)).Within(Delta));
        }
    }

    [Test]
    public void LongitudeToAngle_HouseBased_UsesHouseAngle()
    {
        Assert.That(WheelProjection.LongitudeToAngle(DrawingTypes.HouseBased, 145.0, Asc, Cusps),
            Is.EqualTo(HouseWheelPlotDataBuilder.EclipticToHouseAngle(145.0, Cusps)).Within(Delta));
    }

    [Test]
    public void LongitudeToAngle_Dials_UseDialAngles()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(WheelProjection.LongitudeToAngle(DrawingTypes.Dial360, 200.0, Asc, Cusps), Is.EqualTo(200.0).Within(Delta));
            Assert.That(WheelProjection.LongitudeToAngle(DrawingTypes.Dial90,  200.0, Asc, Cusps), Is.EqualTo(80.0).Within(Delta));
            Assert.That(WheelProjection.LongitudeToAngle(DrawingTypes.Dial45,  200.0, Asc, Cusps), Is.EqualTo(160.0).Within(Delta));
        }
    }

    // MARK: - ZodiacAngleToAngle

    [Test]
    public void ZodiacAngleToAngle_RecoversLongitude()
    {
        var zodiacAngle = WheelGeometry.MundaneAngle(200.0, Asc);
        Assert.That(WheelProjection.ZodiacAngleToAngle(DrawingTypes.Dial360, zodiacAngle, Asc, Cusps),
            Is.EqualTo(200.0).Within(Delta));
    }

    // MARK: - Effective

    [Test]
    public void Effective_HouseBasedWithoutTime_FallsBackToSignBased()
    {
        Assert.That(WheelProjection.Effective(DrawingTypes.HouseBased, MakeData(hasTime: false)),
            Is.EqualTo(DrawingTypes.SignBased));
    }

    [Test]
    public void Effective_HouseBasedWithCusps_IsKept()
    {
        Assert.That(WheelProjection.Effective(DrawingTypes.HouseBased, MakeData()),
            Is.EqualTo(DrawingTypes.HouseBased));
    }

    // MARK: - ProjectChart

    [Test]
    public void ProjectChart_SignBased_ReturnsSameData()
    {
        var data = MakeData(true, MakeItem(Factors.Sun, 130.0));
        Assert.That(WheelProjection.ProjectChart(data, DrawingTypes.SignBased), Is.SameAs(data));
    }

    [Test]
    public void ProjectChart_Dial_AddsAscendantAndMcAndClearsCusps()
    {
        var data   = MakeData(true, MakeItem(Factors.Sun, 130.0));
        var result = WheelProjection.ProjectChart(data, DrawingTypes.Dial90);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.CuspLongitudes, Is.Empty);
            Assert.That(result.PlanetItems.Select(i => i.Factor),
                Is.EquivalentTo(new[] { Factors.Sun, Factors.Ascendant, Factors.Mc }));
            Assert.That(result.PlanetItems.Single(i => i.Factor == Factors.Sun).MundaneAngle,
                Is.EqualTo(160.0).Within(Delta));
            Assert.That(result.PlanetItems.Single(i => i.Factor == Factors.Sun).PositionText,
                Is.EqualTo("40°00'"));
        }
    }

    [Test]
    public void ProjectChart_DialWithoutTime_DoesNotAddAscendantAndMc()
    {
        var data   = MakeData(false, MakeItem(Factors.Sun, 130.0));
        var result = WheelProjection.ProjectChart(data, DrawingTypes.Dial360);

        Assert.That(result.PlanetItems.Select(i => i.Factor), Is.EquivalentTo(new[] { Factors.Sun }));
    }

    [Test]
    public void ProjectChart_HouseBased_MapsItemsToHouseAngles()
    {
        var data   = MakeData(true, MakeItem(Factors.Sun, 145.0));
        var result = WheelProjection.ProjectChart(data, DrawingTypes.HouseBased);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.PlanetItems[0].MundaneAngle,
                Is.EqualTo(HouseWheelPlotDataBuilder.EclipticToHouseAngle(145.0, Cusps)).Within(Delta));
            Assert.That(result.PlanetItems[0].PositionText, Is.EqualTo("zodiac"));
            Assert.That(result.CuspLongitudes, Is.EqualTo(Cusps));
        }
    }

    [Test]
    public void ProjectChart_Dial360_KeepsAspects_Dial90DropsThem()
    {
        var aspect = new WheelAspectItem(WheelGeometry.MundaneAngle(10.0, Asc), WheelGeometry.MundaneAngle(130.0, Asc),
                                         System.Windows.Media.Colors.Red, 1.0, Aspects.Trine);
        var data   = MakeData(true, MakeItem(Factors.Sun, 10.0), MakeItem(Factors.Moon, 130.0)) with { AspectItems = [aspect] };

        var dial360 = WheelProjection.ProjectChart(data, DrawingTypes.Dial360);
        var dial90  = WheelProjection.ProjectChart(data, DrawingTypes.Dial90);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(dial360.AspectItems, Has.Length.EqualTo(1));
            Assert.That(dial360.AspectItems[0].Angle1, Is.EqualTo(10.0).Within(Delta));
            Assert.That(dial360.AspectItems[0].Angle2, Is.EqualTo(130.0).Within(Delta));
            Assert.That(dial90.AspectItems, Is.Empty);
        }
    }

    // MARK: - Specialised drawing types

    [TestCase(DrawingTypes.SignBased,  DrawingTypes.SignBased)]
    [TestCase(DrawingTypes.HouseBased, DrawingTypes.SignBased)]
    [TestCase(DrawingTypes.French,     DrawingTypes.French)]
    [TestCase(DrawingTypes.Ring,       DrawingTypes.Ring)]
    [TestCase(DrawingTypes.Dial360,    DrawingTypes.Dial360)]
    [TestCase(DrawingTypes.Dial90,     DrawingTypes.Dial360)]
    [TestCase(DrawingTypes.Dial45,     DrawingTypes.Dial360)]
    public void Specialised_LimitsDrawingTypes(DrawingTypes configured, DrawingTypes expected)
    {
        Assert.That(EnigmaWin.Sources.Features.ChartDrawing.UI.WheelRenderer.Specialised(configured), Is.EqualTo(expected));
    }

    [TestCase(DrawingTypes.HouseBased, DrawingTypes.HouseBased)]
    [TestCase(DrawingTypes.Dial45,     DrawingTypes.Dial360)]
    [TestCase(DrawingTypes.Ring,       DrawingTypes.Ring)]
    public void SpecialisedWithHouses_KeepsHouseBased(DrawingTypes configured, DrawingTypes expected)
    {
        Assert.That(EnigmaWin.Sources.Features.ChartDrawing.UI.WheelRenderer.SpecialisedWithHouses(configured), Is.EqualTo(expected));
    }

    // MARK: - ProjectItems

    [Test]
    public void ProjectItems_Dial360_UsesLongitudes()
    {
        var items  = new[] { MakeItem(Factors.Moon, 300.0) };
        var result = WheelProjection.ProjectItems(items, DrawingTypes.Dial360, Asc, Cusps);

        Assert.That(result[0].MundaneAngle, Is.EqualTo(300.0).Within(Delta));
    }

    [Test]
    public void ProjectItems_SignBased_ReturnsSameItems()
    {
        var items = new[] { MakeItem(Factors.Moon, 300.0) };
        Assert.That(WheelProjection.ProjectItems(items, DrawingTypes.SignBased, Asc, Cusps), Is.SameAs(items));
    }
}
