// EventRow.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Progressive.Events.UI;

/// <summary>Flattened row object for the events table.</summary>
public sealed class EventRow
{
    public ChartEvent Event { get; }
    public bool IsOddRow { get; }
    public bool IsSelected { get; }
    public string Title { get; }
    public string DisplayDateTime { get; }
    public string DisplayLocation { get; }
    public string LabelSelect { get; }
    public string LabelEdit { get; }
    public string LabelDelete { get; }

    public EventRow(ChartEvent chartEvent, int index, bool isSelected,
        string labelSelect, string labelEdit, string labelDelete)
    {
        Event           = chartEvent;
        IsOddRow        = index % 2 != 0;
        IsSelected      = isSelected;
        Title           = chartEvent.Title;
        DisplayDateTime = chartEvent.OriginalInput ?? $"{chartEvent.JulianDate:F4}";
        DisplayLocation = chartEvent.PlaceName ?? "–";
        LabelSelect     = labelSelect;
        LabelEdit       = labelEdit;
        LabelDelete     = labelDelete;
    }
}
