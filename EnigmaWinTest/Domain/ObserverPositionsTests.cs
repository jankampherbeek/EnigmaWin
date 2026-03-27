// ObserverPositionsTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWintest.Domain;

/// <summary>Tests for ObserverPositions domain types.</summary>
[TestFixture]
public class ObserverPositionsTests
{

    [Test]
    public void TestAllObserverPositionsCases()
    {
        // Test that all cases exist
        var allCases = Enum.GetValues<ObserverPositions>();
        Assert.That(allCases, Contains.Item(ObserverPositions.Geocentric));
        Assert.That(allCases, Contains.Item(ObserverPositions.Topocentric));
        Assert.That(allCases, Contains.Item(ObserverPositions.Heliocentric));
    }

    [Test]
    public void TestAllCasesCompleteness()
    {
        var allCases = Enum.GetValues<ObserverPositions>();
        Assert.That(allCases, Contains.Item(ObserverPositions.Geocentric));
        Assert.That(allCases, Contains.Item(ObserverPositions.Topocentric));
        Assert.That(allCases, Contains.Item(ObserverPositions.Heliocentric));
        Assert.That(allCases, Has.Length.EqualTo(3)); // Total number of observer positions
    }

    // MARK: - LocalizedName Tests

    [Test]
    public void TestLocalizedNameGeocentric()
    {
        Assert.That(ObserverPositions.Geocentric.LocalizedName(), Is.EqualTo("enum.observerpos.geocentric"));
    }

    [Test]
    public void TestLocalizedNameTopocentric()
    {
        Assert.That(ObserverPositions.Topocentric.LocalizedName(), Is.EqualTo("enum.observerpos.topocentric"));
    }

    [Test]
    public void TestLocalizedNameHeliocentric()
    {
        Assert.That(ObserverPositions.Heliocentric.LocalizedName(), Is.EqualTo("enum.observerpos.heliocentric"));
    }

