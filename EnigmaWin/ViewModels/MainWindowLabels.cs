// MainWindowLabels.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System;
using System.Collections.Generic;
using System.Linq;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.Features.GlyphOverView;
using EnigmaWin.Sources.Features.Settings;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.ViewModels;

/// <summary>Localized texts for the main menu and navigation sidebar of MainWindow.</summary>
public sealed class MainWindowLabels
{
    public string SidebarTitle { get; }
    public string SidebarModes { get; }
    public string View1        { get; }
    public string View2        { get; }

    public string File              { get; }
    public string Exit              { get; }
    public string Settings          { get; }
    public string Analysis          { get; }
    public string Help              { get; }
    public string HelpGlyphOverview { get; }
    public string HelpAbout         { get; }

    /// <summary>Name per work mode, keyed by the name of the <see cref="AppMode"/> value.</summary>
    public IReadOnlyDictionary<string, string> Modes { get; }

    /// <summary>"Activate [mode]" per work mode, keyed by the name of the <see cref="AppMode"/> value.</summary>
    public IReadOnlyDictionary<string, string> Activate { get; }

    /// <summary>Name per sub-item, keyed by the name of the <see cref="NavSection"/> value.</summary>
    public IReadOnlyDictionary<string, string> Sections { get; }

    public MainWindowLabels(IRosetta rosetta)
    {
        string T(string key)  => rosetta.GetText(RbFile.Localizable, key);
        string Nv(string key) => rosetta.GetText(RbFile.Navigation, key);

        SidebarTitle = Nv("view.navigation.sidebar.title");
        SidebarModes = Nv("view.navigation.sidebar.modes");
        View1        = T("menu.view1");
        View2        = T("menu.view2");

        File              = Nv("view.navigation.menu.file");
        Exit              = Nv("view.navigation.menu.exit");
        Settings          = rosetta.GetText(RbFile.Settings, SettingsKeys.MenuItem);
        Analysis          = T(AppNavigation.RbKey(NavSection.RadixAnalysis));
        Help              = T("menu.help");
        HelpGlyphOverview = rosetta.GetText(RbFile.GlyphOverview, GlyphOverviewKeys.MenuItem);
        HelpAbout         = T("menu.about");

        var activateFormat = Nv("view.navigation.menu.activate");
        Modes = Enum.GetValues<AppMode>()
            .ToDictionary(mode => mode.ToString(), mode => T(AppNavigation.RbKey(mode)));
        Activate = Modes.ToDictionary(pair => pair.Key, pair => string.Format(activateFormat, pair.Value));
        Sections = Enum.GetValues<NavSection>()
            .ToDictionary(section => section.ToString(), section => T(AppNavigation.RbKey(section)));
    }
}
