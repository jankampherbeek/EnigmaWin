
using EnigmaWin.Sources.Domain;

namespace EnigmaWintest.Domain;

/// <summary>Tests for DateAndTime domain types.</summary>
[TestFixture]
public class DateAndTimeTests
{

    [Test]
    public void TestAstronomicalDateFullInitialization()
    {
        var date = new AstronomicalDate(2025, 1, 15, true);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(date.Year, Is.EqualTo(2025));
            Assert.That(date.Month, Is.EqualTo(1));
            Assert.That(date.Day, Is.EqualTo(15));
            Assert.That(date.Gregorian, Is.True);
        }
    }

    [Test]
    public void TestAstronomicalDateDefaultGregorian()
    {
        var date = new AstronomicalDate(2025, 1, 15);
        Assert.That(date.Gregorian, Is.True);
    }

    [Test]
    public void TestAstronomicalDateJulian()
    {
        var date = new AstronomicalDate(2025, 1, 15, false);
        Assert.That(date.Gregorian, Is.False);
    }

    [Test]
    public void TestAstronomicalDateYearZero()
    {
        var date = new AstronomicalDate(0, 1, 1, true);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(date.Year, Is.EqualTo(0));
            Assert.That(date.Month, Is.EqualTo(1));
            Assert.That(date.Day, Is.EqualTo(1));
        }
    }

    [Test]
    public void TestAstronomicalDateNegativeYear()
    {
        var date = new AstronomicalDate(-1, 1, 1, true);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(date.Year, Is.EqualTo(-1));
            Assert.That(date.Month, Is.EqualTo(1));
            Assert.That(date.Day, Is.EqualTo(1));
        }
    }

    [Test]
    public void TestAstronomicalDateLeapYear()
    {
        var date = new AstronomicalDate(2024, 2, 29, true);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(date.Year, Is.EqualTo(2024));
            Assert.That(date.Month, Is.EqualTo(2));
            Assert.That(date.Day, Is.EqualTo(29));
        }
    }

    [Test]
    public void TestAstronomicalDateFirstDayOfYear()
    {
        var date = new AstronomicalDate(2025, 1, 1, true);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(date.Year, Is.EqualTo(2025));
            Assert.That(date.Month, Is.EqualTo(1));
            Assert.That(date.Day, Is.EqualTo(1));
        }
    }

    [Test]
    public void TestAstronomicalDateLastDayOfYear()
    {
        var date = new AstronomicalDate(2025, 12, 31, true);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(date.Year, Is.EqualTo(2025));
            Assert.That(date.Month, Is.EqualTo(12));
            Assert.That(date.Day, Is.EqualTo(31));
        }
    }

    [Test]
    public void TestAstronomicalTimeFullInitialization()
    {
        var time = new AstronomicalTime(12, 30, 45);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(time.Hour, Is.EqualTo(12));
            Assert.That(time.Minute, Is.EqualTo(30));
            Assert.That(time.Second, Is.EqualTo(45));
        }
    }

    [Test]
    public void TestAstronomicalTimeExactHour()
    {
        var time = new AstronomicalTime(5, 0, 0);
        Assert.That(time.HourDecimal, Is.EqualTo(5.0));
    }

    [Test]
    public void TestAstronomicalTimeHourWithMinutes()
    {
        var time = new AstronomicalTime(2, 30, 0);
        // 2 + 30/60 = 2.5
        Assert.That(time.HourDecimal, Is.EqualTo(2.5));
    }

    [Test]
    public void TestAstronomicalTimeHourWithSeconds()
    {
        var time = new AstronomicalTime(1, 0, 30);
        // 1 + 0/60 + 30/3600 = 1 + 0.008333... = 1.008333...
        Assert.That(time.HourDecimal, Is.EqualTo(1.0 + 30.0 / 3600.0));
    }

    [Test]
    public void TestAstronomicalTimeFullComponents()
    {
        var time = new AstronomicalTime(1, 30, 45);
        // 1 + 30/60 + 45/3600 = 1 + 0.5 + 0.0125 = 1.5125
        Assert.That(time.HourDecimal, Is.EqualTo(1.5125));
    }

    [Test]
    public void TestAstronomicalTimeMidnight()
    {
        var time = new AstronomicalTime(0, 0, 0);
        Assert.That(time.HourDecimal, Is.EqualTo(0.0));
    }

    [Test]
    public void TestAstronomicalTimeEndOfDay()
    {
        var time = new AstronomicalTime(23, 59, 59);
        // 23 + 59/60 + 59/3600 = 23 + 0.983333... + 0.016388... = 23.999722...
        var expected = 23.0 + 59.0 / 60.0 + 59.0 / 3600.0;
        Assert.That(Math.Abs(time.HourDecimal - expected), Is.LessThan(0.000001));
    }

    [Test]
    public void TestAstronomicalTimeNoon()
    {
        var time = new AstronomicalTime(12, 0, 0);
        Assert.That(time.HourDecimal, Is.EqualTo(12.0));
    }

    [Test]
    public void TestAstronomicalTimeMinutesOnly()
    {
        var time = new AstronomicalTime(0, 15, 0);
        // 0 + 15/60 = 0.25
        Assert.That(time.HourDecimal, Is.EqualTo(0.25));
    }

    [Test]
    public void TestAstronomicalTimeSecondsOnly()
    {
        var time = new AstronomicalTime(0, 0, 15);
        // 0 + 0/60 + 15/3600 = 0.0041666...
        var expected = 15.0 / 3600.0;
        Assert.That(Math.Abs(time.HourDecimal - expected), Is.LessThan(0.000001));
    }

    [Test]
    public void TestAstronomicalTimePreciseFractional()
    {
        var time = new AstronomicalTime(10, 30, 30);
        // 10 + 30/60 + 30/3600 = 10 + 0.5 + 0.008333... = 10.508333...
        var expected = 10.0 + 30.0 / 60.0 + 30.0 / 3600.0;
        Assert.That(Math.Abs(time.HourDecimal - expected), Is.LessThan(0.000001));
    }

    [Test]
    public void TestAstronomicalTimeLargeHour()
    {
        var time = new AstronomicalTime(23, 0, 0);
        Assert.That(time.HourDecimal, Is.EqualTo(23.0));
    }
    

    [Test]
    public void TestAstronomicalTimeFromDecimalExactHour()
    {
        var time = new AstronomicalTime(5.0);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(time.HourDecimal, Is.EqualTo(5.0));
            Assert.That(time.Hour, Is.EqualTo(5));
            Assert.That(time.Minute, Is.EqualTo(0));
            Assert.That(time.Second, Is.EqualTo(0));
        }
    }

    [Test]
    public void TestAstronomicalTimeFromDecimalMidnight()
    {
        var time = new AstronomicalTime(0.0);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(time.HourDecimal, Is.EqualTo(0.0));
            Assert.That(time.Hour, Is.EqualTo(0));
            Assert.That(time.Minute, Is.EqualTo(0));
            Assert.That(time.Second, Is.EqualTo(0));
        }
    }

    [Test]
    public void TestAstronomicalTimeFromDecimalNoon()
    {
        var time = new AstronomicalTime(12.0);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(time.HourDecimal, Is.EqualTo(12.0));
            Assert.That(time.Hour, Is.EqualTo(12));
            Assert.That(time.Minute, Is.EqualTo(0));
            Assert.That(time.Second, Is.EqualTo(0));
        }
    }

    [Test]
    public void TestAstronomicalTimeFromDecimalHalfHour()
    {
        var time = new AstronomicalTime(2.5);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(time.HourDecimal, Is.EqualTo(2.5));
            Assert.That(time.Hour, Is.EqualTo(2));
            Assert.That(time.Minute, Is.EqualTo(30));
            Assert.That(time.Second, Is.EqualTo(0));
        }
    }

    [Test]
    public void TestAstronomicalTimeFromDecimalQuarterHour()
    {
        var time = new AstronomicalTime(1.25);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(time.HourDecimal, Is.EqualTo(1.25));
            Assert.That(time.Hour, Is.EqualTo(1));
            Assert.That(time.Minute, Is.EqualTo(15));
            Assert.That(time.Second, Is.EqualTo(0));
        }
    }

    [Test]
    public void TestAstronomicalTimeFromDecimalWithSeconds()
    {
        var time = new AstronomicalTime(1.5125);
        using (Assert.EnterMultipleScope())
        {
            // 1.5125 = 1 hour, 30 minutes, 45 seconds
            Assert.That(time.HourDecimal, Is.EqualTo(1.5125));
            Assert.That(time.Hour, Is.EqualTo(1));
            Assert.That(time.Minute, Is.EqualTo(30));
            Assert.That(time.Second, Is.EqualTo(45));
        }
    }

    [Test]
    public void TestAstronomicalTimeFromDecimalPreciseFractional()
    {
        var time = new AstronomicalTime(10.508333333333333);
        using (Assert.EnterMultipleScope())
        {
            // 10.508333... = 10 hours, 30 minutes, 30 seconds
            Assert.That(time.Hour, Is.EqualTo(10));
            Assert.That(time.Minute, Is.EqualTo(30));
            Assert.That(time.Second, Is.EqualTo(30));
        }
    }

    [Test]
    public void TestAstronomicalTimeFromDecimalSmallFractional()
    {
        var time = new AstronomicalTime(0.004166666666666667);
        using (Assert.EnterMultipleScope())
        {
            // 0.004166... = 15 seconds
            Assert.That(time.Hour, Is.EqualTo(0));
            Assert.That(time.Minute, Is.EqualTo(0));
            Assert.That(time.Second, Is.EqualTo(15));
        }
    }

    [Test]
    public void TestAstronomicalTimeFromDecimalEndOfDay()
    {
        var time = new AstronomicalTime(23.999722222222222);
        using (Assert.EnterMultipleScope())
        {
            // 23.999722... = 23 hours, 59 minutes, 59 seconds
            Assert.That(time.Hour, Is.EqualTo(23));
            Assert.That(time.Minute, Is.EqualTo(59));
            Assert.That(time.Second, Is.EqualTo(59));
        }
    }

    [Test]
    public void TestAstronomicalTimeFromDecimalRoundTrip()
    {
        // Create from components, then recreate from decimal
        var original = new AstronomicalTime(12, 30, 45);
        var reconstructed = new AstronomicalTime(original.HourDecimal);
        using (Assert.EnterMultipleScope())
        {
            // All components should match exactly
            Assert.That(reconstructed.HourDecimal, Is.EqualTo(original.HourDecimal));
            Assert.That(reconstructed.Hour, Is.EqualTo(original.Hour));
            Assert.That(reconstructed.Minute, Is.EqualTo(original.Minute));
            Assert.That(reconstructed.Second, Is.EqualTo(original.Second));
        }
    }

    [Test]
    public void TestAstronomicalTimeFromDecimalReverseRoundTrip()
    {
        // Create from decimal, then recreate from components
        var original = new AstronomicalTime(8.25361111111);
        using (Assert.EnterMultipleScope())
        {
            // 8.253472... = 8 hours, 15 minutes, 13 seconds
            Assert.That(original.Hour, Is.EqualTo(8));
            Assert.That(original.Minute, Is.EqualTo(15));
            Assert.That(original.Second, Is.EqualTo(13));
        }
        var reconstructed = new AstronomicalTime(original.Hour, original.Minute, original.Second);
        using (Assert.EnterMultipleScope())
        {
            // The reconstructed decimal should match the original (within floating point precision)
            Assert.That(Math.Abs(reconstructed.HourDecimal - original.HourDecimal), Is.LessThan(0.0001));
            Assert.That(reconstructed.Hour, Is.EqualTo(original.Hour));
            Assert.That(reconstructed.Minute, Is.EqualTo(original.Minute));
            Assert.That(reconstructed.Second, Is.EqualTo(original.Second));
        }
    }

    [Test]
    public void TestAstronomicalTimeFromDecimalMinutesOnly()
    {
        var time = new AstronomicalTime(0.25);
        using (Assert.EnterMultipleScope())
        {
            // 0.25 = 15 minutes
            Assert.That(time.Hour, Is.EqualTo(0));
            Assert.That(time.Minute, Is.EqualTo(15));
            Assert.That(time.Second, Is.EqualTo(0));
        }
    }

    [Test]
    public void TestAstronomicalTimeFromDecimalSecondsOnly()
    {
        var time = new AstronomicalTime(0.004166666666666667);
        using (Assert.EnterMultipleScope())
        {
            // 0.004166... = 15 seconds
            Assert.That(time.Hour, Is.EqualTo(0));
            Assert.That(time.Minute, Is.EqualTo(0));
            Assert.That(time.Second, Is.EqualTo(15));
        }
    }

    [Test]
    public void TestAstronomicalTimeFromDecimalLargeHour()
    {
        var time = new AstronomicalTime(23.0);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(time.HourDecimal, Is.EqualTo(23.0));
            Assert.That(time.Hour, Is.EqualTo(23));
            Assert.That(time.Minute, Is.EqualTo(0));
            Assert.That(time.Second, Is.EqualTo(0));
        }
    }

    [Test]
    public void TestAstronomicalTimeFromDecimalComplexTime()
    {
        var time = new AstronomicalTime(14.375);
        using (Assert.EnterMultipleScope())
        {
            // 14.375 = 14 hours, 22 minutes, 30 seconds
            Assert.That(time.Hour, Is.EqualTo(14));
            Assert.That(time.Minute, Is.EqualTo(22));
            Assert.That(time.Second, Is.EqualTo(30));
        }
    }

    [Test]
    public void TestAstronomicalTimeFromDecimalPrecision()
    {
        // Test with a value that has many decimal places
        var time = new AstronomicalTime(6.123456789);
        using (Assert.EnterMultipleScope())
        {
            // 6.123456789 = 6 hours, 7 minutes, 24.444... seconds (truncated to 24)
            Assert.That(time.Hour, Is.EqualTo(6));
            Assert.That(time.Minute, Is.EqualTo(7));
            Assert.That(time.Second, Is.EqualTo(24));
        }
    }
}