    [Test]
    public void TestLocalizedNameAll()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(ObserverPositions.Geocentric.LocalizedName(), Is.EqualTo("enum.observerpos.geocentric"));
            Assert.That(ObserverPositions.Topocentric.LocalizedName(), Is.EqualTo("enum.observerpos.topocentric"));
            Assert.That(ObserverPositions.Heliocentric.LocalizedName(), Is.EqualTo("enum.observerpos.heliocentric"));
        }
    }

    [Test]
    public void TestLocalizedNameAllObserverPositions()
    {
        var allCases = Enum.GetValues<ObserverPositions>();
        foreach (var observerPosition in allCases)
        {
            var name = observerPosition.LocalizedName();
            Assert.That(name, Is.Not.Empty, $"Observer position {observerPosition} has empty localized name");
            Assert.That(name, Does.StartWith("enum.observerpos"), 
                $"Observer position {observerPosition} localized name does not start with 'enum.observerpos'");
        }
    }

    // MARK: - FromIndex Tests

    [Test]
    public void TestFromIndexValid()
    {
        var allCases = Enum.GetValues<ObserverPositions>();
        for (var index = 0; index < allCases.Length; index++)
        {
            var expectedObserverPosition = allCases[index];
            var observerPosition = ObserverPositionsExtensions.FromIndex(index);
            Assert.That(observerPosition, Is.EqualTo(expectedObserverPosition), 
                $"Index {index} should return {expectedObserverPosition}");
        }
    }

    [Test]
    public void TestFromIndexFirst()
    {
        var observerPosition = ObserverPositionsExtensions.FromIndex(0);
        var allCases = Enum.GetValues<ObserverPositions>();
        Assert.That(observerPosition, Is.EqualTo(allCases.First()));
        Assert.That(observerPosition, Is.EqualTo(ObserverPositions.Geocentric));
    }

    [Test]
    public void TestFromIndexLast()
    {
        var allCases = Enum.GetValues<ObserverPositions>();
        var lastIndex = allCases.Length - 1;
        var observerPosition = ObserverPositionsExtensions.FromIndex(lastIndex);
        Assert.That(observerPosition, Is.EqualTo(allCases.Last()));
        Assert.That(observerPosition, Is.EqualTo(ObserverPositions.Heliocentric));
    }

    [Test]
    public void TestFromIndexNegative()
    {
        var observerPosition = ObserverPositionsExtensions.FromIndex(-1);
        Assert.That(observerPosition, Is.Null);
    }

    [Test]
    public void TestFromIndexTooLarge()
    {
        var allCases = Enum.GetValues<ObserverPositions>();
        var tooLargeIndex = allCases.Length;
        var observerPosition = ObserverPositionsExtensions.FromIndex(tooLargeIndex);
        Assert.That(observerPosition, Is.Null);
    }

    [Test]
    public void TestFromIndexBoundary()
    {
        var allCases = Enum.GetValues<ObserverPositions>();
        var boundaryIndex = allCases.Length;
        var observerPosition = ObserverPositionsExtensions.FromIndex(boundaryIndex);
        Assert.That(observerPosition, Is.Null);

        var validBoundaryIndex = allCases.Length - 1;
        var validObserverPosition = ObserverPositionsExtensions.FromIndex(validBoundaryIndex);
        Assert.That(validObserverPosition, Is.Not.Null);
        Assert.That(validObserverPosition, Is.EqualTo(ObserverPositions.Heliocentric));
    }

    [Test]
    public void TestFromIndexSpecific()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(ObserverPositionsExtensions.FromIndex(0), Is.EqualTo(ObserverPositions.Geocentric));
            Assert.That(ObserverPositionsExtensions.FromIndex(1), Is.EqualTo(ObserverPositions.Topocentric));
            Assert.That(ObserverPositionsExtensions.FromIndex(2), Is.EqualTo(ObserverPositions.Heliocentric));
        }
    }

    // MARK: - Raw Value Tests

    [Test]
    public void TestRawValuesSequential()
    {
        var allCases = Enum.GetValues<ObserverPositions>();
        for (var index = 0; index < allCases.Length; index++)
        {
            var observerPosition = allCases[index];
            Assert.That((int)observerPosition, Is.EqualTo(index), 
                $"Observer position {observerPosition} should have raw value {index}");
        }
    }

    [Test]
    public void TestRawValuesMatchExpected()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That((int)ObserverPositions.Geocentric, Is.EqualTo(0));
            Assert.That((int)ObserverPositions.Topocentric, Is.EqualTo(1));
            Assert.That((int)ObserverPositions.Heliocentric, Is.EqualTo(2));
        }
    }

    [Test]
    public void TestRawValuesUnique()
    {
        var rawValues = new HashSet<int>();
        var allCases = Enum.GetValues<ObserverPositions>();
        foreach (var observerPosition in allCases)
        {
            var rawValue = (int)observerPosition;
            Assert.That(rawValues, Does.Not.Contain(rawValue), 
                $"Duplicate raw value {rawValue} found for observer position {observerPosition}");
            rawValues.Add(rawValue);
        }
    }

    // MARK: - Comprehensive Tests

    [Test]
    public void TestAllObserverPositionsHaveLocalizedName()
    {
        var allCases = Enum.GetValues<ObserverPositions>();
        foreach (var observerPosition in allCases)
        {
            var name = observerPosition.LocalizedName();
            Assert.That(name, Is.Not.Empty, $"Observer position {observerPosition} has empty localized name");
        }
    }

    [Test]
    public void TestCaseIterable()
    {
        var allCases = Enum.GetValues<ObserverPositions>();
        Assert.That(allCases, Has.Length.EqualTo(3));

        // Verify we can iterate
        var count = 0;
        foreach (var _ in allCases)
        {
            count++;
        }
        Assert.That(count, Is.EqualTo(3));
    }

    [Test]
    public void TestIntBacked()
    {
        // Test that we can create from raw value
        const ObserverPositions geocentric = 0;
        Assert.That(geocentric, Is.EqualTo(ObserverPositions.Geocentric));

        const ObserverPositions topocentric = (ObserverPositions)1;
        Assert.That(topocentric, Is.EqualTo(ObserverPositions.Topocentric));

        const ObserverPositions heliocentric = (ObserverPositions)2;
        Assert.That(heliocentric, Is.EqualTo(ObserverPositions.Heliocentric));
    }

    [Test]
    public void TestAllObserverPositionsDistinct()
    {
        Assert.That(ObserverPositions.Geocentric, Is.Not.EqualTo(ObserverPositions.Topocentric));
        Assert.That(ObserverPositions.Geocentric, Is.Not.EqualTo(ObserverPositions.Heliocentric));
        Assert.That(ObserverPositions.Topocentric, Is.Not.EqualTo(ObserverPositions.Heliocentric));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(ObserverPositions.Geocentric.LocalizedName(), 
                Is.Not.EqualTo(ObserverPositions.Topocentric.LocalizedName()));
            Assert.That(ObserverPositions.Geocentric.LocalizedName(), 
                Is.Not.EqualTo(ObserverPositions.Heliocentric.LocalizedName()));
            Assert.That(ObserverPositions.Topocentric.LocalizedName(), 
                Is.Not.EqualTo(ObserverPositions.Heliocentric.LocalizedName()));
        }
    }

    [Test]
    public void TestPopularObserverPositions()
    {
        // Test the most commonly used observer positions
        var allCases = Enum.GetValues<ObserverPositions>();
        Assert.That(allCases, Contains.Item(ObserverPositions.Geocentric));
        Assert.That(allCases, Contains.Item(ObserverPositions.Topocentric));
        Assert.That(allCases, Contains.Item(ObserverPositions.Heliocentric));
    }

    [Test]
    public void TestLocalizedNameFormat()
    {
        var allCases = Enum.GetValues<ObserverPositions>();
        foreach (var observerPosition in allCases)
        {
            var name = observerPosition.LocalizedName();
            // Verify format: enum.observerpos.<name>
            Assert.That(name, Does.Match(@"^enum\.observerpos\.[a-z]+$"), 
                $"Observer position {observerPosition} localized name should match format 'enum.observerpos.<name>'");
        }
    }

    [Test]
    public void TestFromIndexAllValidIndices()
    {
        var allCases = Enum.GetValues<ObserverPositions>();
        for (var i = 0; i < allCases.Length; i++)
        {
            var observerPosition = ObserverPositionsExtensions.FromIndex(i);
            Assert.That(observerPosition, Is.Not.Null, $"Index {i} should return a valid observer position");
            Assert.That(observerPosition, Is.EqualTo(allCases[i]), 
                $"Index {i} should return {allCases[i]}");
        }
    }

    [Test]
    public void TestFromIndexInvalidIndices()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(ObserverPositionsExtensions.FromIndex(-1), Is.Null);
            Assert.That(ObserverPositionsExtensions.FromIndex(3), Is.Null);
            Assert.That(ObserverPositionsExtensions.FromIndex(100), Is.Null);
        }
    }
}

