// DateTimeValidationTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Validation;

namespace EnigmaWintest.Features.Shared.Validation;

/// <summary>Tests for AstronomicalDateValidation utilities.</summary>
[TestFixture]
public class DateTimeValidationTests
{

    [Test]
    public void TestValidateDateValidGregorian()
    {
        var date = new AstronomicalDate(2025, 1, 15, true);
        Assert.That(AstronomicalDateValidation.ValidateDate(date), Is.True);
    }

    [Test]
    public void TestValidateDateValidJulian()
    {
        var date = new AstronomicalDate(2025, 1, 15, false);
        Assert.That(AstronomicalDateValidation.ValidateDate(date), Is.True);
    }

    [Test]
    public void TestValidateDateLeapYear()
    {
        var date = new AstronomicalDate(2024, 2, 29, true);
        Assert.That(AstronomicalDateValidation.ValidateDate(date), Is.True);
    }

    [Test]
    public void TestValidateDateInvalidFebruary30()
    {
        var date = new AstronomicalDate(2024, 2, 30, true);
        Assert.That(AstronomicalDateValidation.ValidateDate(date), Is.False);
    }

    [Test]
    public void TestValidateDateInvalidApril31()
    {
        var date = new AstronomicalDate(2025, 4, 31, true);
        Assert.That(AstronomicalDateValidation.ValidateDate(date), Is.False);
    }

    [Test]
    public void TestValidateDateInvalidMonth13()
    {
        var date = new AstronomicalDate(2025, 13, 1, true);
        Assert.That(AstronomicalDateValidation.ValidateDate(date), Is.False);
    }

    [Test]
    public void TestValidateDateInvalidMonth0()
    {
        var date = new AstronomicalDate(2025, 0, 1, true);
        Assert.That(AstronomicalDateValidation.ValidateDate(date), Is.False);
    }

    [Test]
    public void TestValidateDateInvalidDay0()
    {
        var date = new AstronomicalDate(2025, 1, 0, true);
        Assert.That(AstronomicalDateValidation.ValidateDate(date), Is.False);
    }

    [Test]
    public void TestValidateDateInvalidDay32()
    {
        var date = new AstronomicalDate(2025, 1, 32, true);
        Assert.That(AstronomicalDateValidation.ValidateDate(date), Is.False);
    }

    [Test]
    public void TestValidateDateFirstDayOfYear()
    {
        var date = new AstronomicalDate(2025, 1, 1, true);
        Assert.That(AstronomicalDateValidation.ValidateDate(date), Is.True);
    }

    [Test]
    public void TestValidateDateLastDayOfYear()
    {
        var date = new AstronomicalDate(2025, 12, 31, true);
        Assert.That(AstronomicalDateValidation.ValidateDate(date), Is.True);
    }

    [Test]
    public void TestValidateDateNonLeapYearFebruary28()
    {
        var date = new AstronomicalDate(2025, 2, 28, true);
        Assert.That(AstronomicalDateValidation.ValidateDate(date), Is.True);
    }

    [Test]
    public void TestValidateDateNonLeapYearFebruary29()
    {
        var date = new AstronomicalDate(2025, 2, 29, true);
        Assert.That(AstronomicalDateValidation.ValidateDate(date), Is.False);
    }

    [Test]
    public void TestValidateDateYearZero()
    {
        var date = new AstronomicalDate(0, 1, 1, true);
        Assert.That(AstronomicalDateValidation.ValidateDate(date), Is.True);
    }

    [Test]
    public void TestValidateDateNegativeYear()
    {
        var date = new AstronomicalDate(-1, 1, 1, true);
        Assert.That(AstronomicalDateValidation.ValidateDate(date), Is.True);
    }

    // MARK: - validateTime Tests

    [Test]
    public void TestValidateTimeMidnight()
    {
        var time = new AstronomicalTime(0, 0, 0);
        Assert.That(AstronomicalDateValidation.ValidateTime(time), Is.True);
    }

