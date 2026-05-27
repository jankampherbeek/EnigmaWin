// IProgressiveSession.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.ComponentModel;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.AppShell.State;

public interface IProgressiveSession : INotifyPropertyChanged
{
    ChartEvent? SelectedEvent { get; }
    void Select(ChartEvent chartEvent);
    void ClearIfDeleted(Guid eventId);
    void ClearSelection();
}
