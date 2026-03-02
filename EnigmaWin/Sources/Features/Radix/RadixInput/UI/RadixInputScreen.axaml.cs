using Avalonia.Controls;
using System.Collections.Generic;
using System.Linq;

namespace EnigmaWin.Sources.Features.Radix.RadixInput.UI;

public partial class RadixInputScreen : UserControl
{
    public IReadOnlyList<int> HourValues { get; } = Enumerable.Range(0, 24).ToList();
    public IReadOnlyList<int> DegreeValues { get; } = Enumerable.Range(0, 181).ToList();
    public IReadOnlyList<int> LatitudeDegreeValues { get; } = Enumerable.Range(0, 90).ToList();
    public IReadOnlyList<int> MinuteSecondValues { get; } = Enumerable.Range(0, 60).ToList();
    public IReadOnlyList<int> MonthValues { get; } = Enumerable.Range(1, 12).ToList();
    public IReadOnlyList<int> DayValues { get; } = Enumerable.Range(1, 31).ToList();

    public RadixInputScreen()
    {
        InitializeComponent();
    }
}