    [Test]
    public void TestValidateTimeNoon()
    {
        var time = new AstronomicalTime(12, 0, 0);
        Assert.That(AstronomicalDateValidation.ValidateTime(time), Is.True);
    }

    [Test]
    public void TestValidateTimeEndOfDay()
    {
        var time = new AstronomicalTime(23, 59, 59);
        Assert.That(AstronomicalDateValidation.ValidateTime(time), Is.True);
    }

    [Test]
    public void TestValidateTimeArbitraryValid()
    {
        var time = new AstronomicalTime(14, 30, 45);
        Assert.That(AstronomicalDateValidation.ValidateTime(time), Is.True);
    }

    [Test]
    public void TestValidateTimeInvalidHour24()
    {
        var time = new AstronomicalTime(24, 0, 0);
        Assert.That(AstronomicalDateValidation.ValidateTime(time), Is.False);
    }

    [Test]
    public void TestValidateTimeInvalidHourNegative()
    {
        var time = new AstronomicalTime(-1, 0, 0);
        Assert.That(AstronomicalDateValidation.ValidateTime(time), Is.False);
    }

    [Test]
    public void TestValidateTimeInvalidMinute60()
    {
        var time = new AstronomicalTime(12, 60, 0);
        Assert.That(AstronomicalDateValidation.ValidateTime(time), Is.False);
    }

    [Test]
    public void TestValidateTimeInvalidMinuteNegative()
    {
        var time = new AstronomicalTime(12, -1, 0);
        Assert.That(AstronomicalDateValidation.ValidateTime(time), Is.False);
    }

    [Test]
    public void TestValidateTimeInvalidSecond60()
    {
        var time = new AstronomicalTime(12, 30, 60);
        Assert.That(AstronomicalDateValidation.ValidateTime(time), Is.False);
    }

    [Test]
    public void TestValidateTimeInvalidSecondNegative()
    {
        var time = new AstronomicalTime(12, 30, -1);
        Assert.That(AstronomicalDateValidation.ValidateTime(time), Is.False);
    }

    [Test]
    public void TestValidateTimeMultipleInvalid()
    {
        var time = new AstronomicalTime(25, 70, 100);
        Assert.That(AstronomicalDateValidation.ValidateTime(time), Is.False);
    }

    [Test]
    public void TestValidateTimeBoundaryHour0()
    {
        var time = new AstronomicalTime(0, 0, 0);
        Assert.That(AstronomicalDateValidation.ValidateTime(time), Is.True);
    }

    [Test]
    public void TestValidateTimeBoundaryHour23()
    {
        var time = new AstronomicalTime(23, 59, 59);
        Assert.That(AstronomicalDateValidation.ValidateTime(time), Is.True);
    }

    [Test]
    public void TestValidateTimeBoundaryMinute0()
    {
        var time = new AstronomicalTime(12, 0, 0);
        Assert.That(AstronomicalDateValidation.ValidateTime(time), Is.True);
    }

    [Test]
    public void TestValidateTimeBoundaryMinute59()
    {
        var time = new AstronomicalTime(12, 59, 0);
        Assert.That(AstronomicalDateValidation.ValidateTime(time), Is.True);
    }

    [Test]
    public void TestValidateTimeBoundarySecond0()
    {
        var time = new AstronomicalTime(12, 30, 0);
        Assert.That(AstronomicalDateValidation.ValidateTime(time), Is.True);
    }

    [Test]
    public void TestValidateTimeBoundarySecond59()
    {
        var time = new AstronomicalTime(12, 30, 59);
        Assert.That(AstronomicalDateValidation.ValidateTime(time), Is.True);
    }

    [Test]
    public void TestValidateTimeFromDecimalHour()
    {
        var time = new AstronomicalTime(14.5125);
        // 14.5125 = 14:30:45
        Assert.That(AstronomicalDateValidation.ValidateTime(time), Is.True);
    }
}

