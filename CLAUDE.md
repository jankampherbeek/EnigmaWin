# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

EnigmaWin is a Windows desktop astrology application built with Avalonia UI (MVVM). It calculates and displays astrological charts (radix/birth charts) using the Swiss Ephemeris native library (`swedll64.dll`) for astronomical calculations. The project is in early/active development.

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
- **Avalonia UI 11.x** — cross-platform XAML desktop UI (views are `.axaml` files)
- **CommunityToolkit.MVVM** — MVVM with source generators (`[ObservableProperty]`, `[RelayCommand]`)
- **Microsoft.Extensions.DependencyInjection** — DI container, wired in `App.axaml.cs`
- **Swiss Ephemeris** — native DLL (`swedll64.dll`) wrapped via P/Invoke in `SEWrapper`
- **Serilog** — structured logging
- **NUnit** — tests (net10.0 test project targeting net8.0 main app)

### Source Layout (`EnigmaWin/Sources/`)

**AppShell/** — Application frame
- `Navigation/` — `INavigationService`, `AppRoutes` (route name constants), `RouteViewModelFactory`
- `State/` — `IChartContext`, `IConfigContext` (global app state, injected everywhere)
- `UI/` — The three top-level workspace screens (Radix, Config, Research)

**Domain/** — Pure domain models and enums (no UI, no calculation logic): `FullChart`, `FullFactorPosition`, `HousePositions`, `Ayanamshas`, `Factors`, `HouseSystems`, etc.

**Features/** — All feature implementations
- `AstronCalc/` — Calculation engine: `SEWrapper` (P/Invoke), `AstronCalcOrchestrator` (entry point), plus specialized calculators (`ElementsCalc`, `FormulaCalc`, `LotsCalc`, `ApsidesCalc`, etc.)
- `Radix/` — Birth chart feature with sub-folders: `RadixInput/UI/`, `RadixPositions/UI/`, `RadixAnalysis/`, `RadixChart/`
- `Config/` — App configuration (`ConfigData`) and its UI screens
- `Localization/` — `Rosetta.cs` custom i18n, loads `.strings` files from `Resources/i18n/` (en, nl, de, fr)
- `Shared/` — Conversion utilities (`DateTimeConversion`, `PositionInDegreesConversion`) and validators

**ViewModels/** — Top-level ViewModels: `MainWindowViewModel` + one `*RouteViewModel` per workspace/route

**Views/** — `MainWindow.axaml` and `ViewLocator.cs` (convention: `FooViewModel` → `FooView`)

### Navigation Pattern
The app uses a custom navigation service with separate main/detail history stacks. Routes are string constants in `AppRoutes`. `RouteViewModelFactory` creates ViewModels for each route. Screens receive dependencies via constructor injection.

### Swiss Ephemeris
The `se/` directory (ephemeris data files) must be present relative to the executable. The app validates this on startup in `App.axaml.cs`. The native DLL is P/Invoked in `SEWrapper`.

## Apple Reference Project

EnigmaWin is a port of an Apple project currently in development. The Apple source is located at `E:\EnigmaApl\EnigmaApl-main\EnigmaApl-main` and can be consulted as a reference for domain logic, calculations, and feature design.

**Key UI difference:** The Apple project uses SwiftUI with a ViewModel (VM) pattern, while EnigmaWin uses Avalonia with full MVVM. When porting logic, translate SwiftUI views/VMs to Avalonia AXAML views + MVVM ViewModels accordingly.

## Key Conventions
- ViewModels live in both `Sources/Features/*/UI/` (feature VMs) and `ViewModels/` (shell/route VMs)
- AXAML code-behind files (`.axaml.cs`) are minimal — logic belongs in the ViewModel
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

<!-- rtk-instructions v2 -->
# RTK (Rust Token Killer) - Token-Optimized Commands

## Golden Rule

**Always prefix commands with `rtk`**. If RTK has a dedicated filter, it uses it. If not, it passes through unchanged. This means RTK is always safe to use.

**Important**: Even in command chains with `&&`, use `rtk`:
```bash
# ❌ Wrong
git add . && git commit -m "msg" && git push

