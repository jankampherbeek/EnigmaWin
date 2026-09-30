// MainWindowLabels.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using EnigmaWin.Sources.Features.GlyphOverView;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.ViewModels;

/// <summary>Localized texts for the main menu and navigation sidebar of MainWindow.</summary>
public sealed class MainWindowLabels
{
    public string SidebarNavigation { get; }
    public string SidebarWorkspace  { get; }
    public string SidebarActions    { get; }
    public string View1             { get; }
    public string View2             { get; }

    public string BackView1 { get; }
    public string BackView2 { get; }

    public string Radix             { get; }
    public string RadixNewChart     { get; }
    public string RadixOverview     { get; }
    public string RadixPositions    { get; }
    public string RadixSearch       { get; }
    public string RadixAnalysis     { get; }
    public string RadixDeclinations { get; }

    public string Config         { get; }
    public string ConfigOverview { get; }
    public string ConfigNew      { get; }
    public string ConfigEdit     { get; }

    public string Research         { get; }
    public string ResearchProjects { get; }

    public string Periods                  { get; }
    public string PeriodsAstronomical      { get; }
    public string PeriodsWaves             { get; }
    public string PeriodsMonthlyEphemeris  { get; }
    public string PeriodsLongTimeEphemeris { get; }
    public string PeriodsEclipses          { get; }

    public string Calculators             { get; }
    public string CalculatorsJulianDay    { get; }
    public string CalculatorsObliquity    { get; }
    public string CalculatorsSiderealTime { get; }

    public string Progressive             { get; }
    public string ProgressiveEvents       { get; }
    public string ProgressiveTransit      { get; }
    public string ProgressiveSecondary    { get; }
    public string ProgressiveSymbolic     { get; }
    public string ProgressiveLogTimeScale { get; }
    public string ProgressiveAgePoint     { get; }
    public string ProgressiveSolarReturn  { get; }
    public string ProgressivePrimDir      { get; }
    public string ProgressivePreNatal     { get; }
    public string ProgressiveCalendar     { get; }

    public string Synastry { get; }

    public string ImportExport           { get; }
    public string ImportExportEnigma     { get; }
    public string ImportExportQuickChart { get; }
    public string ImportExportAaf        { get; }

    public string Help              { get; }
    public string HelpGlyphOverview { get; }
    public string HelpAbout         { get; }

    public MainWindowLabels(IRosetta rosetta)
    {
        string T(string key) => rosetta.GetText(RbFile.Localizable, key);

        SidebarNavigation = T("menu.sidebar.navigation");
        SidebarWorkspace  = T("menu.sidebar.workspace");
        SidebarActions    = T("menu.sidebar.actions");
        View1             = T("menu.view1");
        View2             = T("menu.view2");

        BackView1 = T("menu.backview1");
        BackView2 = T("menu.backview2");

        Radix             = T("menu.radix");
        RadixNewChart     = T("menu.radix.newchart");
        RadixOverview     = T("menu.radix.overview");
        RadixPositions    = T("menu.radix.positions");
        RadixSearch       = T("menu.radix.search");
        RadixAnalysis     = T("menu.radix.analysis");
        RadixDeclinations = T("menu.radix.declinations");

        Config         = T("menu.config");
        ConfigOverview = T("menu.config.overview");
        ConfigNew      = T("menu.config.new");
        ConfigEdit     = T("menu.config.edit");

        Research         = T("menu.research");
        ResearchProjects = T("menu.research.projects");

        Periods                  = T("menu.periods");
        PeriodsAstronomical      = T("menu.cycles.astronomical");
        PeriodsWaves             = T("menu.cycles.waves");
        PeriodsMonthlyEphemeris  = T("menu.cycles.monthlyephemeris");
        PeriodsLongTimeEphemeris = T("menu.cycles.longtimeephemeris");
        PeriodsEclipses          = T("menu.cycles.eclipses");

        Calculators             = T("menu.calculators");
        CalculatorsJulianDay    = T("menu.calculators.julianday");
        CalculatorsObliquity    = T("menu.calculators.obliquity");
        CalculatorsSiderealTime = T("menu.calculators.siderealtime");

        Progressive             = T("menu.progressive");
        ProgressiveEvents       = T("menu.progressive.events");
        ProgressiveTransit      = T("menu.progressive.transit");
        ProgressiveSecondary    = T("menu.progressive.secondary");
        ProgressiveSymbolic     = T("menu.progressive.symbolic");
        ProgressiveLogTimeScale = T("menu.progressive.logtimescale");
        ProgressiveAgePoint     = T("menu.progressive.agepoint");
        ProgressiveSolarReturn  = T("menu.progressive.solarreturn");
        ProgressivePrimDir      = T("menu.progressive.primdir");
        ProgressivePreNatal     = T("menu.progressive.prenatal");
        ProgressiveCalendar     = T("menu.progressive.calendar");

        Synastry = T("menu.synastry");

        ImportExport           = T("menu.importexport");
        ImportExportEnigma     = T("menu.importexport.enigma");
        ImportExportQuickChart = T("menu.importexport.quickchart");
        ImportExportAaf        = T("menu.importexport.aaf");

        Help              = T("menu.help");
        HelpGlyphOverview = rosetta.GetText(RbFile.GlyphOverview, GlyphOverviewKeys.MenuItem);
        HelpAbout         = T("menu.about");
    }
}
