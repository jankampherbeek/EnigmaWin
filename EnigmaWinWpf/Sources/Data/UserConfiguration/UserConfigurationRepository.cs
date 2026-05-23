// UserConfigurationRepository.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
using EnigmaWin.Sources.Data.Db;
using EnigmaWin.Sources.Features.Config;

namespace EnigmaWin.Sources.Data.UserConfiguration;

public sealed class UserConfigurationRepository(IDbConnectionFactory factory) : IUserConfigurationRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // ── Public API ──────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<Features.Config.UserConfiguration>> FetchAllAsync()
    {
        using var conn = factory.Create();
        var rows = await conn.QueryAsync<UserConfigurationRow>(
            "SELECT * FROM UserConfiguration ORDER BY Name");
        return rows.Select(ToDomain).ToList();
    }

    public async Task<Features.Config.UserConfiguration?> FetchActiveAsync()
    {
        using var conn = factory.Create();
        var row = await conn.QuerySingleOrDefaultAsync<UserConfigurationRow>(
            "SELECT * FROM UserConfiguration WHERE IsActive = 1");
        return row is null ? null : ToDomain(row);
    }

    public async Task<Features.Config.UserConfiguration> AddAsync(
        string name,
        Features.Config.UserConfiguration? copyFrom = null)
    {
        var config = new Features.Config.UserConfiguration
        {
            Id                 = Guid.NewGuid(),
            Name               = name,
            IsActive           = false,
            Language           = copyFrom?.Language           ?? string.Empty,
            CalculationConfig  = copyFrom?.CalculationConfig  ?? CalculationConfig.Default,
            DisplayConfig      = copyFrom?.DisplayConfig      ?? DisplayConfig.Default,
            GlyphsConfig       = copyFrom?.GlyphsConfig       ?? GlyphsConfig.Default,
            FactorConfig       = copyFrom?.FactorConfig       ?? FactorConfig.Default,
            AspectConfig       = copyFrom?.AspectConfig       ?? AspectConfig.Default,
            OrbConfig          = copyFrom?.OrbConfig          ?? OrbConfig.Default,
            ProgressionsConfig = copyFrom?.ProgressionsConfig ?? ProgressionsConfig.Default
        };

        using var conn = factory.Create();
        using var tx = conn.BeginTransaction();

        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM UserConfiguration", transaction: tx);

        if (count == 0)
            config.IsActive = true;

        await conn.ExecuteAsync(InsertSql, ToRow(config), tx);
        tx.Commit();
        return config;
    }

    public async Task UpdateAsync(Features.Config.UserConfiguration config)
    {
        using var conn = factory.Create();
        await conn.ExecuteAsync(UpdateSql, ToRow(config));
    }

    public async Task SetActiveAsync(Guid id)
    {
        using var conn = factory.Create();
        using var tx = conn.BeginTransaction();
        await conn.ExecuteAsync(
            "UPDATE UserConfiguration SET IsActive = 0", transaction: tx);
        await conn.ExecuteAsync(
            "UPDATE UserConfiguration SET IsActive = 1 WHERE Id = @Id",
            new { Id = id.ToString() }, tx);
        tx.Commit();
    }

    public async Task DeleteAsync(Guid id)
    {
        using var conn = factory.Create();

        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM UserConfiguration");
        if (count <= 1)
            throw new InvalidOperationException(
                "Cannot delete the only remaining configuration.");

        var isActive = await conn.ExecuteScalarAsync<int>(
            "SELECT IsActive FROM UserConfiguration WHERE Id = @Id",
            new { Id = id.ToString() });
        if (isActive == 1)
            throw new InvalidOperationException(
                "Cannot delete the active configuration.");

        await conn.ExecuteAsync(
            "DELETE FROM UserConfiguration WHERE Id = @Id",
            new { Id = id.ToString() });
    }

    // ── SQL strings ─────────────────────────────────────────────────────────

    private const string InsertSql = """
        INSERT INTO UserConfiguration
            (Id, Name, IsActive, Language,
             CalculationConfigJson, DisplayConfigJson, GlyphsConfigJson,
             FactorConfigJson, AspectConfigJson, OrbConfigJson, ProgressionsConfigJson)
        VALUES
            (@Id, @Name, @IsActive, @Language,
             @CalculationConfigJson, @DisplayConfigJson, @GlyphsConfigJson,
             @FactorConfigJson, @AspectConfigJson, @OrbConfigJson, @ProgressionsConfigJson)
        """;

    private const string UpdateSql = """
        UPDATE UserConfiguration SET
            Name                  = @Name,
            IsActive              = @IsActive,
            Language              = @Language,
            CalculationConfigJson = @CalculationConfigJson,
            DisplayConfigJson     = @DisplayConfigJson,
            GlyphsConfigJson      = @GlyphsConfigJson,
            FactorConfigJson      = @FactorConfigJson,
            AspectConfigJson      = @AspectConfigJson,
            OrbConfigJson         = @OrbConfigJson,
            ProgressionsConfigJson = @ProgressionsConfigJson
        WHERE Id = @Id
        """;

    // ── Mapping ──────────────────────────────────────────────────────────────

    private Features.Config.UserConfiguration ToDomain(UserConfigurationRow row) => new()
    {
        Id                 = Guid.Parse(row.Id),
        Name               = row.Name,
        IsActive           = row.IsActive == 1,
        Language           = row.Language,
        CalculationConfig  = Deserialize(row.CalculationConfigJson,  CalculationConfig.Default),
        DisplayConfig      = Deserialize(row.DisplayConfigJson,      DisplayConfig.Default),
        GlyphsConfig       = Deserialize(row.GlyphsConfigJson,       GlyphsConfig.Default),
        FactorConfig       = MergeFactorConfig(Deserialize(row.FactorConfigJson, FactorConfig.Default)),
        AspectConfig       = MergeAspectConfig(Deserialize(row.AspectConfigJson, AspectConfig.Default)),
        OrbConfig          = MergeOrbConfig(Deserialize(row.OrbConfigJson, OrbConfig.Default)),
        ProgressionsConfig = Deserialize(row.ProgressionsConfigJson, ProgressionsConfig.Default)
    };

    /// <summary>
    /// Replaces any zero-value midpoint dial orbs with the current defaults.
    /// This handles existing database rows that were saved before the three dial orbs were introduced.
    /// </summary>
    private static Features.Config.OrbConfig MergeOrbConfig(Features.Config.OrbConfig saved)
    {
        var def = OrbConfig.Default;
        return saved with
        {
            Midpoint360DialOrb = saved.Midpoint360DialOrb == 0.0 ? def.Midpoint360DialOrb : saved.Midpoint360DialOrb,
            Midpoint90DialOrb  = saved.Midpoint90DialOrb  == 0.0 ? def.Midpoint90DialOrb  : saved.Midpoint90DialOrb,
            Midpoint45DialOrb  = saved.Midpoint45DialOrb  == 0.0 ? def.Midpoint45DialOrb  : saved.Midpoint45DialOrb
        };
    }

    /// <summary>
    /// Ensures that all factors present in the current defaults are included.
    /// Factors missing from the saved config get the default settings.
    /// </summary>
    private static Features.Config.FactorConfig MergeFactorConfig(Features.Config.FactorConfig saved)
    {
        var savedMap = saved.Settings.ToDictionary(s => s.Factor);
        var merged = FactorConfig.DefaultSettings
            .Select(def => savedMap.TryGetValue(def.Factor, out var existing) ? existing : def)
            .ToList();
        return new Features.Config.FactorConfig(merged);
    }

    /// <summary>
    /// Ensures that all aspects present in the current defaults are included.
    /// Aspects missing from the saved config get the default settings.
    /// </summary>
    private static Features.Config.AspectConfig MergeAspectConfig(Features.Config.AspectConfig saved)
    {
        var savedMap = saved.Settings.ToDictionary(s => s.Aspect);
        var merged = AspectConfig.DefaultSettings
            .Select(def => savedMap.TryGetValue(def.Aspect, out var existing) ? existing : def)
            .ToList();
        return new Features.Config.AspectConfig(merged);
    }

    private static UserConfigurationRow ToRow(Features.Config.UserConfiguration c) => new()
    {
        Id                    = c.Id.ToString(),
        Name                  = c.Name,
        IsActive              = c.IsActive ? 1 : 0,
        Language              = c.Language,
        CalculationConfigJson = Serialize(c.CalculationConfig),
        DisplayConfigJson     = Serialize(c.DisplayConfig),
        GlyphsConfigJson      = Serialize(c.GlyphsConfig),
        FactorConfigJson      = Serialize(c.FactorConfig),
        AspectConfigJson      = Serialize(c.AspectConfig),
        OrbConfigJson         = Serialize(c.OrbConfig),
        ProgressionsConfigJson = Serialize(c.ProgressionsConfig)
    };

    private static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, JsonOptions);

    private T Deserialize<T>(string json, T fallback)
    {
        if (string.IsNullOrWhiteSpace(json)) return fallback;
        try   { return JsonSerializer.Deserialize<T>(json, JsonOptions) ?? fallback; }
        catch { return fallback; }
    }
}

/// <summary>Dapper DTO for the UserConfiguration table.</summary>
internal sealed class UserConfigurationRow
{
    public string Id                    { get; set; } = string.Empty;
    public string Name                  { get; set; } = string.Empty;
    public int    IsActive              { get; set; }
    public string Language              { get; set; } = string.Empty;
    public string CalculationConfigJson { get; set; } = string.Empty;
    public string DisplayConfigJson     { get; set; } = string.Empty;
    public string GlyphsConfigJson      { get; set; } = string.Empty;
    public string FactorConfigJson      { get; set; } = string.Empty;
    public string AspectConfigJson      { get; set; } = string.Empty;
    public string OrbConfigJson         { get; set; } = string.Empty;
    public string ProgressionsConfigJson { get; set; } = string.Empty;
}
