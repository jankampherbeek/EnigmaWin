using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EnigmaWin.Sources.Domain;

namespace EnigmaWin.Sources.Features.Radix.RadixPositions.UI;

public sealed class RadixPositionsViewModel : INotifyPropertyChanged
{
    private readonly RadixPositionsModel _model = new();
    private bool _hasData;

    public ObservableCollection<RadixPositionsModel.PlanetPositionRow> PlanetRows { get; } = [];
    public ObservableCollection<RadixPositionsModel.CuspPositionRow> CuspRows { get; } = [];
    public bool HasData
    {
        get => _hasData;
        private set
        {
            if (_hasData == value) return;
            _hasData = value;
            OnPropertyChanged(nameof(HasData));
            OnPropertyChanged(nameof(HasNoData));
        }
    }

    public bool HasNoData => !HasData;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void LoadChart(FullChart? chart)
    {
        PlanetRows.Clear();
        CuspRows.Clear();

        if (chart == null)
        {
            HasData = false;
            return;
        }

        var (planetRows, cuspRows) = _model.BuildRows(chart);
        foreach (var row in planetRows)
        {
            PlanetRows.Add(row);
        }

        foreach (var row in cuspRows)
        {
            CuspRows.Add(row);
        }

        HasData = PlanetRows.Count > 0 || CuspRows.Count > 0;
        OnPropertyChanged(nameof(PlanetRows));
        OnPropertyChanged(nameof(CuspRows));
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
