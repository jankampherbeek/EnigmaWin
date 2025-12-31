using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Shared.Conversion;

namespace EnigmaWintest.Features.Shared.Conversion;

/// <summary>Tests for PositionInDegreesConversion utilities.</summary>
[TestFixture]
public class PositionInDegreesConversionTests
{

    
    [Test]
    public void TestDoubleToDmsPositive()
    {
        var result = PositionInDegreesConversion.DoubleToDms(45.5125);
        Assert.That(result, Is.EqualTo("45°30'45\""));
    }
    
    [Test]
    public void TestDoubleToDmsNegative()
    {
        var result = PositionInDegreesConversion.DoubleToDms(-45.5125);
        Assert.That(result, Is.EqualTo("-45°30'45\""));
    }
    
    [Test]
    public void TestDoubleToDmsZero()
    {
        var result = PositionInDegreesConversion.DoubleToDms(0.0);
        Assert.That(result, Is.EqualTo("0°00'00\""));
    }
    
    [Test]
    public void TestDoubleToDmsZeroPadding()
    {
        var result = PositionInDegreesConversion.DoubleToDms(12.0505);
        // 12.0505 * 3600 = 43381.8 → truncated to 43381 seconds = 12°03'01"
        Assert.That(result, Is.EqualTo("12°03'01\""));
    }
    
    [Test]
    public void TestDoubleToDmsExactDegrees()
    {
        var result = PositionInDegreesConversion.DoubleToDms(90.0);
        Assert.That(result, Is.EqualTo("90°00'00\""));
    }
    
    [Test]
    public void TestDoubleToDmsOnlyMinutes()
    {
        var result = PositionInDegreesConversion.DoubleToDms(30.5);
        Assert.That(result, Is.EqualTo("30°30'00\""));
    }
    
    [Test]
    public void TestDoubleToDmsLargeValue()
    {
        var result = PositionInDegreesConversion.DoubleToDms(359.999722);
        // 359.999722 * 3600 = 1295998.9992 → truncated to 1295998 seconds = 359°59'58"
        Assert.That(result, Is.EqualTo("359°59'58\""));
    }
    
