// ImplicitAspectsTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWintest.Domain;

[TestFixture]
public class ImplicitAspectsTests
{
    /// <summary>All pairs within nodes, Dragon and Beast are implicit.</summary>
    [Test]
    public void TestNodeGroup()
    {
        Factors[] group =
        [
            Factors.NorthNodeMean, Factors.NorthNodeTrue, Factors.SouthNodeMean, Factors.SouthNodeTrue,
            Factors.Dragon, Factors.Beast
        ];
        foreach (var f1 in group)
            foreach (var f2 in group)
                if (f1 != f2)
                    Assert.That(ImplicitAspects.IsImplicit(f1, f2), Is.True, $"{f1} - {f2}");
    }

    /// <summary>Each Black Moon and its matching Priapus are implicit, in both orders.</summary>
    [Test]
    public void TestBlackMoonPriapus()
    {
        (Factors, Factors)[] pairs =
        [
            (Factors.ApogeeMean, Factors.Priapus), (Factors.ApogeeKoch, Factors.PriapusKoch),
            (Factors.ApogeeDuval, Factors.PriapusDuval), (Factors.ApogeeInterpolated, Factors.PriapusInterpolated)
        ];
        foreach (var (a, b) in pairs)
        {
            Assert.That(ImplicitAspects.IsImplicit(a, b), Is.True);
            Assert.That(ImplicitAspects.IsImplicit(b, a), Is.True);
        }
    }

    /// <summary>Black Sun and Diamond are implicit.</summary>
    [Test]
    public void TestBlackSunDiamond()
    {
        Assert.That(ImplicitAspects.IsImplicit(Factors.BlackSun, Factors.Diamond), Is.True);
        Assert.That(ImplicitAspects.IsImplicit(Factors.Diamond, Factors.BlackSun), Is.True);
    }

    /// <summary>Unrelated or mismatched pairs are not implicit.</summary>
    [Test]
    public void TestNotImplicit()
    {
        Assert.That(ImplicitAspects.IsImplicit(Factors.Sun, Factors.Moon), Is.False);
        Assert.That(ImplicitAspects.IsImplicit(Factors.NorthNodeMean, Factors.Moon), Is.False);
        Assert.That(ImplicitAspects.IsImplicit(Factors.ApogeeMean, Factors.PriapusKoch), Is.False);
        Assert.That(ImplicitAspects.IsImplicit(Factors.ApogeeKoch, Factors.Priapus), Is.False);
        Assert.That(ImplicitAspects.IsImplicit(Factors.BlackSun, Factors.ApogeeMean), Is.False);
        Assert.That(ImplicitAspects.IsImplicit(Factors.NorthNodeMean, Factors.NorthNodeMean), Is.False);
    }
}
