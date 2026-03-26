// RadixChartScreen.axaml.cs
// EnigmaWin
// Created by Jan Kampherbeek on 26-03-2026

using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;
using System;

namespace EnigmaWin.Sources.Features.ChartDrawing.UI;

public partial class RadixChartScreen : UserControl
{
    private readonly RadixChartViewModel _viewModel;

    public RadixChartScreen()
    {
        var app = Application.Current as App
            ?? throw new InvalidOperationException("App not available");

        var chartSession  = app.Services.GetRequiredService<IChartSession>();
        var configContext = app.Services.GetRequiredService<IConfigContext>();
        var rosetta       = app.Services.GetRequiredService<IRosetta>();

        _viewModel = new RadixChartViewModel(chartSession, configContext, rosetta);

        InitializeComponent();
        DataContext = _viewModel;
    }
}
