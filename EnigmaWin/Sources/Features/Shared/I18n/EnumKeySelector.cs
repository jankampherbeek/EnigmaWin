using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Shared.I18n;

/// <summary>Central access point for enum localization keys.</summary>
public static class EnumKeySelector
{
    public static string Key(CalendarStyle style)           => CalendarStyleKeys.Key(style);
    public static string Key(YearCount count)               => YearCountKeys.Key(count);
    public static string Key(DSTOption option)              => DSTOptionKeys.Key(option);
    public static string Key(UTOffsetDirection direction)   => UTOffsetDirectionKeys.Key(direction);
    public static string Key(LatitudeHemisphere hemisphere) => LatitudeHemisphereKeys.Key(hemisphere);
    public static string Key(LongitudeHemisphere hemisphere)=> LongitudeHemisphereKeys.Key(hemisphere);
    public static string Key(RoddenRating rating)           => RoddenRatingKeys.Key(rating);
}
