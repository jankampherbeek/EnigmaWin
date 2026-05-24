// ChartWheelHelpWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using EnigmaWin.Sources.Features.Config;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.ChartDrawing.UI;

public partial class ChartWheelHelpWindow : Window
{
    public string HelpText   { get; }
    public string LabelClose { get; }

    public ChartWheelHelpWindow(IRosetta rosetta, DrawingTypes drawingType)
    {
        var helpKey = drawingType switch
        {
            DrawingTypes.HouseBased => ChartWheelKeys.HouseHelp,
            DrawingTypes.French     => ChartWheelKeys.FrenchHelp,
            DrawingTypes.Ring       => ChartWheelKeys.RingHelp,
            DrawingTypes.Dial360    => ChartWheelKeys.Dial360Help,
            DrawingTypes.Dial90     => ChartWheelKeys.Dial90Help,
            DrawingTypes.Dial45     => ChartWheelKeys.Dial45Help,
            _                       => ChartWheelKeys.ZodiacHelp,
        };

        HelpText   = rosetta.GetText(RbFile.ChartWheel, helpKey);
        LabelClose = rosetta.GetText(RbFile.ChartWheel, ChartWheelKeys.HelpClose);

        InitializeComponent();
        DataContext = this;
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();
}
