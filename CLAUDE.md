# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

EnigmaWin is a Windows desktop astrology application built with WPF (MVVM). It calculates and displays astrological charts (radix/birth charts) using the Swiss Ephemeris native library (`swedll64.dll`) for astronomical calculations. The project is in early/active development.

## Commands

```bash
# Build the solution
dotnet build EnigmaWin.sln

# Run the application
dotnet run --project EnigmaWin/EnigmaWin.csproj

# Run all tests
dotnet test EnigmaWinTest/EnigmaWinTest.csproj

# Run a single test class
dotnet test EnigmaWinTest/EnigmaWinTest.csproj --filter "FullyQualifiedName~ClassName"
```

## Architecture

### Frameworks & Libraries
- **WPF** — Windows-only XAML desktop UI (views are `.xaml` files), targeting `net8.0-windows`
- **CommunityToolkit.MVVM** — MVVM with source generators (`[ObservableProperty]`, `[RelayCommand]`)
- **Microsoft.Extensions.DependencyInjection** — DI container, wired in `App.xaml.cs`
- **Swiss Ephemeris** — native DLL (`swedll64.dll`) wrapped via P/Invoke in `SEWrapper`
- **ScottPlot.WPF 5.x** — charting library for line charts
- **Microsoft.Web.WebView2** — embedded browser for HTML factsheets
- **Microsoft.Data.Sqlite + Dapper** — local SQLite database access
- **Serilog** — structured logging
- **NUnit** — tests

### Source Layout (`EnigmaWin/Sources/`)

**AppShell/** — Application frame
- `Navigation/` — `INavigationService`, `AppRoutes` (route name constants), `RouteViewModelFactory`
- `State/` — `IChartContext`, `IConfigContext` (global app state, injected everywhere)
- `UI/` — Top-level workspace screens (Radix, Config, Research)

**Data/** — Persistence layer
- `Db/` — Database setup and migrations
- `Event/`, `Horoscope/`, `UserConfiguration/` — Repository classes per domain area

**Domain/** — Pure domain models and enums (no UI, no calculation logic): `FullChart`, `FullFactorPosition`, `HousePositions`, `Ayanamshas`, `Factors`, `HouseSystems`, etc.

**Features/** — All feature implementations
- `AstronCalc/` — Calculation engine: `SEWrapper` (P/Invoke), `AstronCalcOrchestrator` (entry point), plus specialized calculators
- `ChartDrawing/` — Wheel drawing and chart rendering (`UI/`, `WheelDrawing/`)
- `Cycles/` — Astronomical Cycles and Waves features (`CyclesAstronomical/UI/`, `CyclesWaves/UI/`)
- `Location/` — Location lookup/selection
- `Radix/` — Birth chart feature: `RadixInput/UI/`, `RadixOverview/UI/`, `RadixEdit/UI/`, `RadixSearch/UI/`, `RadixPositions/UI/`, `RadixAnalysis/` (Aspects, Declinations, Harmonics, Midpoints)
- `Research/` — Research feature: `Analysis/`, `Inquiries/`, `Pipeline/`, `ResearchProjects/`, `UI/`
- `Speed/` — Speed-related calculations
- `Config/UI/` — App configuration screens
- `Shared/` — Conversion utilities, WPF value converters, glyph fonts, i18n (`Rosetta/`), validation

**ViewModels/** — Top-level ViewModels: `MainWindowViewModel` + per-workspace route VMs

**Views/** — `MainWindow.xaml` and per-feature XAML views

### Navigation Pattern
The app uses a custom navigation service with separate main/detail history stacks. Routes are string constants in `AppRoutes`. `RouteViewModelFactory` creates ViewModels for each route. Screens receive dependencies via constructor injection.

### Swiss Ephemeris
The `se/` directory (ephemeris data files) must be present relative to the executable. The app validates this on startup in `App.xaml.cs`. The native DLL is P/Invoked in `SEWrapper`.

## Apple Reference Project

EnigmaWin is a port of an Apple project currently in development. The Apple source is located at the disk extSSD`\dev\EnigmaAp` and can be consulted as a reference for domain logic, calculations, and feature design.

**Key UI difference:** The Apple project uses SwiftUI with a ViewModel (VM) pattern, while EnigmaWin uses WPF with full MVVM. When porting logic, translate SwiftUI views/VMs to WPF XAML views + MVVM ViewModels accordingly.

## Key Conventions
- ViewModels live in both `Sources/Features/*/UI/` (feature VMs) and `ViewModels/` (shell/route VMs)
- XAML code-behind files (`.xaml.cs`) are minimal — logic belongs in the ViewModel
- All localized strings go through `Rosetta` — no hardcoded UI strings
- `IChartContext` and `IConfigContext` are the global state containers; inject these rather than passing state through constructors

## Copyright Header
Every new C# (`.cs`) file must start with the following three-line comment block, with `[Filename]` replaced by the actual filename and `[year]` by the current year:

```
// [Filename]
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek [year].
```

Replace any existing copyright/file-header comment block at the top of the file with this format.

