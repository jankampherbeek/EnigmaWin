using System;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Radix.RadixOverview.UI;

public sealed class SessionChartRow
{
    public Guid       Id               { get; }
    public string     Name             { get; }
    public string     DisplayJulianDay { get; }
    public string     LabelSelect      { get; }
    public string     LabelEdit        { get; }
    public NamedChart NamedChart       { get; }

    public SessionChartRow(NamedChart namedChart, string labelSelect, string labelEdit)
    {
        NamedChart       = namedChart;
        Id               = namedChart.Id;
        Name             = namedChart.Name;
        DisplayJulianDay = RadixOverviewModel.FormattedJulianDay(namedChart.Chart.JulianDay);
        LabelSelect      = labelSelect;
        LabelEdit        = labelEdit;
    }
}
