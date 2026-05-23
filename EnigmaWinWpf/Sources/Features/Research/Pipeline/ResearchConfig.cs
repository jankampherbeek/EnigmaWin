// ResearchConfig.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Research.Inquiries;
using InquiriesEnum = EnigmaWin.Sources.Features.Research.Inquiries.Inquiries;

namespace EnigmaWin.Sources.Features.Research.Pipeline;

/// <summary>Describes the layout of one factor's block inside a results.bin record.</summary>
public sealed class FactorLayout
{
    public Factors Factor { get; init; }
    /// <summary>Byte offset of this factor's block from the start of the record's data region.</summary>
    public int ByteOffset { get; init; }
    public bool HasEcliptical { get; init; }
    public bool HasEquatorial { get; init; }
    public bool HasHorizontal { get; init; }

    /// <summary>Number of coordinate systems × 5 doubles each.</summary>
    public int DoubleCount =>
        (HasEcliptical ? 1 : 0) + (HasEquatorial ? 1 : 0) + (HasHorizontal ? 1 : 0) is var systems
            ? systems * 5
            : 0;

    /// <summary>Byte size of this factor's block (each double is 8 bytes).</summary>
    public int ByteSize => DoubleCount * 8;
}

/// <summary>Decoded form of the JSON stored in ResearchProject.Config. Computes the binary record layout for results.bin.</summary>
public sealed class ResearchConfig
{
    // ── Stored (JSON) ────────────────────────────────────────────────────────

    [JsonPropertyName("enabledFactorIds")]
    public List<int> EnabledFactorIds { get; init; } = [];

    /// <summary>Raw value of the Inquiries enum.</summary>
    [JsonPropertyName("inquiryId")]
    public int InquiryId { get; init; }

    /// <summary>Raw value of HouseSystems. Only relevant when inquiry == FactorsInHouses.</summary>
    [JsonPropertyName("houseSystemId")]
    public int HouseSystemId { get; init; }

    /// <summary>CalculationConfig serialised as a JSON string.</summary>
    [JsonPropertyName("calculationConfigJson")]
    public string CalculationConfigJson { get; init; } = string.Empty;

    [JsonPropertyName("orbConfigJson")]
    public string? OrbConfigJson { get; init; }

    // ── Optional inquiry-specific settings ──────────────────────────────────

    [JsonPropertyName("enabledAspectIds")]
    public List<int>? EnabledAspectIds { get; init; }

    [JsonPropertyName("aspectOrbOverride")]
    public double? AspectOrbOverride { get; init; }

    [JsonPropertyName("unaspectOrbOverride")]
    public double? UnaspectOrbOverride { get; init; }

    [JsonPropertyName("declMidpointOrbOverride")]
    public double? DeclMidpointOrbOverride { get; init; }

    [JsonPropertyName("harmonicOrbOverride")]
    public double? HarmonicOrbOverride { get; init; }

    [JsonPropertyName("parallelOrbOverride")]
    public double? ParallelOrbOverride { get; init; }

    [JsonPropertyName("enabledDialSizes")]
    public List<int>? EnabledDialSizes { get; init; }

    [JsonPropertyName("harmonicNumber")]
    public int? HarmonicNumber { get; init; }

    // ── Derived coordinate flags ─────────────────────────────────────────────

    [JsonIgnore]
    private InquiriesEnum? Inquiry =>
        Enum.IsDefined(typeof(InquiriesEnum), InquiryId) ? (InquiriesEnum)InquiryId : null;

    [JsonIgnore]
    public bool UseEcliptical => Inquiry?.UsesEcliptical() ?? true;

    [JsonIgnore]
    public bool UseEquatorial => Inquiry?.UsesEquatorial() ?? false;

    [JsonIgnore]
    public bool UseHorizontal => false;

    [JsonIgnore]
    public HouseSystems HouseSystem =>
        Enum.IsDefined(typeof(HouseSystems), HouseSystemId) ? (HouseSystems)HouseSystemId : HouseSystems.Placidus;

    // ── Computed layout ──────────────────────────────────────────────────────

    [JsonIgnore]
    public List<Factors> EnabledFactors =>
        EnabledFactorIds.Select(id => (Factors?)((Enum.IsDefined(typeof(Factors), id) ? (Factors)id : (Factors?)null)))
                        .Where(f => f.HasValue)
                        .Select(f => f!.Value)
                        .ToList();

    /// <summary>Per-factor layout entries in stable enum-declaration order, filtered to enabled factors.</summary>
    [JsonIgnore]
    public List<FactorLayout> FactorLayouts
    {
        get
        {
            var enabled = new HashSet<Factors>(EnabledFactors);
            var offset = 0;
            var layouts = new List<FactorLayout>();
            foreach (var factor in Enum.GetValues<Factors>())
            {
                if (!enabled.Contains(factor)) continue;
                var layout = new FactorLayout
                {
                    Factor = factor,
                    ByteOffset = offset,
                    HasEcliptical = UseEcliptical,
                    HasEquatorial = UseEquatorial,
                    HasHorizontal = false
                };
                layouts.Add(layout);
                offset += layout.ByteSize;
            }
            return layouts;
        }
    }

    [JsonIgnore]
    public int DataRegionSize => FactorLayouts.Sum(l => l.ByteSize);

    /// <summary>Total bytes per record: 8 (recordId) + 1 (isData) + 7 (padding) + data region.</summary>
    [JsonIgnore]
    public int RecordSize => 16 + DataRegionSize;

    [JsonIgnore]
    public OrbConfig? OrbConfig
    {
        get
        {
            if (string.IsNullOrEmpty(OrbConfigJson)) return null;
            try { return JsonSerializer.Deserialize<OrbConfig>(OrbConfigJson); }
            catch { return null; }
        }
    }

    [JsonIgnore]
    public CalculationConfig? CalculationConfig
    {
        get
        {
            if (string.IsNullOrEmpty(CalculationConfigJson)) return null;
            try { return JsonSerializer.Deserialize<CalculationConfig>(CalculationConfigJson); }
            catch { return null; }
        }
    }

    // ── JSON helpers ─────────────────────────────────────────────────────────

    public static ResearchConfig FromJson(string json) =>
        JsonSerializer.Deserialize<ResearchConfig>(json)
            ?? throw new ResearchConfigException("Deserialisation returned null.");

    public string ToJson() => JsonSerializer.Serialize(this);
}

public sealed class ResearchConfigException(string message) : Exception(message);

/// <summary>Orb configuration for research analysis, serialised as orbConfigJson inside ResearchConfig.</summary>
public sealed class OrbConfig
{
    [JsonPropertyName("aspectBaseOrb")]
    public double AspectBaseOrb { get; init; } = 8.0;

    [JsonPropertyName("midpoint360DialOrb")]
    public double Midpoint360DialOrb { get; init; } = 1.5;

    [JsonPropertyName("midpoint90DialOrb")]
    public double Midpoint90DialOrb { get; init; } = 1.0;

    [JsonPropertyName("midpoint45DialOrb")]
    public double Midpoint45DialOrb { get; init; } = 0.5;

    [JsonPropertyName("harmonicOrb")]
    public double HarmonicOrb { get; init; } = 2.0;

    [JsonPropertyName("parallelOrb")]
    public double ParallelOrb { get; init; } = 1.0;

    [JsonPropertyName("declinationMidpointOrb")]
    public double DeclinationMidpointOrb { get; init; } = 0.75;
}
