// ProgressiveCalendarInputViewModel.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

namespace EnigmaWin.Sources.Features.Progressive.ProgressiveCalendar.UI;

public sealed class ProgressiveCalendarInputViewModel(ProgressiveCalendarViewModel inner)
{
    public ProgressiveCalendarViewModel Inner { get; } = inner;
}