    [Test]
    public void TestDoubleToDmsSignAries()
    {
        var (dms, sign, success) = PositionInDegreesConversion.DoubleToDmsSign(15.51251);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.True);
            Assert.That(dms, Is.EqualTo("15°30'45\""));
            Assert.That(sign, Is.EqualTo(Signs.Aries));
        }
    }
    
    [Test]
    public void TestDoubleToDmsSignTaurus()
    {
        var (dms, sign, success) = PositionInDegreesConversion.DoubleToDmsSign(45.51251);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.True);
            Assert.That(dms, Is.EqualTo("15°30'45\""));
            Assert.That(sign, Is.EqualTo(Signs.Taurus));
        }
    }
    
    [Test]
    public void TestDoubleToDmsSignGemini()
    {
        var (dms, sign, success) = PositionInDegreesConversion.DoubleToDmsSign(75.51251);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.True);
            Assert.That(dms, Is.EqualTo("15°30'45\""));
            Assert.That(sign, Is.EqualTo(Signs.Gemini));
        }
    }
    
    [Test]
    public void TestDoubleToDmsSignCancer()
    {
        var (dms, sign, success) = PositionInDegreesConversion.DoubleToDmsSign(105.51251);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.True);
            Assert.That(dms, Is.EqualTo("15°30'45\""));
            Assert.That(sign, Is.EqualTo(Signs.Cancer));
        }
    }
    
    [Test]
    public void TestDoubleToDmsSignLeo()
    {
        var (dms, sign, success) = PositionInDegreesConversion.DoubleToDmsSign(135.51251);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.True);
            Assert.That(dms, Is.EqualTo("15°30'45\""));
            Assert.That(sign, Is.EqualTo(Signs.Leo));
        }
    }
    
    [Test]
    public void TestDoubleToDmsSignVirgo()
    {
        var (dms, sign, success) = PositionInDegreesConversion.DoubleToDmsSign(165.51251);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.True);
            Assert.That(dms, Is.EqualTo("15°30'45\""));
            Assert.That(sign, Is.EqualTo(Signs.Virgo));
        }
    }
    
    [Test]
    public void TestDoubleToDmsSignLibra()
    {
        var (dms, sign, success) = PositionInDegreesConversion.DoubleToDmsSign(195.51251);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.True);
            Assert.That(dms, Is.EqualTo("15°30'45\""));
            Assert.That(sign, Is.EqualTo(Signs.Libra));
        }
    }
    
    [Test]
    public void TestDoubleToDmsSignScorpio()
    {
        var (dms, sign, success) = PositionInDegreesConversion.DoubleToDmsSign(225.51251);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.True);
            Assert.That(dms, Is.EqualTo("15°30'45\""));
            Assert.That(sign, Is.EqualTo(Signs.Scorpio));
        }
    }
    
    [Test]
    public void TestDoubleToDmsSignSagittarius()
    {
        var (dms, sign, success) = PositionInDegreesConversion.DoubleToDmsSign(255.51251);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.True);
            Assert.That(dms, Is.EqualTo("15°30'45\""));
            Assert.That(sign, Is.EqualTo(Signs.Sagittarius));
        }
    }
    
    [Test]
    public void TestDoubleToDmsSignCapricorn()
    {
        var (dms, sign, success) = PositionInDegreesConversion.DoubleToDmsSign(285.51251);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.True);
            Assert.That(dms, Is.EqualTo("15°30'45\""));
            Assert.That(sign, Is.EqualTo(Signs.Capricorn));
        }
    }
    
    [Test]
    public void TestDoubleToDmsSignAquarius()
    {
        var (dms, sign, success) = PositionInDegreesConversion.DoubleToDmsSign(315.51251);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.True);
            Assert.That(dms, Is.EqualTo("15°30'45\""));
            Assert.That(sign, Is.EqualTo(Signs.Aquarius));
        }
    }
    
    [Test]
    public void TestDoubleToDmsSignPisces()
    {
        var (dms, sign, success) = PositionInDegreesConversion.DoubleToDmsSign(345.51251);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.True);
            Assert.That(dms, Is.EqualTo("15°30'45\""));
            Assert.That(sign, Is.EqualTo(Signs.Pisces));
        }
    }
    
    [Test]
    public void TestDoubleToDmsSignBoundaryZero()
    {
        var (dms, sign, success) = PositionInDegreesConversion.DoubleToDmsSign(0.0);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.True);
            Assert.That(dms, Is.EqualTo("0°00'00\""));
            Assert.That(sign, Is.EqualTo(Signs.Aries));
        }
    }
    
    [Test]
    public void TestDoubleToDmsSignBoundaryThirty()
    {
        var (dms, sign, success) = PositionInDegreesConversion.DoubleToDmsSign(30.0);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.True);
            Assert.That(dms, Is.EqualTo("0°00'00\""));
            Assert.That(sign, Is.EqualTo(Signs.Taurus));
        }
    }
    
    [Test]
    public void TestDoubleToDmsSignJustBeforeThirty()
    {
        var (_, sign, success) = PositionInDegreesConversion.DoubleToDmsSign(29.9999);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.True);
            Assert.That(sign, Is.EqualTo(Signs.Aries));
        }
    }
    
    [Test]
    public void TestDoubleToDmsSignBoundary359()
    {
        var (dms, sign, success) = PositionInDegreesConversion.DoubleToDmsSign(359.999);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.True);
            // 359.999 % 30 = 29.999, 29.999 * 3600 = 107996.4 → truncated to 107996 seconds = 29°59'56"
            Assert.That(dms, Is.EqualTo("29°59'56\""));
            Assert.That(sign, Is.EqualTo(Signs.Pisces));
        }
    }
    
    [Test]
    public void TestDoubleToDmsSignError360()
    {
        var (dms, sign, success) = PositionInDegreesConversion.DoubleToDmsSign(360.0);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.False);
            Assert.That(dms, Is.EqualTo(""));
            Assert.That(sign, Is.Null);
        }
    }
    
    [Test]
    public void TestDoubleToDmsSignErrorGreaterThan360()
    {
        var (dms, sign, success) = PositionInDegreesConversion.DoubleToDmsSign(450.0);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.False);
            Assert.That(dms, Is.EqualTo(""));
            Assert.That(sign, Is.Null);
        }
    }
    
    [Test]
    public void TestDoubleToDmsSignErrorNegative()
    {
        var (dms, sign, success) = PositionInDegreesConversion.DoubleToDmsSign(-15.0);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.False);
            Assert.That(dms, Is.EqualTo(""));
            Assert.That(sign, Is.Null);
        }
    }
    
    [Test]
    public void TestDoubleToDmsSignErrorSmallNegative()
    {
        var (dms, sign, success) = PositionInDegreesConversion.DoubleToDmsSign(-0.1);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.False);
            Assert.That(dms, Is.EqualTo(""));
            Assert.That(sign, Is.Null);
        }
    }
    
    [Test]
    public void TestDoubleToDmsSignErrorLargeNegative()
    {
        var (dms, sign, success) = PositionInDegreesConversion.DoubleToDmsSign(-100.0);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.False);
            Assert.That(dms, Is.EqualTo(""));
            Assert.That(sign, Is.Null);
        }
    }
    
    [Test]
    public void TestDoubleToDmsSignZeroPadding()
    {
        var (dms, sign, success) = PositionInDegreesConversion.DoubleToDmsSign(12.0505);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.True);
            // 12.0505 % 30 = 12.0505, 12.0505 * 3600 = 43381.8 → truncated to 43381 seconds = 12°03'01"
            Assert.That(dms, Is.EqualTo("12°03'01\""));
            Assert.That(sign, Is.EqualTo(Signs.Aries));
        }
    }
    
    // MARK: - DoubleToDegrees Tests
    
    [Test]
    public void TestDoubleToDegreesPositiveDefault()
    {
        var result = PositionInDegreesConversion.DoubleToDegrees(45.5125);
        Assert.That(result, Is.EqualTo("45.51°"));
    }
    
    [Test]
    public void TestDoubleToDegreesNegativeDefault()
    {
        var result = PositionInDegreesConversion.DoubleToDegrees(-45.5125);
        Assert.That(result, Is.EqualTo("-45.51°"));
    }
    
    [Test]
    public void TestDoubleToDegreesZero()
    {
        var result = PositionInDegreesConversion.DoubleToDegrees(0.0);
        Assert.That(result, Is.EqualTo("0.00°"));
    }
    
    [Test]
    public void TestDoubleToDegreesZeroDecimals()
    {
        var result = PositionInDegreesConversion.DoubleToDegrees(45.5125, 0);
        Assert.That(result, Is.EqualTo("46°"));
    }
    
    [Test]
    public void TestDoubleToDegreesOneDecimal()
    {
        var result = PositionInDegreesConversion.DoubleToDegrees(45.5125, 1);
        Assert.That(result, Is.EqualTo("45.5°"));
    }
    
    [Test]
    public void TestDoubleToDegreesThreeDecimals()
    {
        var result = PositionInDegreesConversion.DoubleToDegrees(45.5125, 3);
        Assert.That(result, Is.EqualTo("45.513°"));
    }
    
    [Test]
    public void TestDoubleToDegreesFiveDecimals()
    {
        var result = PositionInDegreesConversion.DoubleToDegrees(45.5125, 5);
        Assert.That(result, Is.EqualTo("45.51250°"));
    }
    
    [Test]
    public void TestDoubleToDegreesRounding()
    {
        var result = PositionInDegreesConversion.DoubleToDegrees(45.9999, 2);
        Assert.That(result, Is.EqualTo("46.00°"));
    }
    
    [Test]
    public void TestDoubleToDegreesLargeValue()
    {
        var result = PositionInDegreesConversion.DoubleToDegrees(359.999, 2);
        Assert.That(result, Is.EqualTo("360.00°"));
    }
}

