// ProgressiveEventNavigationParameters.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Progressive.Events.UI;

public sealed record ProgressiveEventNavigationParameter(Guid HoroscopeId) : INavigationParameter;
public sealed record ProgressiveEventEditNavigationParameter(ChartEvent Event) : INavigationParameter;
