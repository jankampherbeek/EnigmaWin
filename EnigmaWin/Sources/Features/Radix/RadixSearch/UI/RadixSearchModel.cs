// RadixSearchModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnigmaWin.Sources.Data.Horoscope;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixSearch.UI;

public sealed record HoroscopeSearchRow(
    Guid   HoroscopeId,
    string Name,
    string DisplayDateTime,
    string DisplayLocation,
    double Latitude,
    double Longitude,
    double JulianDate,
    string LabelSelect,
    string LabelDelete);

public static class RadixSearchModel
{
    public static async Task<IReadOnlyList<HoroscopeSearchRow>> SearchAsync(
        string nameFragment,
        IHoroscopeRepository repository,
        IRosetta rosetta)
    {
        var all = await repository.FetchAllAsync();

        var matches = string.IsNullOrWhiteSpace(nameFragment)
            ? all
            : all.Where(h => h.Name.Contains(nameFragment, StringComparison.OrdinalIgnoreCase));

        var labelSelect = rosetta.GetText(RbFile.RadixSearch, "view.radixsearchscreen.select");
        var labelDelete = rosetta.GetText(RbFile.RadixSearch, "view.radixsearchscreen.delete");

        var rows = new List<HoroscopeSearchRow>();
        foreach (var horoscope in matches.OrderBy(h => h.Name))
        {
            var full = await repository.FetchAsync(horoscope.Id);
            if (full is null) continue;

            var preferred = full.DateTimes.FirstOrDefault(dt => dt.IsPreferred)
                         ?? full.DateTimes.FirstOrDefault();

            rows.Add(new HoroscopeSearchRow(
                HoroscopeId:     full.Id,
                Name:            full.Name,
                DisplayDateTime: preferred?.OriginalInput ?? "—",
                DisplayLocation: string.IsNullOrWhiteSpace(full.PlaceName) ? "—" : full.PlaceName,
                Latitude:        full.Latitude ?? 0.0,
                Longitude:       full.Longitude ?? 0.0,
                JulianDate:      preferred?.JulianDate ?? 0.0,
                LabelSelect:     labelSelect,
                LabelDelete:     labelDelete));
        }
        return rows;
    }

    public static FullChart CalculateChart(HoroscopeSearchRow row, FactorConfig? factorConfig = null, CalculationConfig? calculationConfig = null)
    {
        var factorsToUse = factorConfig.HasValue
            ? factorConfig.Value.Settings.Where(s => s.IsUsed).Select(s => s.Factor).ToList()
            : new List<Factors>
            {
                Factors.Sun, Factors.Moon, Factors.Mercury, Factors.Venus,
                Factors.Mars, Factors.Jupiter, Factors.Saturn, Factors.Pluto
            };

        var calcConfig = calculationConfig ?? CalculationConfig.Default;
        var request = new CalcRequest(
            julianDay: row.JulianDate,
            factorsToUse: factorsToUse,
            houseSystem: calcConfig.HouseSystem.SeId(),
            seFlags: 258,
            latitude:  row.Latitude,
            longitude: row.Longitude,
            height: 0.0,
            configData: calcConfig
        );
        return AstronCalcOrchestrator.PerformCalculation(request);
    }
}
