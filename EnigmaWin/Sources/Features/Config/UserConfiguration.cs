// UserConfiguration.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;

namespace EnigmaWin.Sources.Features.Config;

/// <summary>A named user configuration. The active configuration is used for all calculations.</summary>
/// <remarks>
/// Sub-configs are stored as JSON strings in the database (step 2).
/// Use the <see cref="Default"/> factory method to create a configuration with sensible defaults.
/// </remarks>
public class UserConfiguration
{
    public Guid   Id          { get; set; } = Guid.NewGuid();
    public string Name        { get; set; } = string.Empty;
    public bool   IsActive    { get; set; }
    /// <summary>BCP-47 language code (e.g. "en", "nl"). Empty string means system locale.</summary>
    public string Language    { get; set; } = string.Empty;

    public CalculationConfig   CalculationConfig  { get; set; } = CalculationConfig.Default;
    public DisplayConfig       DisplayConfig      { get; set; } = DisplayConfig.Default;
    public GlyphsConfig        GlyphsConfig       { get; set; } = GlyphsConfig.Default;
    public FactorConfig        FactorConfig       { get; set; } = FactorConfig.Default;
    public AspectConfig        AspectConfig       { get; set; } = AspectConfig.Default;
    public OrbConfig           OrbConfig          { get; set; } = OrbConfig.Default;
    public ProgressionsConfig  ProgressionsConfig { get; set; } = ProgressionsConfig.Default;
    public FixStarConfig       FixStarConfig      { get; set; } = FixStarConfig.Default;

    /// <summary>Creates a new <see cref="UserConfiguration"/> with all defaults applied.</summary>
    public static UserConfiguration Default => new()
    {
        Name              = "Default",
        IsActive          = true,
        Language          = string.Empty,
        CalculationConfig = CalculationConfig.Default,
        DisplayConfig     = DisplayConfig.Default,
        GlyphsConfig      = GlyphsConfig.Default,
        FactorConfig      = FactorConfig.Default,
        AspectConfig      = AspectConfig.Default,
        OrbConfig         = OrbConfig.Default,
        ProgressionsConfig = ProgressionsConfig.Default,
        FixStarConfig      = FixStarConfig.Default
    };
}
