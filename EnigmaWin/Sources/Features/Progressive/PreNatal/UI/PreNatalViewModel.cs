// PreNatalViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Data.Horoscope;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.Sources.Features.Progressive.Events;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Progressive.PreNatal.UI;

public partial class PreNatalViewModel : ObservableObject
{
    private readonly IRosetta _rosetta;

    private List<PreNatalMoment> _originalMoments = [];

    public PreNatalInputViewModel Inner { get; }

    [ObservableProperty][NotifyPropertyChangedFor(nameof(HasResults))]
    private ObservableCollection<PreNatalResultRow> _resultRows = [];

    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool   _hasError;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(HasInfoText))]
    private string _infoText = string.Empty;

    // The prenatal moment row selected in the results table (used by Combine in input VM)
    [ObservableProperty] private PreNatalResultRow? _selectedMomentRow;

    public bool HasResults  => ResultRows.Count > 0;
    public bool HasInfoText => !string.IsNullOrEmpty(InfoText);

    // Labels — results screen
    public string LabelResultsTitle    => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.results.title");
    public string LabelNoResults       => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.results.noresults");
    public string LabelColActualDate   => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.col.actualdate");
    public string LabelColType         => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.col.type");
    public string LabelColPosition1    => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.col.position1");
    public string LabelColPosition2    => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.col.position2");
    public string LabelColPrenatalDate => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.col.prenataldate");
    public string LabelResultsHelp     => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.results.help");
    public string LabelSelectMoment    => _rosetta.GetText(RbFile.PreNatal, "view.prenatal.results.selectmoment");

    public PreNatalViewModel(IChartSession chartSession, IRosetta rosetta,
        EventsOrchestrator eventsOrchestrator, IHoroscopeRepository horoscopeRepository)
    {
        _rosetta = rosetta;
        Inner    = new PreNatalInputViewModel(this, chartSession, rosetta, eventsOrchestrator, horoscopeRepository);
    }

    internal void ClearResults()
    {
        _originalMoments  = [];
        ErrorMessage      = string.Empty;
        HasError          = false;
        InfoText          = string.Empty;
        SelectedMomentRow = null;
        ResultRows.Clear();
    }

    internal void SetResults(List<PreNatalMoment> moments)
    {
        _originalMoments  = moments;
        ErrorMessage      = string.Empty;
        HasError          = false;
        InfoText          = string.Empty;
        SelectedMomentRow = null;

        if (moments.Count == 0)
        {
            ErrorMessage = _rosetta.GetText(RbFile.PreNatal, "view.prenatal.nohits");
            HasError     = true;
            ResultRows.Clear();
        }
        else
        {
            RebuildRows(moments);
        }
    }

    internal void SetError(string message)
    {
        _originalMoments  = [];
        ErrorMessage      = message;
        HasError          = true;
        SelectedMomentRow = null;
        ResultRows.Clear();
        InfoText          = string.Empty;
    }

    private void RebuildRows(List<PreNatalMoment> moments)
    {
        ResultRows = new ObservableCollection<PreNatalResultRow>(
            moments.Select((m, i) => new PreNatalResultRow(m, i, _rosetta)));

        if (moments.Count > 0)
        {
            var first = FormatDate(SEWrapper.DateFromJulianDay(moments[0].ActualJD, true));
            var last  = FormatDate(SEWrapper.DateFromJulianDay(moments[^1].ActualJD, true));
            InfoText  = $"{first} – {last}";
        }
    }

    private static string FormatDate(AstronomicalDateTime dt)
        => $"{dt.Date.Year:D4}/{dt.Date.Month:D2}/{dt.Date.Day:D2}";
}
