// IChartSession.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Collections.Generic;
using System.ComponentModel;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.AppShell.State;

public interface IChartSession : INotifyPropertyChanged
{
    IReadOnlyList<NamedChart> Charts { get; }
    NamedChart? Selected { get; }
    FullChart? SelectedChart { get; }
    Horoscope? EditingHoroscope { get; set; }
    NamedChart? EditingNamedChart { get; set; }

    void Add(string name, FullChart chart);
    void Select(NamedChart namedChart);
    void Replace(NamedChart old, string newName, FullChart newChart);
    void Remove(NamedChart namedChart);
    void RemoveByName(string name);
    void Clear();
}
