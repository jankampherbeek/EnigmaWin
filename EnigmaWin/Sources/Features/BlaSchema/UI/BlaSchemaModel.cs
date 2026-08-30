// BlaSchemaModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Shared.Conversion;
using EnigmaWin.Sources.Features.Shared.Glyphs;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.BlaSchema.UI;

/// <summary>Recomputes a chart with the BLA schema's own fixed point set and house system, then shapes the
/// calculation engine's output into rows ready for grid binding.</summary>
public sealed class BlaSchemaModel
{
    public sealed record PositionRow(string FactorGlyph, string FactorName, string PositionText, string SignGlyph, int House, int Decanate, bool IsEvenRow);

    public sealed record HousePositionRow(int Number, string PositionText, string SignGlyph, int Decanate, bool IsEvenRow);

    public sealed record CountRow(string Name, int Sign, int House, int Sum, int HCusp, int Total, bool IsEvenRow);

    public sealed record QuadrantRow(string Name, int Count, bool IsEvenRow);

    public sealed record DispositorRow(
        string MainRulerGlyph, string SubRulerGlyph, string RulerPairName,
        int SignSplit, int SignMain, int SignIndirect, int SignSum,
        int HouseMain, int HouseIndirect, int HouseSum,
        int DecanateDirect, int Total, bool IsEvenRow);

    public sealed record DetailRow(string Name, string Text, bool IsEvenRow);

    public sealed record CycleRow(string ElemCross, string Description, bool IsEvenRow);

    public sealed record ReinforcementRow(string FactorGlyph, string Description, string PositionText, bool IsEvenRow);

    public sealed record PairAnalogRow(string Factor1Glyph, string SignGlyph, string Factor2Glyph, int House, bool IsEvenRow);

    public sealed record ReceptionRow(string Factor1Glyph, string Position1Text, string Factor2Glyph, string Position2Text, bool IsEvenRow);

    public sealed record Result(
        List<PositionRow> Positions,
        List<HousePositionRow> HousePositions,
        List<CountRow> ElementsCounts,
        List<CountRow> CrossesCounts,
        List<QuadrantRow> QuadrantCounts,
        List<DispositorRow> Dispositors,
        List<DetailRow> Details,
        List<CycleRow> Cycles,
        List<CycleRow> ShortenedCycles,
        List<ReinforcementRow> FactorsInOwnSigns,
        List<ReinforcementRow> FactorsInOwnHouses,
        List<ReinforcementRow> FactorsInOwnMundaneHouses,
        List<ReinforcementRow> HouseLordsInAnalogSigns,
        List<PairAnalogRow> PairsAnalogHouseSign,
        List<ReceptionRow> ReceptionsInSigns,
        List<ReceptionRow> ReceptionsInHouses,
        List<ReceptionRow> ReceptionsInMundaneHouses);

