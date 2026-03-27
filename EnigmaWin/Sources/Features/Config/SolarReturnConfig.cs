// SolarReturnConfig.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Config;

public enum SolarMethods
{
    TropicalParallax    = 0,
    TropicalNoParallax  = 1,
    SiderealParallax    = 2,
    SiderealNoParallax  = 3
}

public enum SolarLocations
{
    Birth              = 0,
    RelocateBirthDay   = 1,
    RelocateResidence  = 2
}

public static class SolarMethodsExtensions
{
    public static string LocalizedName(this SolarMethods m) => m switch
    {
        SolarMethods.TropicalParallax   => "enum.solarmethod.tropicalparallax",
        SolarMethods.TropicalNoParallax => "enum.solarmethod.tropicalnoparallax",
        SolarMethods.SiderealParallax   => "enum.solarmethod.siderealparallax",
        SolarMethods.SiderealNoParallax => "enum.solarmethod.siderealnoparallax",
        _                               => string.Empty
    };
}

public static class SolarLocationsExtensions
{
    public static string LocalizedName(this SolarLocations l) => l switch
    {
        SolarLocations.Birth             => "enum.solarlocation.birth",
        SolarLocations.RelocateBirthDay  => "enum.solarlocation.relocatebirthday",
        SolarLocations.RelocateResidence => "enum.solarlocation.relocateresidence",
        _                                => string.Empty
    };
}

/// <summary>Configuration for solar returns.</summary>
public readonly record struct SolarReturnConfig(
    double Orb,
    SolarMethods Method,
    SolarLocations Location)
{
    public static SolarReturnConfig Default => new(
        Orb: 1.0,
        SolarMethods.TropicalParallax,
        SolarLocations.Birth);
}
