// BlaSchemaOrchestrator.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.BlaSchema;

/// <summary>Orchestrator for the BLA schema ("Invisible Luminaries Astrology") calculations, ported from
/// Enigma.Core/Slices/BlaSchema/BlaSchemaOrchestrator.cs. Precomputes point/house details once for a chart,
/// then exposes getters for every BLA table.</summary>
public sealed class BlaSchemaOrchestrator
{
    private readonly List<BlaPointDetails> _pointDetails;
    private readonly List<BlaHouseDetails> _houseDetails;
    private readonly Dictionary<int, int> _houseCounts;
    private readonly Dictionary<int, int> _signCounts;
    private readonly Dictionary<Factors, int> _planetsInSigns;
    private readonly Dictionary<Factors, int> _planetsInDecanates;
    private readonly Dictionary<Factors, int> _planetsInHouses;
    private readonly Dictionary<int, int> _signsOnCusps;
    private readonly Factors _northNode;
    private readonly Factors _southNode;

    public BlaSchemaOrchestrator(BlaChartLongitudes chart, bool useDecanates, Factors northNode, Factors southNode)
    {
        _northNode = northNode;
        _southNode = southNode;

        // Iterate the canonical order (not chart.Points directly) so results are reproducible regardless
        // of Dictionary iteration order.
        _pointDetails = chart.Points.Keys
            .OrderBy(p => (int)p)
            .Select(point => BlaDetailsFactory.CreateBlaPointDetails(point, chart))
            .ToList();

        _planetsInDecanates = new Dictionary<Factors, int>();
        if (useDecanates)
        {
            foreach (var point in _pointDetails)
            {
                _planetsInDecanates.Add(point.Point, point.Decanate);
            }
        }

        _houseDetails = [];
        for (var i = 1; i < 13; i++)
        {
            _houseDetails.Add(BlaDetailsFactory.CreateBlaHouseDetails(i, chart));
        }

        _houseCounts = BlaHousePositions.DefineHouseCounts(_pointDetails);
        _signCounts = SignPositions.DefineSignCounts(_pointDetails);
        _planetsInSigns = _pointDetails.ToDictionary(x => x.Point, x => x.Sign);
        _planetsInHouses = BlaHousePositions.DefineHousePositions(chart);
        _signsOnCusps = SignsOnCusps.DefineSignsOnCusps(chart.Cusps);
    }

    public List<BlaPointDetails> GetPointDetails() => _pointDetails;

    public List<BlaHouseDetails> GetHouseDetails() => _houseDetails;

    public Dictionary<int, int> GetSignCounts() => _signCounts;

    public Dictionary<int, int> GetHouseCounts() => _houseCounts;

    public Dictionary<Factors, int> GetPlanetsInSigns() => _planetsInSigns;

    public Dictionary<Factors, int> GetPlanetsInHouses() => _planetsInHouses;

    public Dictionary<int, int> GetSignsOnCusps() => _signsOnCusps;

    public (Dictionary<int, BlaSignHouseCountLine> Crosses, Dictionary<int, BlaSignHouseCountLine> Elements) GetCrossElementCounts()
        => CrossElementCounts.CreateCrossesElementsCounts(_signCounts, _houseCounts, _houseDetails);

    public Dictionary<int, int> GetQuadrantCounts() => QuadrantPositions.DefineQuadrants(_houseDetails);

    public List<BlaDispositorLine> GetDispositors(bool useDecanates) => Dispositors.CreateDispositors(
        _signCounts, _houseCounts, _signsOnCusps, _planetsInSigns, _planetsInHouses, _planetsInDecanates, useDecanates);

    /// <summary>Define details: rulers of asc, sisterRuler asc, clamped houses, intercepted signs, ground note, mundane house asc, sister sign cusp.</summary>
    public BlaDetailsData GetDetails() => BlaDetails.CreateDetails(_signsOnCusps, _planetsInHouses);

    public BlaCyclesData GetCycles() => BlaCycles.CreateCyclesData(_planetsInHouses, _signsOnCusps);

    public BlaCyclesData GetShortenedCycles() => BlaCycles.CreateShortenedCyclesData(_planetsInHouses, _signsOnCusps);

    // Reinforcements

    public Dictionary<Factors, int> GetPointsInOwnSign() => ReinforcementCalc.FindPointsInOwnSign(_planetsInSigns);

    public Dictionary<Factors, int> GetPointsInOwnHouse() => ReinforcementCalc.FindPointsInOwnHouse(_planetsInHouses, _signsOnCusps);

    public Dictionary<Factors, int> GetPointsInOwnMundaneHouse() => ReinforcementCalc.FindPointsInMundaneHouses(_planetsInHouses);

    public Dictionary<Factors, int> GetRulersInHouseAsSign() => ReinforcementCalc.FindRulerInHouseAsSign(_signsOnCusps, _planetsInSigns);

    public List<FactorPairAnalogHouseSign> GetFactorPairsAnalogHouseSigns()
        => ReinforcementCalc.FindFactorPairs(_planetsInSigns, _planetsInHouses, _northNode, _southNode);

    public List<Reception> GetReceptionsInSigns() => ReinforcementCalc.FindReceptionInSigns(_planetsInSigns);

    public List<Reception> GetReceptionsInHouses() => ReinforcementCalc.FindReceptionInHouses(_planetsInHouses, _signsOnCusps);

    public List<Reception> GetReceptionsInMundaneHouses() => ReinforcementCalc.FindReceptionInMundaneHouses(_planetsInHouses, _signsOnCusps);
}