    public Result Calculate(
        NamedChart namedChart,
        IConfigContext configContext,
        IRosetta rosetta,
        HouseSystems houseSystem,
        BlaApogeeCorrectionType correctionType,
        bool useTrueNode,
        bool useChiron,
        bool useCeres,
        bool useDecanates)
    {
        var northNode = useTrueNode ? Factors.NorthNodeTrue : Factors.NorthNodeMean;
        var southNode = useTrueNode ? Factors.SouthNodeTrue : Factors.SouthNodeMean;

        var requestedFactors = BlaSchemaDomain.BuildRequestedFactors(useTrueNode, useChiron, useCeres, correctionType);
        var baseCalcConfig = configContext.ActiveConfig.CalculationConfig;
        var calcConfig = baseCalcConfig with { HouseSystem = houseSystem };

        var calcRequest = new CalcRequest(
            julianDay: namedChart.Chart.JulianDay,
            factorsToUse: requestedFactors,
            houseSystem: houseSystem.SeId(),
            seFlags: 258,
            latitude: namedChart.Latitude,
            longitude: namedChart.Longitude,
            height: namedChart.Height,
            configData: calcConfig);

        var chart = AstronCalcOrchestrator.PerformCalculation(calcRequest);

        var points = new Dictionary<Factors, double>();
        foreach (var factor in requestedFactors)
        {
            if (chart.Coordinates.TryGetValue(factor, out var pos) && pos.Ecliptical.Length > 0)
            {
                points[factor] = pos.Ecliptical[0].MainPos;
            }
        }
        points[Factors.Ascendant] = chart.HousePositions.Ascendant.Longitude;
        points[Factors.Mc] = chart.HousePositions.Midheaven.Longitude;

        var cusps = new Dictionary<int, double>();
        for (var i = 0; i < chart.HousePositions.Cusps.Length; i++)
        {
            cusps[i + 1] = chart.HousePositions.Cusps[i].Longitude;
        }

        var chartLongitudes = new BlaChartLongitudes(points, cusps);
        var orchestrator = new BlaSchemaOrchestrator(chartLongitudes, useDecanates, northNode, southNode);

        var signCounts = orchestrator.GetSignCounts();
        var houseCounts = orchestrator.GetHouseCounts();
        var planetsInHouses = orchestrator.GetPlanetsInHouses();
        var signsOnCusps = orchestrator.GetSignsOnCusps();
        var (crosses, elements) = orchestrator.GetCrossElementCounts();

        return new Result(
            Positions: BuildPositionRows(orchestrator.GetPointDetails(), rosetta),
            HousePositions: BuildHousePositionRows(orchestrator.GetHouseDetails()),
            ElementsCounts: BuildCountRows(elements, ["fire", "earth", "air", "water"], rosetta),
            CrossesCounts: BuildCountRows(crosses, ["cardinal", "fixed", "mutable"], rosetta),
            QuadrantCounts: BuildQuadrantRows(orchestrator.GetQuadrantCounts(), rosetta),
            Dispositors: BuildDispositorRows(orchestrator.GetDispositors(useDecanates), rosetta),
            Details: BuildDetailRows(orchestrator.GetDetails(), rosetta),
            Cycles: BuildCycleRows(orchestrator.GetCycles(), rosetta),
            ShortenedCycles: BuildCycleRows(orchestrator.GetShortenedCycles(), rosetta),
            FactorsInOwnSigns: BuildReinforcementRows(orchestrator.GetPointsInOwnSign(), rosetta, isHouse: false),
            FactorsInOwnHouses: BuildReinforcementRows(orchestrator.GetPointsInOwnHouse(), rosetta, isHouse: true),
            FactorsInOwnMundaneHouses: BuildReinforcementRows(orchestrator.GetPointsInOwnMundaneHouse(), rosetta, isHouse: true),
            HouseLordsInAnalogSigns: BuildReinforcementRows(orchestrator.GetRulersInHouseAsSign(), rosetta, isHouse: false),
            PairsAnalogHouseSign: BuildPairAnalogRows(orchestrator.GetFactorPairsAnalogHouseSigns()),
            ReceptionsInSigns: BuildReceptionRows(orchestrator.GetReceptionsInSigns(), isHouse: false),
            ReceptionsInHouses: BuildReceptionRows(orchestrator.GetReceptionsInHouses(), isHouse: true),
            ReceptionsInMundaneHouses: BuildReceptionRows(orchestrator.GetReceptionsInMundaneHouses(), isHouse: true));
    }

    private static string FactorName(Factors factor, IRosetta rosetta) => rosetta.GetText(RbFile.Localizable, factor.LocalizedName());

    private static string SignGlyph(int signIndex) => GlyphSelector.GetGlyphForSign((Signs)signIndex);

    private static string PositionText(double longitude) => PositionInDegreesConversion.DoubleToDms(longitude % 30.0);

    private static List<PositionRow> BuildPositionRows(List<BlaPointDetails> details, IRosetta rosetta)
    {
        var rows = new List<PositionRow>();
        var i = 0;
        foreach (var detail in details)
        {
            rows.Add(new PositionRow(
                GlyphSelector.GetGlyphForFactor(detail.Point),
                FactorName(detail.Point, rosetta),
                PositionText(detail.Longitude),
                SignGlyph(detail.Sign),
                detail.House,
                detail.Decanate,
                i++ % 2 == 0));
        }
        return rows;
    }

    private static List<HousePositionRow> BuildHousePositionRows(List<BlaHouseDetails> details)
    {
        var rows = new List<HousePositionRow>();
        var i = 0;
        foreach (var detail in details)
        {
            rows.Add(new HousePositionRow(
                detail.HouseNr,
                PositionText(detail.Longitude),
                SignGlyph(detail.SignOnCusp),
                detail.Decanate,
                i++ % 2 == 0));
        }
        return rows;
    }

    private static List<CountRow> BuildCountRows(Dictionary<int, BlaSignHouseCountLine> counts, string[] labelKeys, IRosetta rosetta)
    {
        var rows = new List<CountRow>();
        var i = 0;
        foreach (var key in labelKeys)
        {
            var index = i + 1;
            if (!counts.TryGetValue(index, out var line)) { i++; continue; }
            rows.Add(new CountRow(
                rosetta.GetText(RbFile.RadixBlaSchema, $"blaschema.label.{key}"),
                line.Sign, line.House, line.Sum, line.HCusp, line.Total,
                i % 2 == 0));
            i++;
        }
        return rows;
    }

    private static List<QuadrantRow> BuildQuadrantRows(Dictionary<int, int> quadrantCounts, IRosetta rosetta)
    {
        var rows = new List<QuadrantRow>();
        for (var q = 1; q <= 4; q++)
        {
            rows.Add(new QuadrantRow(
                rosetta.GetText(RbFile.RadixBlaSchema, $"blaschema.label.quadrant{q}"),
                quadrantCounts.GetValueOrDefault(q, 0),
                (q - 1) % 2 == 0));
        }
        return rows;
    }

