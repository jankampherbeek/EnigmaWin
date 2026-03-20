using Avalonia.Controls;
using Avalonia.Interactivity;
using EnigmaWin.Sources.Features.Shared.I18n.Rosetta;

namespace EnigmaWin.Sources.Features.Radix.RadixSearch.UI;

public partial class RadixSearchHelpWindow : Window
{
    public string LabelTitle { get; }
    public string HelpText   { get; }
    public string LabelClose { get; }

    public RadixSearchHelpWindow(IRosetta rosetta)
    {
        LabelTitle = rosetta.GetText(RbFile.RadixSearch, "view.radixsearchscreen.help.title");
        HelpText   = rosetta.GetText(RbFile.RadixSearch, "view.radixsearchscreen.help.text");
        LabelClose = rosetta.GetText(RbFile.RadixSearch, "view.radixsearchscreen.help.close");

        Title = LabelTitle;
        InitializeComponent();
        DataContext = this;
    }

    private void OnCloseClicked(object? sender, RoutedEventArgs e) => Close();
}