# ✅ Correct
rtk git add . && rtk git commit -m "msg" && rtk git push
```

## RTK Commands by Workflow

### Build & Compile (80-90% savings)
```bash
rtk cargo build         # Cargo build output
rtk cargo check         # Cargo check output
rtk cargo clippy        # Clippy warnings grouped by file (80%)
rtk tsc                 # TypeScript errors grouped by file/code (83%)
rtk lint                # ESLint/Biome violations grouped (84%)
rtk prettier --check    # Files needing format only (70%)
rtk next build          # Next.js build with route metrics (87%)
```

### Test (90-99% savings)
```bash
rtk cargo test          # Cargo test failures only (90%)
rtk vitest run          # Vitest failures only (99.5%)
rtk playwright test     # Playwright failures only (94%)
rtk test <cmd>          # Generic test wrapper - failures only
```

### Git (59-80% savings)
```bash
rtk git status          # Compact status
rtk git log             # Compact log (works with all git flags)
rtk git diff            # Compact diff (80%)
rtk git show            # Compact show (80%)
rtk git add             # Ultra-compact confirmations (59%)
rtk git commit          # Ultra-compact confirmations (59%)
rtk git push            # Ultra-compact confirmations
rtk git pull            # Ultra-compact confirmations
rtk git branch          # Compact branch list
rtk git fetch           # Compact fetch
rtk git stash           # Compact stash
rtk git worktree        # Compact worktree
```

Note: Git passthrough works for ALL subcommands, even those not explicitly listed.

### GitHub (26-87% savings)
```bash
rtk gh pr view <num>    # Compact PR view (87%)
rtk gh pr checks        # Compact PR checks (79%)
rtk gh run list         # Compact workflow runs (82%)
rtk gh issue list       # Compact issue list (80%)
rtk gh api              # Compact API responses (26%)
```

### JavaScript/TypeScript Tooling (70-90% savings)
```bash
rtk pnpm list           # Compact dependency tree (70%)
rtk pnpm outdated       # Compact outdated packages (80%)
rtk pnpm install        # Compact install output (90%)
rtk npm run <script>    # Compact npm script output
rtk npx <cmd>           # Compact npx command output
rtk prisma              # Prisma without ASCII art (88%)
```

### Files & Search (60-75% savings)
```bash
rtk ls <path>           # Tree format, compact (65%)
rtk read <file>         # Code reading with filtering (60%)
rtk grep <pattern>      # Search grouped by file (75%)
rtk find <pattern>      # Find grouped by directory (70%)
```

### Analysis & Debug (70-90% savings)
```bash
rtk err <cmd>           # Filter errors only from any command
rtk log <file>          # Deduplicated logs with counts
rtk json <file>         # JSON structure without values
rtk deps                # Dependency overview
rtk env                 # Environment variables compact
rtk summary <cmd>       # Smart summary of command output
rtk diff                # Ultra-compact diffs
```

### Infrastructure (85% savings)
```bash
rtk docker ps           # Compact container list
rtk docker images       # Compact image list
rtk docker logs <c>     # Deduplicated logs
rtk kubectl get         # Compact resource list
rtk kubectl logs        # Deduplicated pod logs
```

### Network (65-70% savings)
```bash
rtk curl <url>          # Compact HTTP responses (70%)
rtk wget <url>          # Compact download output (65%)
```

### Meta Commands
```bash
rtk gain                # View token savings statistics
rtk gain --history      # View command history with savings
rtk discover            # Analyze Claude Code sessions for missed RTK usage
rtk proxy <cmd>         # Run command without filtering (for debugging)
rtk init                # Add RTK instructions to CLAUDE.md
rtk init --global       # Add RTK to ~/.claude/CLAUDE.md
```

## Token Savings Overview

| Category | Commands | Typical Savings |
|----------|----------|-----------------|
| Tests | vitest, playwright, cargo test | 90-99% |
| Build | next, tsc, lint, prettier | 70-87% |
| Git | status, log, diff, add, commit | 59-80% |
| GitHub | gh pr, gh run, gh issue | 26-87% |
| Package Managers | pnpm, npm, npx | 70-90% |
| Files | ls, read, grep, find | 60-75% |
| Infrastructure | docker, kubectl | 85% |
| Network | curl, wget | 65-70% |

Overall average: **60-90% token reduction** on common development operations.
<!-- /rtk-instructions -->
