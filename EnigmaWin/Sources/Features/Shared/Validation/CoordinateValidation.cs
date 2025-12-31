namespace EnigmaWin.Sources.Features.Shared.Validation;

/// <summary>Validate geographic coordinates (latitude and longitude).</summary>
public static class CoordinateValidation
{
    /// <summary>Validates a longitude value.</summary>
    /// <param name="value">The longitude value to validate</param>
    /// <returns>True if the value is between -180.0 and 180.0 (inclusive), false otherwise</returns>
    public static bool ValidateLongitude(double value)
    {
        return value is >= -180.0 and <= 180.0;
    }
    
    /// <summary>Validates a latitude value.</summary>
    /// <param name="value">The latitude value to validate</param>
    /// <returns>True if the value is between -90.0 and 90.0 (exclusive), false otherwise</returns>
    public static bool ValidateLatitude(double value)
    {
        return value is > -90.0 and < 90.0;
    }
}

