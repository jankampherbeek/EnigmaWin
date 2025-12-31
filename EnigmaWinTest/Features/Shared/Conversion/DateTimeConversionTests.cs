using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Conversion;

namespace EnigmaWintest.Features.Shared.Conversion;

/// <summary>Tests for DateTimeConversion utilities.</summary>
[TestFixture]
public class DateTimeConversionTests
{

    [Test]
    public void TestDateToTextGregorianSingleDigits()
    {
        var date = new AstronomicalDate(2025, 1, 5, true);
        var result = DateTimeConversion.DateToText(date);
        Assert.That(result, Is.EqualTo("2025/01/05 G"));
    }

    [Test]
    public void TestDateToTextGregorianDoubleDigits()
    {
        var date = new AstronomicalDate(2025, 12, 31, true);
        var result = DateTimeConversion.DateToText(date);
        Assert.That(result, Is.EqualTo("2025/12/31 G"));
    }

    [Test]
    public void TestDateToTextJulian()
    {
        var date = new AstronomicalDate(2025, 1, 15, false);
        var result = DateTimeConversion.DateToText(date);
        Assert.That(result, Is.EqualTo("2025/01/15 J"));
    }

    [Test]
    public void TestDateToTextFirstDayOfYear()
    {
        var date = new AstronomicalDate(2025, 1, 1, true);
        var result = DateTimeConversion.DateToText(date);
        Assert.That(result, Is.EqualTo("2025/01/01 G"));
    }

    [Test]
    public void TestDateToTextLastDayOfYear()
    {
        var date = new AstronomicalDate(2025, 12, 31, true);
        var result = DateTimeConversion.DateToText(date);
        Assert.That(result, Is.EqualTo("2025/12/31 G"));
    }

    [Test]
    public void TestDateToTextLeapYear()
    {
        var date = new AstronomicalDate(2024, 2, 29, true);
        var result = DateTimeConversion.DateToText(date);
        Assert.That(result, Is.EqualTo("2024/02/29 G"));
    }

    [Test]
    public void TestDateToTextMonth10Day10()
    {
        var date = new AstronomicalDate(2025, 10, 10, true);
        var result = DateTimeConversion.DateToText(date);
        Assert.That(result, Is.EqualTo("2025/10/10 G"));
    }

    [Test]
    public void TestDateToTextJulianSingleDigits()
    {
        var date = new AstronomicalDate(2025, 3, 7, false);
        var result = DateTimeConversion.DateToText(date);
        Assert.That(result, Is.EqualTo("2025/03/07 J"));
    }

    [Test]
    public void TestDateToTextYearZero()
    {
        var date = new AstronomicalDate(0, 1, 1, true);
        var result = DateTimeConversion.DateToText(date);
        Assert.That(result, Is.EqualTo("0/01/01 G"));
    }

    [Test]
    public void TestDateToTextNegativeYear()
    {
        var date = new AstronomicalDate(-1, 1, 1, true);
        var result = DateTimeConversion.DateToText(date);
        Assert.That(result, Is.EqualTo("-1/01/01 G"));
    }

    [Test]
    public void TestTimeToTextSingleDigits()
    {
        var time = new AstronomicalTime(14, 5, 3);
        var result = DateTimeConversion.TimeToText(time);
        Assert.That(result, Is.EqualTo("14:05:03"));
    }

    [Test]
    public void TestTimeToTextDoubleDigits()
    {
        var time = new AstronomicalTime(14, 30, 45);
        var result = DateTimeConversion.TimeToText(time);
        Assert.That(result, Is.EqualTo("14:30:45"));
    }

    [Test]
    public void TestTimeToTextMidnight()
    {
        var time = new AstronomicalTime(0, 0, 0);
        var result = DateTimeConversion.TimeToText(time);
        Assert.That(result, Is.EqualTo("00:00:00"));
    }

    [Test]
    public void TestTimeToTextNoon()
    {
        var time = new AstronomicalTime(12, 0, 0);
        var result = DateTimeConversion.TimeToText(time);
        Assert.That(result, Is.EqualTo("12:00:00"));
    }

    [Test]
    public void TestTimeToTextEndOfDay()
    {
        var time = new AstronomicalTime(23, 59, 59);
        var result = DateTimeConversion.TimeToText(time);
        Assert.That(result, Is.EqualTo("23:59:59"));
    }

    [Test]
    public void TestTimeToTextSingleDigitHour()
    {
        var time = new AstronomicalTime(5, 30, 15);
        var result = DateTimeConversion.TimeToText(time);
        Assert.That(result, Is.EqualTo("05:30:15"));
    }

