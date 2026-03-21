using Avalonia.Controls;
using Avalonia.Interactivity;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixOverview.UI;

public partial class RadixOverviewHelpWindow : Window
{
    public string LabelTitle { get; }
    public string HelpText   { get; }
    public string LabelClose { get; }

    public RadixOverviewHelpWindow(IRosetta rosetta)
    {
        LabelTitle = rosetta.GetText(RbFile.RadixOverview, "view.radixoverviewscreen.help.title");
        HelpText   = rosetta.GetText(RbFile.RadixOverview, "view.radixoverviewscreen.help.text");
        LabelClose = rosetta.GetText(RbFile.RadixOverview, "view.radixoverviewscreen.help.close");

        Title = LabelTitle;
        InitializeComponent();
        DataContext = this;
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e) => Close();
}
