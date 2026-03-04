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
