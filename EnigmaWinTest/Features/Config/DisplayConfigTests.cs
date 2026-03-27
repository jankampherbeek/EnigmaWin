// DisplayConfigTests.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWintest.Features.Config;

[TestFixture]
public class DrawingTypesTests
{
    [Test]
    public void SignBased_HasRawValue0()
    {
        Assert.That((int)DrawingTypes.SignBased, Is.EqualTo(0));
    }

    [Test]
    public void HouseBased_HasRawValue1()
    {
        Assert.That((int)DrawingTypes.HouseBased, Is.EqualTo(1));
    }

    [Test]
    public void French_HasRawValue2()
    {
        Assert.That((int)DrawingTypes.French, Is.EqualTo(2));
    }

    [Test]
    public void Ring_HasRawValue3()
    {
        Assert.That((int)DrawingTypes.Ring, Is.EqualTo(3));
    }

    [Test]
    public void Dial360_HasRawValue4()
    {
        Assert.That((int)DrawingTypes.Dial360, Is.EqualTo(4));
    }

    [Test]
    public void Dial90_HasRawValue5()
    {
        Assert.That((int)DrawingTypes.Dial90, Is.EqualTo(5));
    }

    [Test]
    public void Dial45_HasRawValue6()
    {
        Assert.That((int)DrawingTypes.Dial45, Is.EqualTo(6));
    }

    [Test]
    public void AllValues_CountIsSeven()
    {
        Assert.That(Enum.GetValues<DrawingTypes>().Length, Is.EqualTo(7));
    }

    [Test]
    public void Ring_LocalizedName_IsCorrectKey()
    {
        Assert.That(DrawingTypes.Ring.LocalizedName(), Is.EqualTo("enum.drawingtype.ring"));
    }

    [Test]
    public void Dial360_LocalizedName_IsCorrectKey()
    {
        Assert.That(DrawingTypes.Dial360.LocalizedName(), Is.EqualTo("enum.drawingtype.dial360"));
    }

    [Test]
    public void Dial90_LocalizedName_IsCorrectKey()
    {
        Assert.That(DrawingTypes.Dial90.LocalizedName(), Is.EqualTo("enum.drawingtype.dial90"));
    }

    [Test]
    public void Dial45_LocalizedName_IsCorrectKey()
    {
        Assert.That(DrawingTypes.Dial45.LocalizedName(), Is.EqualTo("enum.drawingtype.dial45"));
    }

    [Test]
    public void SignBased_LocalizedName_IsCorrectKey()
    {
        Assert.That(DrawingTypes.SignBased.LocalizedName(), Is.EqualTo("enum.drawingtype.signbased"));
    }

    [Test]
    public void HouseBased_LocalizedName_IsCorrectKey()
    {
        Assert.That(DrawingTypes.HouseBased.LocalizedName(), Is.EqualTo("enum.drawingtype.housebased"));
    }
}

[TestFixture]
public class DisplayConfigTests
{
    [Test]
    public void Default_DrawingType_IsSignBased()
    {
        Assert.That(DisplayConfig.Default.DrawingType, Is.EqualTo(DrawingTypes.SignBased));
    }

    [Test]
    public void Default_SignColors_IsEmpty()
    {
        Assert.That(DisplayConfig.Default.SignColors, Is.Empty);
    }

    [Test]
    public void Init_WithDrawingTypeAndColors_StoresCorrectly()
    {
        var color    = new ColorConfig(0.5, 0.5, 0.5);
        var override_ = new SignColorOverride(Signs.Aries, color);
        var config   = new DisplayConfig(DrawingTypes.Ring, [override_]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(config.DrawingType,      Is.EqualTo(DrawingTypes.Ring));
            Assert.That(config.SignColors.Count, Is.EqualTo(1));
            Assert.That(config.SignColors[0],    Is.EqualTo(override_));
        }
    }

    [Test]
    [TestCase(DrawingTypes.SignBased)]
    [TestCase(DrawingTypes.HouseBased)]
    [TestCase(DrawingTypes.French)]
    [TestCase(DrawingTypes.Ring)]
    [TestCase(DrawingTypes.Dial360)]
    [TestCase(DrawingTypes.Dial90)]
    [TestCase(DrawingTypes.Dial45)]
    public void AllDrawingTypes_AreAccepted(DrawingTypes type)
    {
        var config = new DisplayConfig(type, []);
        Assert.That(config.DrawingType, Is.EqualTo(type));
    }
}

[TestFixture]
public class SignColorOverrideTests
{
    [Test]
    public void Init_StoresSignAndColor()
    {
        var color    = new ColorConfig(0.1, 0.2, 0.3);
        var override_ = new SignColorOverride(Signs.Taurus, color);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(override_.Sign,  Is.EqualTo(Signs.Taurus));
            Assert.That(override_.Color, Is.EqualTo(color));
        }
    }
}

[TestFixture]
public class ColorConfigTests
{
    [Test]
    public void Init_StoresRgbValues()
    {
        var color = new ColorConfig(0.2, 0.4, 0.6);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(color.Red,   Is.EqualTo(0.2));
            Assert.That(color.Green, Is.EqualTo(0.4));
            Assert.That(color.Blue,  Is.EqualTo(0.6));
        }
    }

    [Test]
    public void Default_Opacity_IsOne()
    {
        var color = new ColorConfig(0.0, 0.0, 0.0);
        Assert.That(color.Opacity, Is.EqualTo(1.0));
    }

    [Test]
    public void Init_WithExplicitOpacity_StoresValue()
    {
        var color = new ColorConfig(0.0, 0.0, 0.0, 0.5);
        Assert.That(color.Opacity, Is.EqualTo(0.5));
    }

    [Test]
    public void DefaultSignColor_HasExpectedValues()
    {
        var color = ColorConfig.DefaultSignColor;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(color.Red,     Is.EqualTo(0.678));
            Assert.That(color.Green,   Is.EqualTo(0.847));
            Assert.That(color.Blue,    Is.EqualTo(0.902));
            Assert.That(color.Opacity, Is.EqualTo(1.0));
        }
    }
}
