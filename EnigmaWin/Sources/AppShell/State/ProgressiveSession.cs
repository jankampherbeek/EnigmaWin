// ProgressiveSession.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.AppShell.State;

public sealed partial class ProgressiveSession : ObservableObject, IProgressiveSession
{
    [ObservableProperty]
    private ChartEvent? _selectedEvent;

    public void Select(ChartEvent chartEvent) => SelectedEvent = chartEvent;

    public void ClearIfDeleted(Guid eventId)
    {
        if (SelectedEvent?.Id == eventId)
            SelectedEvent = null;
    }

    public void ClearSelection() => SelectedEvent = null;
}
