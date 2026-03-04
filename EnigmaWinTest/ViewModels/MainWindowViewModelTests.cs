using System.Collections.Generic;
using EnigmaWin.Sources.AppShell.Navigation;
using EnigmaWin.Sources.AppShell.State;
using EnigmaWin.Sources.Domain;
using EnigmaWin.Sources.Features.AstronCalc;
using EnigmaWin.ViewModels;

namespace EnigmaWintest.ViewModels;

[TestFixture]
public class MainWindowViewModelTests
{
    [Test]
    public void ShowRadixPositionsFromCalculation_UpdatesChartContextAndNavigatesToPositions()
    {
        var navigationService = new NavigationService();
        var chartContext = new ChartContext();
        var configContext = new ConfigContext();
        var routeFactory = new RouteViewModelFactory(navigationService, chartContext, configContext);
        var viewModel = new MainWindowViewModel(navigationService, chartContext, routeFactory);
        var chart = CreateChart();

        viewModel.ShowRadixPositionsFromCalculation(chart);

        Assert.That(chartContext.CurrentChart, Is.SameAs(chart));
        Assert.That(navigationService.CurrentDetailRoute, Is.EqualTo(AppRoutes.RadixPositions));
        Assert.That(viewModel.CurrentDetailViewModel, Is.InstanceOf<EnigmaWin.ViewModels.Routes.RadixPositionsRouteViewModel>());
        Assert.That(viewModel.ShowView2Placeholder, Is.False);
    }

    private static FullChart CreateChart()
    {
        var coordinates = new Dictionary<Factors, FullFactorPosition>
        {
            [Factors.Sun] = new FullFactorPosition(
                Ecliptical: [new MainAstronomicalPosition(10.0, 1.0, 1.0)],
                Equatorial: [new MainAstronomicalPosition(20.0, 2.0, 1.0)],
                Horizontal: [new HorizontalPosition(30.0, 3.0)])
        };

        var cusp = new FullCuspPosition(
            Longitude: 1.0,
            RightAscension: 2.0,
            Declination: 3.0,
            Horizontal: new HorizontalPosition(4.0, 5.0));

        var houses = new HousePositions(
            Cusps: [cusp],
            Ascendant: cusp,
            Midheaven: cusp,
            Eastpoint: cusp,
            Vertex: cusp);

        return new FullChart(
            Coordinates: coordinates,
            HousePositions: houses,
            SiderealTime: 0.0,
            JulianDay: 0.0,
            Obliquity: 0.0);
    }
}
