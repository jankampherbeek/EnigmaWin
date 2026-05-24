// RadixInputHelpWindow.xaml.cs
// EnigmaApl is open source. For more information see se_license.html and License, both at the root of the application.
// Created by Jan Kampherbeek 2026.

using System.Windows;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixInput.UI;

public partial class RadixInputHelpWindow : Window
{
    public string HeaderAbout    { get; }
    public string AboutLine1     { get; }
    public string AboutLine2     { get; }
    public string HeaderLocation { get; }
    public string LocationLine1  { get; }
    public string LocationLine2  { get; }
    public string HeaderDateTime { get; }
    public string DateTimeLine1  { get; }
    public string DateTimeLine2  { get; }
    public string DateTimeLine3  { get; }
    public string LabelClose     { get; }

    public RadixInputHelpWindow(IRosetta rosetta)
    {
        HeaderAbout    = rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.aboutchart");
        AboutLine1     = rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.help.aboutchart.line1");
        AboutLine2     = rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.help.aboutchart.line2");
        HeaderLocation = rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.location");
        LocationLine1  = rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.help.location.line1");
        LocationLine2  = rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.help.location.line2");
        HeaderDateTime = rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.datetime");
        DateTimeLine1  = rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.help.datetime.line1");
        DateTimeLine2  = rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.help.datetime.line2");
        DateTimeLine3  = rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.help.datetime.line3");
        LabelClose     = rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.help.close");

        Title = rosetta.GetText(RbFile.RadixInput, "view.radixinputscreen.help.title");

        InitializeComponent();
        DataContext = this;
    }

    private void OnCloseClicked(object sender, RoutedEventArgs e) => Close();
}