    private static List<DispositorRow> BuildDispositorRows(List<BlaDispositorLine> lines, IRosetta rosetta)
    {
        var rows = new List<DispositorRow>();
        var i = 0;
        foreach (var line in lines)
        {
            rows.Add(new DispositorRow(
                GlyphSelector.GetGlyphForFactor(line.MainRuler),
                GlyphSelector.GetGlyphForFactor(line.SubRuler),
                $"{FactorName(line.MainRuler, rosetta)}/{FactorName(line.SubRuler, rosetta)}",
                line.MainRulerSignCount + line.SubRulerSignCount,
                line.SumRulerSignCount,
                line.IndirectRulerSignCount,
                line.TotalRulerSignCount,
                line.DirectRulerHouseCount,
                line.IndirectRulerHouseCount,
                line.SumRulerHouseCount,
                line.DirectRulerDecanateCount,
                line.Total,
                i++ % 2 == 0));
        }
        return rows;
    }

    private static List<DetailRow> BuildDetailRows(BlaDetailsData details, IRosetta rosetta)
    {
        string T(string key) => rosetta.GetText(RbFile.RadixBlaSchema, key);
        var rows = new List<DetailRow>
        {
            new(T("blaschema.details.sistersignasc"), SignGlyph(details.SisterSignAsc), true),
            new(T("blaschema.details.clampedhouses"), string.Join(", ", details.ClampedHouses), false),
            new(T("blaschema.details.interceptedsigns"), string.Join(" ", details.InterceptedSigns.ConvertAll(s => SignGlyph(s))), true),
            new(T("blaschema.details.groundnote"), string.Join(", ", details.GroundNote), false),
            new(T("blaschema.details.lordascinhouses"), string.Join(", ", details.LordAscInHouses), true),
            new(T("blaschema.details.mooninhouse"), details.MoonInHouse.ToString(), false)
        };
        return rows;
    }

    private static List<CycleRow> BuildCycleRows(BlaCyclesData cycles, IRosetta rosetta)
    {
        string T(string key) => rosetta.GetText(RbFile.RadixBlaSchema, key);
        var groups = new (string Key, List<(int, int)> Items)[]
        {
            ("cardinal", cycles.Cardinal), ("fixed", cycles.Fixed), ("mutable", cycles.Mutable),
            ("fire", cycles.Fire), ("earth", cycles.Earth), ("air", cycles.Air), ("water", cycles.Water)
        };

        var rows = new List<CycleRow>();
        var i = 0;
        foreach (var (key, items) in groups)
        {
            var description = items.Count == 0
                ? "—"
                : string.Join(", ", items.ConvertAll(pair => $"{pair.Item1}→{pair.Item2}"));
            rows.Add(new CycleRow(T($"blaschema.label.{key}"), description, i++ % 2 == 0));
        }
        return rows;
    }

    private static List<ReinforcementRow> BuildReinforcementRows(Dictionary<Factors, int> factors, IRosetta rosetta, bool isHouse)
    {
        var rows = new List<ReinforcementRow>();
        var i = 0;
        foreach (var (factor, value) in factors)
        {
            rows.Add(new ReinforcementRow(
                GlyphSelector.GetGlyphForFactor(factor),
                FactorName(factor, rosetta),
                isHouse ? value.ToString() : SignGlyph(value),
                i++ % 2 == 0));
        }
        return rows;
    }

    private static List<PairAnalogRow> BuildPairAnalogRows(List<FactorPairAnalogHouseSign> pairs)
    {
        var rows = new List<PairAnalogRow>();
        var i = 0;
        foreach (var pair in pairs)
        {
            rows.Add(new PairAnalogRow(
                GlyphSelector.GetGlyphForFactor(pair.PointInSign),
                SignGlyph(pair.Sign),
                GlyphSelector.GetGlyphForFactor(pair.PointInHouse),
                pair.House,
                i++ % 2 == 0));
        }
        return rows;
    }

    private static List<ReceptionRow> BuildReceptionRows(List<Reception> receptions, bool isHouse)
    {
        var rows = new List<ReceptionRow>();
        var i = 0;
        foreach (var reception in receptions)
        {
            rows.Add(new ReceptionRow(
                GlyphSelector.GetGlyphForFactor(reception.Point1),
                isHouse ? reception.SignOrHouse1.ToString() : SignGlyph(reception.SignOrHouse1),
                GlyphSelector.GetGlyphForFactor(reception.Point2),
                isHouse ? reception.SignOrHouse2.ToString() : SignGlyph(reception.SignOrHouse2),
                i++ % 2 == 0));
        }
        return rows;
    }
}
