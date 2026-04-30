// NavigationParameters.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;

namespace EnigmaWin.Sources.AppShell.Navigation;

public interface INavigationParameter
{
}

public readonly record struct RadixInputNavigationParameter(Guid SessionId) : INavigationParameter;

public enum ConfigHomeMode
{
    Overview,
    New,
    Edit
}

public readonly record struct ConfigHomeNavigationParameter(ConfigHomeMode Mode) : INavigationParameter;

public enum ConfigEditorMode
{
    New,
    Edit
}

public readonly record struct ConfigEditorNavigationParameter(ConfigEditorMode Mode) : INavigationParameter;

public readonly record struct ConfigEditNavigationParameter(Guid ConfigId) : INavigationParameter;

public readonly record struct ResearchProjectConfigNavigationParameter(
    EnigmaWin.Sources.Features.Research.UI.ResearchProjectDraft Draft) : INavigationParameter;

public enum ResearchProjectListMode { All, Search }

public readonly record struct ResearchProjectOpenNavigationParameter(int ProjectId, string ProjectName) : INavigationParameter;