    [Test]
    public void TestTimeToTextAllTens()
    {
        var time = new AstronomicalTime(10, 10, 10);
        var result = DateTimeConversion.TimeToText(time);
        Assert.That(result, Is.EqualTo("10:10:10"));
    }

    [Test]
    public void TestTimeToTextZeroMinutesSeconds()
    {
        var time = new AstronomicalTime(8, 0, 0);
        var result = DateTimeConversion.TimeToText(time);
        Assert.That(result, Is.EqualTo("08:00:00"));
    }

    [Test]
    public void TestTimeToTextZeroSeconds()
    {
        var time = new AstronomicalTime(15, 45, 0);
        var result = DateTimeConversion.TimeToText(time);
        Assert.That(result, Is.EqualTo("15:45:00"));
    }

    [Test]
    public void TestTimeToTextFromDecimalHour()
    {
        var time = new AstronomicalTime(14.5125);
        // 14.5125 = 14:30:45
        var result = DateTimeConversion.TimeToText(time);
        Assert.That(result, Is.EqualTo("14:30:45"));
    }

    // MARK: - DateTimeToText Tests

    [Test]
    public void TestDateTimeToTextGregorian()
    {
        var date = new AstronomicalDate(2025, 1, 15, true);
        var time = new AstronomicalTime(14, 30, 45);
        var dateTime = new AstronomicalDateTime(date, time);
        var result = DateTimeConversion.DateTimeToText(dateTime);
        Assert.That(result, Is.EqualTo("2025/01/15 G 14:30:45"));
    }

    [Test]
    public void TestDateTimeToTextJulian()
    {
        var date = new AstronomicalDate(2025, 1, 15, false);
        var time = new AstronomicalTime(14, 30, 45);
        var dateTime = new AstronomicalDateTime(date, time);
        var result = DateTimeConversion.DateTimeToText(dateTime);
        Assert.That(result, Is.EqualTo("2025/01/15 J 14:30:45"));
    }

    [Test]
    public void TestDateTimeToTextSingleDigits()
    {
        var date = new AstronomicalDate(2025, 3, 5, true);
        var time = new AstronomicalTime(5, 3, 7);
        var dateTime = new AstronomicalDateTime(date, time);
        var result = DateTimeConversion.DateTimeToText(dateTime);
        Assert.That(result, Is.EqualTo("2025/03/05 G 05:03:07"));
    }

    [Test]
    public void TestDateTimeToTextMidnightFirstDay()
    {
        var date = new AstronomicalDate(2025, 1, 1, true);
        var time = new AstronomicalTime(0, 0, 0);
        var dateTime = new AstronomicalDateTime(date, time);
        var result = DateTimeConversion.DateTimeToText(dateTime);
        Assert.That(result, Is.EqualTo("2025/01/01 G 00:00:00"));
    }

    [Test]
    public void TestDateTimeToTextEndOfDayLastDay()
    {
        var date = new AstronomicalDate(2025, 12, 31, true);
        var time = new AstronomicalTime(23, 59, 59);
        var dateTime = new AstronomicalDateTime(date, time);
        var result = DateTimeConversion.DateTimeToText(dateTime);
        Assert.That(result, Is.EqualTo("2025/12/31 G 23:59:59"));
    }

    [Test]
    public void TestDateTimeToTextNoonLeapYear()
    {
        var date = new AstronomicalDate(2024, 2, 29, true);
        var time = new AstronomicalTime(12, 0, 0);
        var dateTime = new AstronomicalDateTime(date, time);
        var result = DateTimeConversion.DateTimeToText(dateTime);
        Assert.That(result, Is.EqualTo("2024/02/29 G 12:00:00"));
    }

    [Test]
    public void TestDateTimeToTextJulianWithDecimalTime()
    {
        var date = new AstronomicalDate(2025, 6, 15, false);
        var time = new AstronomicalTime(10.508333333333333);
        // 10.508333... = 10:30:30
        var dateTime = new AstronomicalDateTime(date, time);
        var result = DateTimeConversion.DateTimeToText(dateTime);
        Assert.That(result, Is.EqualTo("2025/06/15 J 10:30:30"));
    }

    [Test]
    public void TestDateTimeToTextAllTens()
    {
        var date = new AstronomicalDate(2010, 10, 10, true);
        var time = new AstronomicalTime(10, 10, 10);
        var dateTime = new AstronomicalDateTime(date, time);
        var result = DateTimeConversion.DateTimeToText(dateTime);
        Assert.That(result, Is.EqualTo("2010/10/10 G 10:10:10"));
    }
}

